using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Bulldozer.Services
{
    // Small placeholder UI, one canvas and the shared built-in font. Replace with supplied sprites later.
    public sealed class QuarryServiceOverlay : MonoBehaviour
    {
        public static bool ConnectionBlocked => QuarryServicesRoot.Instance &&
            (!QuarryServicesRoot.Instance.connectivity || !QuarryServicesRoot.Instance.connectivity.IsReady ||
             QuarryServicesRoot.Instance.connectivity.Status != ConnectivityStatus.Online);
        public static bool BlocksInput => ConnectionBlocked || (QuarryOnlineServices.Instance && QuarryOnlineServices.Instance.AdShowing);
        public static bool ConnectionPanelRequired => ConnectionBlocked && QuarryServicesRoot.Instance &&
            QuarryServicesRoot.Instance.connectivity && QuarryServicesRoot.Instance.connectivity.ConnectionProblem;
        public bool ConnectionPanelVisible => gate && gate.activeSelf;
        GameObject gate, countdownRoot, rewardRoot, purchaseRoot;
        Text gateText, countdownText, rewardText, purchaseText;
        Button rewardButton, dismissButton;
        bool purchaseDismissed, previousConnection;
        bool heldClock;
        float savedTimeScale = 1;
        float purchaseStarted, nextRefresh;
        Font font;
        void Start()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvasObject = new GameObject("Online Services UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 32000;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(720, 1280); scaler.matchWidthOrHeight = .5f;
            gate = Panel(canvasObject.transform, "Internet Required", new Vector2(0,0), Vector2.zero, true);
            gateText = Label(gate.transform, "Connection", new Vector2(0, 50), new Vector2(720, 150), 30);
            MakeButton(gate.transform, "RETRY", new Vector2(0, -90), new Vector2(240, 64), () => { QuarryServicesRoot.Instance?.connectivity.CheckNow(); }).name = "Retry Internet";
            purchaseRoot = Panel(canvasObject.transform, "Purchase Status", Vector2.zero, new Vector2(700, 260), false);
            purchaseText = Label(purchaseRoot.transform, "Status", new Vector2(0, 40), new Vector2(650, 130), 24);
            dismissButton = MakeButton(purchaseRoot.transform, "CONTINUE PLAYING", new Vector2(0,-75), new Vector2(320,60), () => { purchaseDismissed = true; });
            countdownRoot = Panel(canvasObject.transform, "Ad Countdown", new Vector2(0,250), new Vector2(500,64), false);
            countdownText = Label(countdownRoot.transform, "Countdown", Vector2.zero, new Vector2(480,60), 26);
            rewardRoot = new GameObject("Rewarded Cash", typeof(RectTransform));
            rewardRoot.transform.SetParent(canvasObject.transform, false);
            var rewardRect = (RectTransform)rewardRoot.transform; rewardRect.anchorMin = rewardRect.anchorMax = new Vector2(.5f,0); rewardRect.anchoredPosition = new Vector2(0,70);
            rewardButton = MakeButton(rewardRoot.transform, "WATCH VIDEO", Vector2.zero, new Vector2(380,62), () => { QuarryOnlineServices.Instance?.WatchReward(); });
            rewardText = rewardButton.GetComponentInChildren<Text>();
            gate.SetActive(false); purchaseRoot.SetActive(false); countdownRoot.SetActive(false); rewardRoot.SetActive(false);
        }
        GameObject Panel(Transform parent, string name, Vector2 position, Vector2 size, bool fill)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false);
            var rect = (RectTransform)go.transform;
            if (fill) { rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero; }
            else { rect.sizeDelta=size;rect.anchoredPosition=position; }
            go.GetComponent<Image>().color=new Color(.035f,.065f,.08f,.97f); return go;
        }
        Text Label(Transform parent,string name,Vector2 position,Vector2 size,int fontSize)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);
            var rect=(RectTransform)go.transform;rect.sizeDelta=size;rect.anchoredPosition=position;
            var text=go.GetComponent<Text>();text.font=font;text.fontSize=fontSize;text.alignment=TextAnchor.MiddleCenter;text.color=Color.white;text.raycastTarget=false;return text;
        }
        Button MakeButton(Transform parent,string label,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action)
        {
            var go=Panel(parent,label,position,size,false);go.GetComponent<Image>().color=new Color(.9f,.57f,.12f);
            var button=go.AddComponent<Button>();button.onClick.AddListener(action);
            Label(go.transform,"Label",Vector2.zero,size-Vector2.one*12,22).text=label;return button;
        }
        void Update()
        {
            if (!gate || Time.unscaledTime < nextRefresh) return; nextRefresh=Time.unscaledTime+.1f;
            bool blocked=ConnectionBlocked;
            if (blocked) { if (!heldClock) { savedTimeScale=Time.timeScale; heldClock=true; } Time.timeScale=0; }
            else if (heldClock) { heldClock=false; Time.timeScale=savedTimeScale; }
            bool showConnection=ConnectionPanelRequired;
            gate.SetActive(showConnection);if(showConnection){gate.transform.SetAsLastSibling();gateText.text="CONNECTION COULD NOT BE VERIFIED\nCheck your connection and retry. Checking automatically...";Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
            if(previousConnection!=showConnection){QuarryOnlineServices.Instance?.Event(showConnection?"internet_gate_shown":"internet_gate_hidden");previousConnection=showConnection;}
            var online=QuarryOnlineServices.Instance;
            bool countdown=online && online.AdCountdownSeconds>0;
            countdownRoot.SetActive(countdown&&!blocked);if(countdown)countdownText.text="Displaying ad in "+Mathf.CeilToInt(online.AdCountdownSeconds)+" seconds";
            var ui=Bulldozer.UI.QuarryGameUI.Instance;
            rewardRoot.SetActive(!blocked&&ui&&ui.shopPage&&ui.shopPage.activeInHierarchy);
            if(rewardRoot.activeSelf){rewardButton.interactable=online&&online.RewardReady;rewardText.text=online&&online.RewardReady?"WATCH VIDEO  +$"+online.settings.rewardedCashAmount:"REWARD VIDEO LOADING";}
            var store=QuarryStore.Instance;
            bool purchasing=store&&store.CheckoutPending;
            if(!purchasing){purchaseDismissed=false;purchaseStarted=Time.unscaledTime;}
            purchaseRoot.SetActive(purchasing&&!purchaseDismissed&&!blocked);
            if(purchaseRoot.activeSelf){bool slow=Time.unscaledTime-purchaseStarted>=30;purchaseText.text=slow?"Purchase is pending. You can continue playing; delivery will recover automatically.":"Contacting store...";dismissButton.gameObject.SetActive(slow);}
        }
        void OnDestroy() { if (heldClock) Time.timeScale=savedTimeScale; }
    }
}
