#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using Bulldozer.Player.Persistence;
using UnityEngine.Purchasing;

namespace Bulldozer.Services
{
    public static class QuarryServiceChecks
    {
        public static string Run()
        {
            int passed = 0;
            void Check(bool ok, string message) { if (!ok) throw new Exception(message); passed++; }
            var initial = MiningSnapshot.Create(0, 10);
            Check(QuarryPurchaseGrant.Prepare(initial, "store:verified", 1000, false, out var granted) && granted.coins == 1010, "Verified cash grant");
            Check(QuarryPurchaseGrant.Prepare(granted, "store:verified", 1000, false, out var duplicate) && duplicate.coins == 1010, "Repeated purchase must not duplicate cash");
            Check(QuarryPurchaseGrant.Prepare(duplicate, "ad:completed", 50, false, out var reward) && reward.coins == 1060, "Reward completion grants cash");
            Check(QuarryPurchaseGrant.Prepare(reward, "ad:completed", 50, false, out var repeatReward) && repeatReward.coins == 1060, "Repeated ad callback must not duplicate cash");
            Check(QuarryPurchaseGrant.Prepare(reward, "store:remove", 0, true, out var remove) && remove.removeAds && remove.coins == 1060, "Remove Ads does not change cash");
            Check(!QuarryPurchaseGrant.Prepare(initial, "", 1000, false, out _), "Reject missing transaction");
            var overflow = MiningSnapshot.Create(0, int.MaxValue);
            Check(!QuarryPurchaseGrant.Prepare(overflow, "store:overflow", 1, false, out _), "Reject overflowing cash");

            var go = new GameObject("Consent recovery test");
            try {
                var consent = go.AddComponent<QuarryConsent>();
                int changes = 0; consent.Changed += () => changes++;
                int first = consent.BeginRequest(0, 30); consent.CheckTimeout(31);
                Check(!consent.Busy && consent.Attempted && changes == 1 && !consent.EditorBypass, "Timeout releases consent request without manufacturing permission");
                int next = consent.BeginRequest(32, 120);
                consent.CompleteRequest(first, null);
                Check(consent.Busy && changes == 1, "Late consent callback cannot finish a newer request");
                consent.CompleteRequest(next, null); consent.CompleteRequest(next, null);
                Check(!consent.Busy && changes == 2, "Consent completes only once");
            } finally { UnityEngine.Object.DestroyImmediate(go); }

            string dir = Path.Combine(Path.GetTempPath(), "quarry-services-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try {
                string path = Path.Combine(dir, "queue.json");
                var box = new TelemetryOutbox();
                Check(box.JournalPurchase(new TelemetryMessage { kind = "purchase", transaction = "one", product = "cash" }, path), "Purchase revenue journal persisted");
                var loaded = TelemetryOutbox.Load(path);
                Check(loaded.messages.Count == 1 && !loaded.Add(new TelemetryMessage { kind = "purchase", transaction = "one" }), "Journal survives restart and rejects duplicate");
                int firebaseCalls = 0, tenjinCalls = 0;
                loaded.Drain(m => { firebaseCalls++; return true; }, m => false);
                loaded.Drain(m => { firebaseCalls++; return true; }, m => { tenjinCalls++; return true; });
                Check(firebaseCalls == 1 && tenjinCalls == 1 && loaded.messages.Count == 0, "Telemetry destinations retry independently");
                for (int i = 0; i < 300; i++) loaded.Add(new TelemetryMessage { kind = "event", name = "event" });
                Check(loaded.messages.Count == 256, "Gameplay telemetry queue is bounded");
                var progress = new MiningProgressStore(Path.Combine(dir, "save.json"));
                Check(progress.TryCommit(reward), "Persist purchase ledger and wallet atomically");
                var reopened = new MiningProgressStore(Path.Combine(dir, "save.json"));
                Check(reopened.Current.coins == 1060 && Array.IndexOf(reopened.Current.fulfilledPurchases, "ad:completed") >= 0, "Cash and receipt ledger survive restart");
            } finally { Directory.Delete(dir, true); }
            Check(QuarryOnlineServices.RetryDelay(0) == 5 && QuarryOnlineServices.RetryDelay(12) == 60, "Retry backoff capped");
            // Exercise the actual installed SDK request list without starting a store in Edit mode.
            var assembly=typeof(PendingOrder).Assembly;
            var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            var useCaseType=assembly.GetType("UnityEngine.Purchasing.ConfirmOrderUseCase");
            var serviceType=assembly.GetType("UnityEngine.Purchasing.PurchaseService");
            var useCase=System.Runtime.Serialization.FormatterServices.GetUninitializedObject(useCaseType);
            var requestsField=useCaseType.GetField("m_ConfirmationRequests",flags);
            var requests=(System.Collections.IList)Activator.CreateInstance(requestsField.FieldType);requestsField.SetValue(useCase,requests);
            var service=System.Runtime.Serialization.FormatterServices.GetUninitializedObject(serviceType);
            serviceType.GetField("m_ConfirmOrderUseCase",flags).SetValue(service,useCase);
            var controller=System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(StoreController));
            typeof(StoreController).GetField("m_PurchaseService",flags).SetValue(controller,service);
            var info=(IOrderInfo)Activator.CreateInstance(assembly.GetType("UnityEngine.Purchasing.OrderInfo"),flags|System.Reflection.BindingFlags.Public,null,new object[]{"","stalled","fake"},null);
            var order=new PendingOrder(new Cart(new System.Collections.Generic.List<CartItem>()),info);
            int failures=0;
            Action<PendingOrder,Order> completed=(pending,result)=>{Check(result is FailedOrder,"Timeout must not manufacture success");failures++;};
            requests.Add(Activator.CreateInstance(assembly.GetType("UnityEngine.Purchasing.ConfirmOrderRequest"),flags,null,new object[]{order,completed},null));
            var callback=QuarryStore.FindConfirmationCallback(controller);
            Check(callback!=null,"Installed IAP confirmation recovery binding resolves");
            callback.OnConfirmOrderFailed(new FailedOrder(order,PurchaseFailureReason.Unknown,"timeout"));
            Check(failures==1&&requests.Count==0,"Stalled SDK confirmation request is released");
            callback.OnConfirmOrderSucceeded("stalled");
            Check(failures==1,"Late confirmation cannot deliver twice");
            return passed + " service checks passed (Editor; no real purchases or ad requests).";
        }
    }
}
#endif
