using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Bulldozer.Services
{
    public sealed partial class QuarryOnlineServices
    {
        TelemetryOutbox outbox;
        BaseTenjin tenjin;
        bool firebaseReady, telemetryDirty;
        Task<DependencyStatus> firebaseTask;
        double flushAt;
        string OutboxPath => Path.Combine(Application.persistentDataPath, "quarry-telemetry-outbox.json");
        [Serializable] sealed class ReceiptEnvelope { public string Payload; }
        [Serializable] sealed class GooglePayload { public string json, signature; }
        [Serializable] sealed class AdRevenueData
        {
            public double revenue;
            public string ad_revenue_currency = "USD", country, network_name, ad_unit_id, format, placement, network_placement, creative_id, revenue_precision;
        }
        void InitializeTelemetry() { outbox = Application.isEditor ? new TelemetryOutbox() : TelemetryOutbox.Load(OutboxPath); }
        void InitTelemetry()
        {
            // Editor SDK placeholders exercise ads and purchases without sending production telemetry.
            if (Application.isEditor) return;
            if (settings.firebaseConfigurationAdded && !firebaseReady)
            {
                try {
                    if (firebaseTask == null) firebaseTask = FirebaseApp.CheckAndFixDependenciesAsync();
                    if (firebaseTask.IsCompleted) {
                        var task = firebaseTask; firebaseTask = null;
                        if (task.GetAwaiter().GetResult() == DependencyStatus.Available) {
                            var app = FirebaseApp.DefaultInstance; firebaseReady = true;
                            Crashlytics.ReportUncaughtExceptionsAsFatal = true; ApplyTelemetryChoices();
                        }
                    }
                } catch { firebaseTask = null; Status = "Analytics unavailable; retrying"; }
            }
            if (Attribution && !settings.underAgeOfConsent && TrackingSettled() && !tenjin && !string.IsNullOrWhiteSpace(settings.TenjinKey))
            {
                GameObject child = null;
                try {
                    child = new GameObject("Tenjin"); child.transform.SetParent(transform, false);
#if UNITY_ANDROID && !UNITY_EDITOR
                    tenjin = child.AddComponent<AndroidTenjin>();
#elif UNITY_IOS && !UNITY_EDITOR
                    tenjin = child.AddComponent<IosTenjin>();
#endif
                    if (!tenjin) { Destroy(child); return; }
                    tenjin.Init(settings.TenjinKey);
                    if (Application.platform == RuntimePlatform.Android) tenjin.SetAppStoreType(AppStoreType.googleplay);
                    // Denied ATT must not be treated as permission to track.
                    if (TrackingAuthorized) { tenjin.OptIn(); tenjin.Connect(); } else tenjin.OptOut();
                } catch { if (child) Destroy(child); tenjin = null; Status = "Attribution unavailable; retrying"; }
            }
        }
        void ApplyTelemetryChoices()
        {
            if (firebaseReady) {
                FirebaseAnalytics.SetConsent(new System.Collections.Generic.Dictionary<ConsentType, ConsentStatus> {
                    { ConsentType.AnalyticsStorage, Analytics ? ConsentStatus.Granted : ConsentStatus.Denied },
                    { ConsentType.AdStorage, ConsentStatus.Denied }, { ConsentType.AdUserData, ConsentStatus.Denied }, { ConsentType.AdPersonalization, ConsentStatus.Denied }
                });
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(Analytics);
                Crashlytics.IsCrashlyticsCollectionEnabled = CrashReports;
            }
            if (tenjin) { if (Attribution && TrackingAuthorized && !settings.underAgeOfConsent) tenjin.OptIn(); else tenjin.OptOut(); }
            // A revoked choice discards queued data for that destination instead of sending it on a later opt-in.
            if (outbox != null) {
                foreach (var message in outbox.messages) { if (!Analytics) message.firebaseSent = true; if (!Attribution) message.tenjinSent = true; }
                telemetryDirty = true; PersistTelemetry();
            }
            telemetryRetry = 0;
        }
        void ReconnectTenjin() { try { if (tenjin && Attribution && TrackingAuthorized) tenjin.Connect(); } catch { Destroy(tenjin.gameObject); tenjin = null; } }
        public void Event(string name, long value = 0) { Queue(new TelemetryMessage { kind = "event", name = name, value = value }); }
        void Queue(TelemetryMessage message)
        {
            if (!InitializationRequested || Application.isEditor || outbox == null) return;
            message.firebaseSent = !Analytics; message.tenjinSent = !Attribution || (settings && settings.underAgeOfConsent);
            if (outbox.Add(message)) { telemetryDirty = true; if (message.kind != "event") PersistTelemetry(); }
        }
        void PersistTelemetry() { if (!Application.isEditor && telemetryDirty && outbox != null && outbox.Save(OutboxPath)) telemetryDirty = false; }
        void TickTelemetry(double now)
        {
            if (Application.isEditor || outbox == null || now < flushAt) return;
            flushAt = now + 2;
            PersistTelemetry(); if (telemetryDirty) return;
            if (outbox.Drain(SendFirebase, SendTenjin)) { telemetryDirty = true; PersistTelemetry(); }
        }
        bool SendFirebase(TelemetryMessage message)
        {
            if (!Analytics) return true;
            if (!firebaseReady) return false;
            if (message.kind == "event") FirebaseAnalytics.LogEvent(message.name, "value", message.value);
            else if (message.kind == "purchase") FirebaseAnalytics.LogEvent("quarry_purchase_delivered", new Parameter("item_id", message.product), new Parameter("currency", message.currency), new Parameter("value", message.value), new Parameter("transaction_id", message.transaction));
            else {
                var ad = JsonUtility.FromJson<AdRevenueData>(message.payload);
                FirebaseAnalytics.LogEvent("ad_impression", new Parameter("ad_platform", "AppLovin"), new Parameter("ad_source", ad.network_name ?? ""), new Parameter("ad_format", ad.format ?? ""), new Parameter("ad_unit_name", ad.ad_unit_id ?? ""), new Parameter("currency", "USD"), new Parameter("value", ad.revenue));
            }
            return true;
        }
        bool SendTenjin(TelemetryMessage message)
        {
            if (!Attribution || settings.underAgeOfConsent) return true;
            if (!TrackingSettled()) return false;
            if (!TrackingAuthorized) return true;
            if (!tenjin) return false;
            if (message.kind == "event") tenjin.SendEvent(message.name);
            else if (message.kind == "ad") tenjin.AppLovinImpressionFromJSON(message.payload);
            else if (message.apple) tenjin.Transaction(message.product, message.currency, 1, message.value, message.transaction, null, null);
            else {
                var envelope = JsonUtility.FromJson<ReceiptEnvelope>(message.receipt);
                var receipt = JsonUtility.FromJson<GooglePayload>(envelope.Payload);
                if (string.IsNullOrEmpty(receipt.json) || string.IsNullOrEmpty(receipt.signature)) return false;
                tenjin.Transaction(message.product, message.currency, 1, message.value, message.transaction, receipt.json, receipt.signature);
            }
            return true;
        }
        public bool RecordDeliveredPurchase(Order order)
        {
            if (Application.isEditor) return true;
            if (outbox == null || order?.Info == null || string.IsNullOrWhiteSpace(order.Info.TransactionID)) return false;
            var product = order.CartOrdered.Items().FirstOrDefault()?.Product; if (product == null) return false;
            var message = new TelemetryMessage { kind = "purchase", product = product.definition.id, transaction = order.Info.TransactionID, currency = product.metadata.isoCurrencyCode, value = (double)product.metadata.localizedPrice, receipt = order.Info.Receipt, apple = Application.platform == RuntimePlatform.IPhonePlayer, firebaseSent = !Analytics, tenjinSent = !Attribution };
            telemetryDirty = true;
            if (!outbox.JournalPurchase(message, OutboxPath)) return false;
            telemetryDirty = false; return true;
        }
        void Revenue(string id, MaxSdkBase.AdInfo info)
        {
            if (info == null || info.Revenue < 0 || double.IsNaN(info.Revenue) || double.IsInfinity(info.Revenue)) return;
            string country = ""; try { country = MaxSdk.GetSdkConfiguration().CountryCode; } catch {}
            Queue(new TelemetryMessage { kind = "ad", payload = JsonUtility.ToJson(new AdRevenueData { revenue = info.Revenue, country = country, network_name = info.NetworkName, ad_unit_id = info.AdUnitIdentifier, format = info.AdFormat, placement = info.Placement, network_placement = info.NetworkPlacement, creative_id = info.CreativeIdentifier, revenue_precision = info.RevenuePrecision }) });
        }
    }
}
