using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace Bulldozer.Services
{
    internal sealed class StoreReceiptVerifier : IDisposable
    {
        private CrossPlatformValidator googleValidator;
        public bool CanValidate { get; private set; }
        public string SetupError { get; private set; }
#if UNITY_IOS && !UNITY_EDITOR
        private readonly AppleTransactionVerifier appleVerifier = new AppleTransactionVerifier();
#endif

        public StoreReceiptVerifier(QuarryServiceSettings catalog)
        {
            try
            {
#if UNITY_EDITOR
                // Simulation only. This branch is absent from every device build.
                CanValidate = !string.IsNullOrEmpty(UnityEditor.SessionState.GetString("Bulldozer.Mining.TestSavePath", ""));
#elif UNITY_ANDROID
                if (string.IsNullOrWhiteSpace(catalog.googlePlayPublicKey))
                    throw new InvalidOperationException("Add the Google Play public licensing key to Shop Catalog.");
                googleValidator = new CrossPlatformValidator(Convert.FromBase64String(catalog.googlePlayPublicKey), Application.identifier);
                CanValidate = true;
#elif UNITY_IOS && !UNITY_EDITOR
                // Independently query StoreKit's verified transaction. Unity IAP 5.4.3 can
                // deliver a direct-purchase callback even after native verification failed.
                CanValidate = AppleTransactionVerifier.IsSupported;
                if (!CanValidate) SetupError = "Purchases require iOS 15 or later and Apple StoreKit support.";
#else
                SetupError = "Paid purchases require a supported Android/iOS device build.";
#endif
            }
            catch (Exception exception) { SetupError = exception.Message; }
        }

        public async Task<string> VerifyAsync(Order order, string expectedProductId)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (!CanValidate || order?.Info?.Apple == null || string.IsNullOrWhiteSpace(order.Info.TransactionID) ||
                string.IsNullOrWhiteSpace(expectedProductId)) return null;
            var items = order.CartOrdered?.Items();
            if (items == null || items.Count != 1 || items[0].Quantity != 1 ||
                items[0].Product?.definition.id != expectedProductId) return null;
            ProductType type = items[0].Product.definition.type;
            if (type != ProductType.NonConsumable && type != ProductType.Consumable) return null;
            bool verified = await appleVerifier.VerifyAsync(expectedProductId, order.Info.TransactionID,
                type == ProductType.NonConsumable);
            return verified ? "store:" + QuarryStore.Hash("AppleAppStore|" + order.Info.TransactionID) : null;
#else
            // Keep Editor simulation and Android signature verification on their existing paths.
            await Task.CompletedTask;
            return TryVerify(order, expectedProductId, out string key) ? key : null;
#endif
        }

        public void Dispose()
        {
#if UNITY_IOS && !UNITY_EDITOR
            appleVerifier.Dispose();
#endif
        }

        // Called only from the IAP SDK's pending/confirmed-order callbacks, never from UI.
        public bool TryVerify(Order order, string expectedProductId, out string deduplicationId)
        {
            deduplicationId = null;
            if (!CanValidate || order?.Info == null || string.IsNullOrWhiteSpace(order.Info.TransactionID)) return false;
            try
            {
#if UNITY_EDITOR
                var items = order.CartOrdered?.Items();
                if (items == null || items.Count != 1 || items[0].Quantity != 1 ||
                    string.IsNullOrWhiteSpace(expectedProductId) || items[0].Product?.definition.id != expectedProductId) return false;
                JObject simulated = JObject.Parse(order.Info.Receipt);
                if ((string)simulated["Store"] != "fake" ||
                    (string)simulated["Payload"] != "ThisIsFakeReceiptData" ||
                    (string)simulated["TransactionID"] != order.Info.TransactionID) return false;
                deduplicationId = "editor-store:" + QuarryStore.Hash("fake|" + order.Info.TransactionID);
                return true;
#elif UNITY_ANDROID
                foreach (IPurchaseReceipt receipt in googleValidator.Validate(order.Info.Receipt))
                {
                    if (!(receipt is GooglePlayReceipt google) || google.productID != expectedProductId ||
                        google.packageName != Application.identifier || google.purchaseState != GooglePurchaseState.Purchased ||
                        string.IsNullOrWhiteSpace(google.purchaseToken) || google.purchaseToken != order.Info.TransactionID) continue;
                    // Validate quantity from the same signed payload; this storefront deliberately sells one bundle at a time.
                    JObject wrapper = JObject.Parse(order.Info.Receipt);
                    JObject payload = JObject.Parse((string)wrapper["Payload"]);
                    JObject purchase = JObject.Parse((string)payload["json"]);
                    if ((purchase.Value<int?>("quantity") ?? 1) != 1) return false;
                    deduplicationId = "store:" + QuarryStore.Hash("GooglePlay|" + google.purchaseToken);
                    return true;
                }
#endif
            }
            catch (Exception) { /* Invalid/unavailable proof: leave the order pending and grant nothing. */ }
            return false;
        }
    }
}

