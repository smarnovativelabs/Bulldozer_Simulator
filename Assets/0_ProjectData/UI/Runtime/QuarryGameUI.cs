using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;
using Bulldozer.Player;
namespace Bulldozer.UI {
 public sealed class QuarryGameUI : MonoBehaviour {
  public MinerController player;public MinerEquipment equipment;public MinerUpgradeArea workbench;public QuarryUIFeedback feedback;public QuarryModal modal;
  public GameObject settingsPage,pausePage,upgradePage,inventoryPage,confirmationPage,performanceOptions,shopPage,privacyPage;
  public TMP_Text modalTitle,cash,inventorySummary,inventoryFooter,upgradeFooter,performanceLabel,notificationText;
  public TMP_Text[] inventoryCounts,upgradeValues,upgradePrices;public RectTransform[] upgradeIcons;public UnityEngine.UI.Image[] upgradeCards;public UnityEngine.UI.Button[] buyButtons;
  public UnityEngine.UI.Slider volume;public QuarryToggle soundToggle,vibrationToggle;public CanvasGroup notification;public RectTransform notificationRect;
  public static QuarryGameUI Instance{get;private set;}public static bool BlocksInput=>Instance&&Instance.modal&&Instance.modal.gameObject.activeSelf;
  float previousTimeScale=1,poll;int previousCash;bool insideWorkbench;string page="";int storage;float[] nextFeedback=new float[3];Tween[] cardTweens=new Tween[3],iconTweens=new Tween[3],priceTweens=new Tween[3],valueTweens=new Tween[3];Vector2[] priceHomes;readonly Queue<string> notices=new Queue<string>();Sequence noticeTween;Vector2 noticeHome;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset()=>Instance=null;
  void Awake(){Instance=this;modal.Closed=ReleasePause;modal.gameObject.SetActive(false);notification.gameObject.SetActive(false);noticeHome=notificationRect.anchoredPosition;priceHomes=upgradePrices.Select(t=>t.rectTransform.anchoredPosition).ToArray();}
  void Start(){previousCash=equipment.Cash;volume.SetValueWithoutNotify(PlayerPrefs.GetFloat(QuarryUIFeedback.PreferencePrefix+"Volume",.8f));volume.onValueChanged.AddListener(SetVolume);for(int i=0;i<3;i++){int n=i;buyButtons[i].onClick.AddListener(()=>Purchase(n));}Refresh();}
  void Update(){if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){if(BlocksInput)Close();else Open("pause");}if(Time.unscaledTime<poll)return;poll=Time.unscaledTime+.2f;int current=equipment.Cash;if(current>previousCash)Notify("CASH RECEIVED  +$"+(current-previousCash));previousCash=current;bool near=MiningProduction.Contains(workbench.zone,player.transform.position);if(near&&!insideWorkbench&&!BlocksInput&&!player.InputBlocked)Open("upgrades");insideWorkbench=near;Refresh();}
  public void Open(string target){if(player.ExcavationInputBlocked||CommonUI.Instance&&CommonUI.Instance.BlocksGameplayInput)return;if(!modal.gameObject.activeSelf){previousTimeScale=Time.timeScale;Time.timeScale=0;player.CancelMining();player.touchControls.Move=Vector2.zero;player.touchControls.Sprinting=false;}CancelUpgradeMotion();page=target;settingsPage.SetActive(target=="settings");pausePage.SetActive(target=="pause");upgradePage.SetActive(target=="upgrades");inventoryPage.SetActive(target=="inventory");confirmationPage.SetActive(target=="confirmation");if(shopPage)shopPage.SetActive(target=="shop");if(privacyPage)privacyPage.SetActive(target=="privacy");performanceOptions.SetActive(false);modalTitle.text=target=="shop"?"QUARRY STORE":target=="privacy"?"PRIVACY CHOICES":target=="confirmation"?"RESET SETTINGS?":target=="upgrades"?"EQUIPMENT UPGRADES":target=="inventory"?"WORKSITE INVENTORY":target=="pause"?"SHIFT PAUSED":"SETTINGS";modal.Open();feedback.Play();Refresh();}
  public void Close(){CancelUpgradeMotion();feedback.Play();modal.Close();}
  void ReleasePause(){Time.timeScale=previousTimeScale;page="";player.touchControls.Move=Vector2.zero;}
  public void SetVolume(float v){PlayerPrefs.SetFloat(QuarryUIFeedback.PreferencePrefix+"Volume",v);PlayerPrefs.Save();QuarryUIFeedback.ApplySettings();}
  public void TogglePerformance(){performanceOptions.transform.SetAsLastSibling();performanceOptions.SetActive(!performanceOptions.activeSelf);feedback.Play();}
  public void SetPerformance(int mode){PlayerPrefs.SetInt(QuarryUIFeedback.PreferencePrefix+"Performance",mode);PlayerPrefs.Save();QuarryUIFeedback.ApplySettings();performanceOptions.SetActive(false);feedback.Play();Refresh();}
  public void ResetSettings(){foreach(var key in new[]{"Sound","Volume","Vibration","Performance"})PlayerPrefs.DeleteKey(QuarryUIFeedback.PreferencePrefix+key);PlayerPrefs.Save();QuarryUIFeedback.ApplySettings();volume.SetValueWithoutNotify(.8f);soundToggle.Refresh();vibrationToggle.Refresh();Open("settings");Notify("SETTINGS RESTORED");}
  public void SelectStorage(int value){storage=value;feedback.Play();Refresh();}
  int displayedCash=int.MinValue;
  readonly int[] inventoryTotals=new int[12];
  public void Refresh(){
   if(!equipment)return;
   int current=equipment.Cash;
   if(current!=displayedCash){displayedCash=current;cash.text="$"+current;}
   // Hidden panels must not copy save data, build inventory arrays or rebuild labels.
   if(!modal.gameObject.activeSelf)return;
   if(settingsPage.activeSelf)performanceLabel.text=PlayerPrefs.GetInt(QuarryUIFeedback.PreferencePrefix+"Performance",0)==0?"BATTERY SAVER  /  30 FPS":"BALANCED  /  60 FPS";
   if(inventoryPage.activeSelf)RefreshInventory();
   if(upgradePage.activeSelf)RefreshUpgrades();
  }
  void RefreshInventory(){
   var s=equipment.Snapshot;Array.Clear(inventoryTotals,0,inventoryTotals.Length);int count=0;
   void Count(int[] items){foreach(int id in items)if(id>=0&&id<inventoryTotals.Length){inventoryTotals[id]++;count++;}}
   if(storage==0)Count(s.items);else if(storage==1){Count(s.backpackItems);Count(s.barrowItems);}else if(storage==2){Count(s.refineryInput);Count(s.refineryOutput);}else{Count(s.loadingQueue);Count(s.truckItems);}
   string name=storage==0?(equipment.UsingWheelbarrow?"CART":"BACKPACK"):storage==1?"PARKED STORAGE":storage==2?"REFINERY":"LOADING & TRUCK";
   inventorySummary.text=name+"  /  "+count+" ITEMS";
   for(int i=0;i<6;i++)inventoryCounts[i].text=inventoryTotals[i]+" RAW   /   "+inventoryTotals[i+6]+" REFINED";
   inventoryFooter.text="Carrying "+equipment.CargoCount+" / "+equipment.Capacity+"    |    Loose stones: "+s.groundDrops.Length+"    |    Deliveries: "+s.trucksCompleted;
  }
  void RefreshUpgrades(){
   bool near=MiningProduction.Contains(workbench.zone,player.transform.position);
   upgradeFooter.text=near?"Purchases save immediately. Your upgrades carry into every excavation.":"Visit the workbench to purchase. Follow the quarry guide back to the upgrade pad.";
   for(int i=0;i<3;i++){
    int level=i==0?equipment.DamageLevel:i==1?equipment.CapacityLevel:equipment.SpeedLevel;
    int limit=i==0?10:i==1?15:5;
    int cost=i==0?equipment.DamageCost:i==1?equipment.CapacityCost:equipment.SpeedCost;
    bool max=level>=limit;buyButtons[i].interactable=!max;
    upgradePrices[i].text=max?"MAX LEVEL":"$"+cost+(equipment.Cash<cost?"  /  NEED CASH":"  /  UPGRADE");
    if(valueTweens[i]==null||!valueTweens[i].IsActive())upgradeValues[i].text=i==0?"POWER  "+equipment.Damage+(max?"":"  >  "+(equipment.Damage+1)):i==1?"CAPACITY  "+(5+equipment.CapacityLevel)+(max?"":"  >  "+(6+equipment.CapacityLevel)):"MOVE SPEED  "+Mathf.RoundToInt(equipment.MovementMultiplier*100)+"%"+(max?"":"  >  "+Mathf.RoundToInt((equipment.MovementMultiplier+.08f)*100)+"%");
   }
  }

  public bool Purchase(int index){if(index<0||index>2||Time.unscaledTime<nextFeedback[index])return false;nextFeedback[index]=Time.unscaledTime+.3f;if(!MiningProduction.Contains(workbench.zone,player.transform.position)){Notify("VISIT THE WORKBENCH TO UPGRADE");Error(index);return false;}float before=index==0?equipment.Damage:index==1?5+equipment.CapacityLevel:equipment.MovementMultiplier*100;bool ok=index==2?equipment.TryUpgradeSpeed():equipment.TryUpgrade(index==1);if(!ok){Error(index);if(!string.IsNullOrEmpty(equipment.SaveError))Notify("SAVE FAILED - PURCHASE CANCELLED");return false;}previousCash=equipment.Cash;cardTweens[index]?.Kill();iconTweens[index]?.Kill();valueTweens[index]?.Kill();upgradeCards[index].color=new Color(1,.78f,.40f);cardTweens[index]=upgradeCards[index].DOColor(Color.white,.35f).SetUpdate(true);upgradeIcons[index].localScale=Vector3.one;iconTweens[index]=DOTween.Sequence().SetUpdate(true).Append(upgradeIcons[index].DOScale(1.04f,.125f)).Append(upgradeIcons[index].DOScale(1,.125f));float after=index==0?equipment.Damage:index==1?5+equipment.CapacityLevel:equipment.MovementMultiplier*100;valueTweens[index]=DOTween.To(()=>before,x=>{before=x;upgradeValues[index].text=(index==2?"MOVE SPEED  ":"UPGRADED  ")+x.ToString("0")+(index==2?"%":"");},after,.25f).SetUpdate(true).SetEase(Ease.Linear);feedback.Play(1);Notify(index==0?"PICKAXE UPGRADED":index==1?"CARGO RACK UPGRADED":"MOVEMENT SPEED UPGRADED");Refresh();return true;}
  void CancelUpgradeMotion(){for(int i=0;i<3;i++){cardTweens[i]?.Kill();iconTweens[i]?.Kill();priceTweens[i]?.Kill();valueTweens[i]?.Kill();upgradeCards[i].color=Color.white;upgradeIcons[i].localScale=Vector3.one;upgradePrices[i].color=new Color(.1f,.12f,.13f);if(priceHomes!=null)upgradePrices[i].rectTransform.anchoredPosition=priceHomes[i];}}
  void Error(int i){priceTweens[i]?.Kill();upgradePrices[i].rectTransform.anchoredPosition=priceHomes[i];upgradePrices[i].color=new Color(.5f,.13f,.10f);priceTweens[i]=DOTween.Sequence().SetUpdate(true).Append(upgradePrices[i].rectTransform.DOShakeAnchorPos(.18f,new Vector2(4,0),8,0,false,true)).Join(upgradePrices[i].DOColor(new Color(.1f,.12f,.13f),.3f)).OnComplete(()=>upgradePrices[i].rectTransform.anchoredPosition=priceHomes[i]);feedback.Play(2);}
  public void Notify(string message){if(notices.Count<32)notices.Enqueue(message);if(noticeTween==null||!noticeTween.IsActive())NextNotice();}
  void NextNotice(){if(notices.Count==0){noticeTween=null;notification.gameObject.SetActive(false);return;}notificationText.text=notices.Dequeue();notification.gameObject.SetActive(true);notification.alpha=0;notificationRect.anchoredPosition=noticeHome-Vector2.up*20;noticeTween=DOTween.Sequence().SetUpdate(true).Append(notification.DOFade(1,.2f)).Join(notificationRect.DOAnchorPos(noticeHome,.2f)).AppendInterval(2).Append(notification.DOFade(0,.2f)).OnComplete(NextNotice);}
  void OnDisable(){noticeTween?.Kill();notices.Clear();for(int i=0;i<3;i++){cardTweens[i]?.Kill();iconTweens[i]?.Kill();priceTweens[i]?.Kill();valueTweens[i]?.Kill();}if(modal&&modal.gameObject.activeSelf){modal.gameObject.SetActive(false);Time.timeScale=previousTimeScale;}if(Instance==this)Instance=null;}
 }
}






