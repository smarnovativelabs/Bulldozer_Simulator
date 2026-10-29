using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Bulldozer.Player;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Bulldozer.Services
{
    public sealed class QuarryStore : MonoBehaviour
    {
        public QuarryServiceSettings settings;
        public static QuarryStore Instance { get; private set; }
        public string Status { get; private set; } = "Store prepared — initialization not requested";
        public bool InitializationRequested { get; private set; }
        public bool Ready { get; private set; }
        public bool Busy { get; private set; }
        public event Action Changed;
        StoreController store;
        StoreReceiptVerifier verifier;
        readonly HashSet<string> fulfilling = new HashSet<string>();
        readonly HashSet<string> acknowledged = new HashSet<string>();
        float operationStarted, nextRetry;
        bool alive, connected, fetching;
        Task connection;
        string removeId, cashId;
        string activeProduct, activeTransaction;
        int cashAmount;
        MinerEquipment Equipment => FindFirstObjectByType<MinerEquipment>();
        public bool AdsRemoved => Equipment && Equipment.Snapshot.removeAds;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void ResetInstance() { Instance = null; }
        void Awake() { if (Instance && Instance != this) { enabled = false; return; } Instance = this; alive = true; }
        // Only the explicit service-owner entry point may start purchasing.
        public void Initialize()
        {
            if (InitializationRequested) return;
            InitializationRequested = true;
            Retry();
        }
        public async void Retry()
        {
            if (!InitializationRequested) return;
            if (Busy || Time.unscaledTime < nextRetry || !settings) return;
            nextRetry = Time.unscaledTime + 3;
            if (string.IsNullOrWhiteSpace(settings.RemoveAdsId) || string.IsNullOrWhiteSpace(settings.CashPackId) || settings.RemoveAdsId == settings.CashPackId || settings.cashPackAmount <= 0) { SetStatus("Store setup pending — product IDs required"); return; }
            if (Application.internetReachability == NetworkReachability.NotReachable) { SetStatus("Offline — mining remains available"); return; }
            if (store != null) { if (connected) Fetch(); else await Connect(); return; }
            removeId = settings.RemoveAdsId; cashId = settings.CashPackId; cashAmount = settings.cashPackAmount;
            verifier = new StoreReceiptVerifier(settings);
            if (!verifier.CanValidate) { SetStatus(verifier.SetupError ?? "Store verification setup pending"); return; }
#if UNITY_EDITOR
            store = UnityIAPServices.StoreController("fake");
#else
            store = UnityIAPServices.StoreController();
#endif
            store.ProcessPendingOrdersOnPurchasesFetched(false);
            store.OnStoreConnected += Connected;
            store.OnStoreDisconnected += Disconnected;
            store.OnProductsFetched += ProductsFetched;
            store.OnProductsFetchFailed += ProductsFailed;
            store.OnPurchasesFetched += PurchasesFetched;
            store.OnPurchasesFetchFailed += PurchasesFailed;
            store.OnPurchasePending += Pending;
            store.OnPurchaseConfirmed += Confirmed;
            store.OnPurchaseFailed += Failed;
            store.OnPurchaseDeferred += Deferred;
            await Connect();
        }
        async Task Connect() { if(connection != null && !connection.IsCompleted)return; SetBusy("Connecting to store…"); try { connection = store.Connect(); await connection; } catch (Exception) { if (alive) { Ready = false; Finish("Store unavailable — retry when online"); } } }
        void Connected() { if (!alive) return; connected = true; Fetch(); }
        void Fetch() { if (fetching) return; fetching = true; SetBusy("Loading store prices…"); store.FetchProducts(new List<ProductDefinition> { new ProductDefinition(removeId, ProductType.NonConsumable), new ProductDefinition(cashId, ProductType.Consumable) }); }
        void Disconnected(StoreConnectionFailureDescription _) { connected = false; Ready = false; fetching = false; Finish("Store disconnected — retry"); }
        void ProductsFetched(List<Product> _) { fetching = false; Ready = true; SetBusy("Checking existing purchases…"); store.FetchPurchases(); }
        void ProductsFailed(ProductFetchFailed _) { fetching = false; Ready = false; Finish("Products unavailable — check store setup"); }
        void PurchasesFailed(PurchasesFetchFailureDescription _) { Finish("Purchase lookup failed — retry to recover pending orders"); }
        async void PurchasesFetched(Orders orders)
        {
            bool ok = true;
            foreach (var order in orders.ConfirmedOrders)
                if (ProductId(order) == removeId) ok &= await Fulfill(order, false);
            foreach (var order in orders.PendingOrders) ok &= await Fulfill(order, true);
            if (alive) Finish(ok ? "Store ready" : "Purchase recovery pending — retry");
        }
        public string Price(bool removeAds) => removeAds && AdsRemoved ? "OWNED" : store?.GetProductById(removeAds ? removeId : cashId)?.metadata.localizedPriceString ?? "SETUP PENDING";
        public bool CanBuy(bool removeAds) => Ready && !Busy && !(removeAds && AdsRemoved) && store?.GetProductById(removeAds ? removeId : cashId)?.availableToPurchase == true;
        public void Buy(bool removeAds)
        {
            if (!CanBuy(removeAds)) return;
            if (Application.internetReachability == NetworkReachability.NotReachable) { Finish("Internet connection needed for purchases"); return; }
            SetBusy("Waiting for the store…");
            activeProduct = removeAds ? removeId : cashId;
            try { store.PurchaseProduct(store.GetProductById(removeAds ? removeId : cashId)); }
            catch (Exception) { Finish("Could not open store — please retry"); }
        }
        public void Restore()
        {
            if (!Ready || Busy) return;
            SetBusy("Restoring Remove Ads…");
            store.RestoreTransactions((success, _) => { if (!alive) return; if (success) store.FetchPurchases(); else Finish("Restore failed — please retry"); });
        }
        async void Pending(PendingOrder order)
        {
            bool ownsOperation = activeProduct == ProductId(order);
            if (ownsOperation) activeTransaction = order.Info.TransactionID;
            bool ok = await Fulfill(order, true);
            // A successful pending order stays busy until its own confirmation arrives.
            // Late confirmations must never unlock a newer checkout or a restore operation.
            if (alive && !ok && ownsOperation && activeTransaction == order.Info.TransactionID)
                Finish("Purchase pending — verification or save could not finish");
        }
        async Task<bool> Fulfill(Order order, bool confirm)
        {
            string id = ProductId(order), transaction = order?.Info?.TransactionID;
            if (!alive || string.IsNullOrWhiteSpace(transaction) || (id != removeId && id != cashId)) return false;
            // Only pending consumables may grant. Confirmed history never refills cash.
            if (!confirm && id == cashId) return true;
            if (!fulfilling.Add(transaction)) return false;
            try {
                string key = await verifier.VerifyAsync(order, id);
                if (!alive || key == null) return false;
                var equipment = Equipment;
                // CommonUI can connect while the loading scene is still active.
                // Keep verified orders pending until the gameplay wallet exists.
                for(int wait=0; alive && !equipment && wait<150; wait++) { await Task.Delay(100); equipment=Equipment; }
                if(!alive)return false;
                if (!equipment || !QuarryPurchaseGrant.Prepare(equipment.Snapshot, key, id == cashId ? cashAmount : 0, id == removeId, out var next) || !equipment.Commit(next)) return false;
                Changed?.Invoke();
                if (confirm && order is PendingOrder pending && acknowledged.Add(transaction)) {
                    try { store.ConfirmPurchase(pending); }
                    catch { acknowledged.Remove(transaction); throw; }
                }
                return true;
            } catch (Exception) { return false; }
            finally { fulfilling.Remove(transaction); }
        }
        static string ProductId(Order order) { var items = order?.CartOrdered?.Items(); return items != null && items.Count == 1 && items[0].Quantity == 1 ? items[0].Product?.definition.id : null; }
        void Confirmed(Order order)
        {
            if(order is FailedOrder) acknowledged.Remove(order.Info.TransactionID);
            if(activeTransaction != order.Info.TransactionID || activeProduct != ProductId(order)) return;
            Finish(order is FailedOrder ? "Saved purchase awaits store acknowledgement — retry" : "Purchase complete");
        }
        void Failed(FailedOrder order) { if(activeProduct == ProductId(order)) Finish("Purchase cancelled or unavailable. No new reward granted."); }
        void Deferred(DeferredOrder order) { if(activeProduct == ProductId(order)) Finish("Purchase awaiting approval — reward arrives after approval"); }
        void SetBusy(string message) { activeProduct=null;activeTransaction=null;Busy = true; operationStarted = Time.unscaledTime; SetStatus(message); }
        void Finish(string message) { activeProduct=null;activeTransaction=null;Busy = false; SetStatus(message); }
        void SetStatus(string message) { Status = message; Changed?.Invoke(); }
        void Update() { if (Busy && Time.unscaledTime - operationStarted > 60) { fetching = false; Finish("Store response delayed — retry to recover purchases"); } }
        internal static string Hash(string value) { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", ""); }
        void OnDestroy()
        {
            alive = false; verifier?.Dispose(); if (Instance == this) Instance = null;
            if (store == null) return;
            store.OnStoreConnected -= Connected; store.OnStoreDisconnected -= Disconnected;
            store.OnProductsFetched -= ProductsFetched; store.OnProductsFetchFailed -= ProductsFailed;
            store.OnPurchasesFetched -= PurchasesFetched; store.OnPurchasesFetchFailed -= PurchasesFailed;
            store.OnPurchasePending -= Pending; store.OnPurchaseConfirmed -= Confirmed;
            store.OnPurchaseFailed -= Failed; store.OnPurchaseDeferred -= Deferred;
        }
    }
}
