using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Bulldozer.Services
{
    public sealed partial class QuarryStore
    {
        readonly Dictionary<string, PendingOrder> pendingOrders = new Dictionary<string, PendingOrder>();
        readonly Dictionary<string, double> confirmationDeadlines = new Dictionary<string, double>();
        double recoveryAt, deliveryAt;
        bool restoreRequested;
        void ScheduleRecovery(double delay) => recoveryAt = Time.realtimeSinceStartupAsDouble + delay;
        void AccountChanged() { Ready = false; nextRetry = 0; ScheduleRecovery(0); }
        void OnApplicationPause(bool paused) { if (!paused) ScheduleRecovery(0); }
        void TickRecovery()
        {
            if (!InitializationRequested || !alive) return;
            double now = Time.realtimeSinceStartupAsDouble;
            foreach (var pair in confirmationDeadlines.ToArray())
            {
                if (now < pair.Value) continue;
                if (pendingOrders.TryGetValue(pair.Key, out var order))
                {
                    // Release the SDK's pending request too; clearing only our set leaves
                    // IAP rejecting every retry as ExistingPurchasePending.
                    try {
                        var callback = FindConfirmationCallback(store);
                        if (callback != null) callback.OnConfirmOrderFailed(new FailedOrder(order, PurchaseFailureReason.Unknown, "Confirmation timed out; fetching again."));
                        else Debug.LogError("IAP confirmation recovery binding missing; verify the installed package.");
                    } catch (Exception e) { Debug.LogWarning("Confirmation recovery: " + e.GetType().Name); }
                }
                confirmationDeadlines.Remove(pair.Key); acknowledged.Remove(pair.Key); ScheduleRecovery(0);
            }
            if (now >= deliveryAt)
            {
                deliveryAt = now + 5;
                foreach (var order in pendingOrders.Values.ToArray())
                    if (!acknowledged.Contains(order.Info.TransactionID)) RetryDelivery(order);
            }
            if (Busy || now < recoveryAt || Application.internetReachability == NetworkReachability.NotReachable) return;
            ScheduleRecovery(30); Retry();
        }
        async void RetryDelivery(PendingOrder order) { await Fulfill(order, true); }
        // Inspected against the installed Unity IAP package; link.xml preserves these fields.
        internal static IStorePurchaseConfirmCallback FindConfirmationCallback(object owner)
        {
            if (owner == null) return null;
            if (owner is IStorePurchaseConfirmCallback callback) return callback;
            for (var type = owner.GetType(); type != null; type = type.BaseType)
                foreach (var name in new[] { "m_PurchaseService", "m_BaseInternalPurchaseService", "m_ConfirmOrderUseCase" })
                {
                    var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    if (field != null) return FindConfirmationCallback(field.GetValue(owner));
                }
            return null;
        }
    }
}
