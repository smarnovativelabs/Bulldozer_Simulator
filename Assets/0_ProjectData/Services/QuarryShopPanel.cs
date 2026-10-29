using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Bulldozer.Services
{
    public sealed class QuarryShopPanel : MonoBehaviour
    {
        public TMP_Text status, cashDescription, removePrice, cashPrice;
        public Button removeButton, cashButton, restoreButton;
        QuarryStore subscribed;
        void OnEnable() { subscribed = QuarryStore.Instance; if (subscribed) subscribed.Changed += Refresh; Refresh(); }
        void OnDisable() { if (subscribed) subscribed.Changed -= Refresh; subscribed = null; }
        public void Refresh()
        {
            var store = QuarryStore.Instance;
            status.text = store ? store.Status : "Store setup pending";
            cashDescription.text = store && store.settings ? "$" + store.settings.cashPackAmount.ToString("N0") + " QUARRY CASH" : "QUARRY CASH";
            removePrice.text = store ? store.Price(true) : "SETUP PENDING";
            cashPrice.text = store ? store.Price(false) : "SETUP PENDING";
            removeButton.interactable = store && store.CanBuy(true);
            cashButton.interactable = store && store.CanBuy(false);
            restoreButton.interactable = store && store.Ready && !store.Busy;
        }
        public void BuyRemoveAds() { Click(); QuarryStore.Instance?.Buy(true); }
        public void BuyCash() { Click(); QuarryStore.Instance?.Buy(false); }
        public void Restore() { Click(); QuarryStore.Instance?.Restore(); }
        public void Retry() { Click(); QuarryStore.Instance?.Retry(); }
        void Click() { Bulldozer.UI.QuarryGameUI.Instance?.feedback.Play(); }
    }
}
