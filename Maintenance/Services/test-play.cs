if(!EditorApplication.isPlaying)throw new System.Exception("Play mode required");
var ui=Bulldozer.UI.QuarryGameUI.Instance;var store=Bulldozer.Services.QuarryStore.Instance;var services=Bulldozer.Services.QuarryOnlineServices.Instance;
var report=new System.Collections.Generic.List<string>();void Check(bool ok,string name){report.Add((ok?"PASS ":"FAIL ")+name);}
Check(ui&&store&&services,"gameplay UI and persistent services exist");
Check(UnityEngine.Object.FindObjectsByType<Bulldozer.Services.QuarryStore>(FindObjectsSortMode.None).Length==1,"single store service");
Check(UnityEngine.Object.FindObjectsByType<Bulldozer.Services.QuarryOnlineServices>(FindObjectsSortMode.None).Length==1,"single online service");
Check(store.enabled&&services.enabled,"service components enabled");
ui.player.autoMine=false;ui.Open("shop");var shop=ui.shopPage.GetComponent<Bulldozer.Services.QuarryShopPanel>();
Check(ui.shopPage.activeSelf&&Bulldozer.UI.QuarryGameUI.BlocksInput&&Time.timeScale==0,"shop opens as a modal and pauses gameplay");
Check(!shop.cashButton.interactable&&!shop.removeButton.interactable&&shop.cashPrice.text=="SETUP PENDING","unconfigured purchase buttons do not simulate payment");
ui.Open("privacy");var privacy=ui.privacyPage.GetComponent<Bulldozer.Services.QuarryPrivacyPanel>();privacy.analytics.isOn=true;Check(services.Analytics,"analytics toggle passes true");privacy.analytics.isOn=false;Check(!services.Analytics,"analytics toggle passes false");
privacy.crashReports.isOn=true;Check(services.CrashReports,"crash reports toggle works independently");privacy.crashReports.isOn=false;
privacy.attribution.isOn=true;Check(services.Attribution,"attribution choice saved without configured key");privacy.attribution.isOn=false;
int before=ui.equipment.Cash;var fixture=UnityEngine.Object.Instantiate(store.settings);fixture.androidRemoveAdsId="test.quarry.remove_ads";fixture.iosRemoveAdsId=fixture.androidRemoveAdsId;fixture.androidCashPackId="test.quarry.cash1000";fixture.iosCashPackId=fixture.androidCashPackId;store.settings=fixture;
var approved=new System.Collections.Generic.HashSet<string>();int gateFrame=0;int phase=0;float deadline=Time.realtimeSinceStartup+90;double retry=EditorApplication.timeSinceStartup+4;int startFrame=Time.frameCount;EditorApplication.isPaused=true;Time.captureDeltaTime=1f/30;
EditorApplication.CallbackFunction tick=null;tick=()=>{
 if(!EditorApplication.isPlaying){EditorApplication.update-=tick;return;}
 try{ EditorApplication.Step(); var window=Resources.FindObjectsOfTypeAll<MonoBehaviour>().FirstOrDefault(o=>o&&o.GetType().FullName=="UnityEngine.Purchasing.UIFakeStoreWindow");if(window&&store.Busy){var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;string key=window.GetInstanceID()+":"+window.GetType().GetField("m_QueryText",flags).GetValue(window);if(approved.Add(key))window.GetType().GetMethod("OnOkClicked",flags).Invoke(window,null);}
  if(Time.realtimeSinceStartup>deadline)throw new System.Exception("Store fixture timeout at phase "+phase+": "+store.Status);
  if(phase==0&&EditorApplication.timeSinceStartup>=retry){store.Retry();phase=1;}
  else if(phase==1&&store.Ready&&!store.Busy){Check(Time.frameCount>startFrame+1,"Play Mode advances while UI paused");Check(store.CanBuy(false),"IAP fake store connects and fetches products");ui.Open("shop");shop.BuyCash();phase=2;}
  else if(phase==2&&!store.Busy){Check(ui.equipment.Cash==before+1000,"verified fake purchase grants cash through real pending callback");Check(ui.equipment.Snapshot.fulfilledPurchases.Length==1,"purchase receipt recorded in same save");gateFrame=Time.frameCount+3;phase=20;}
  else if(phase==20&&Time.frameCount>=gateFrame){shop.BuyRemoveAds();phase=3;}
  else if(phase==3&&!store.Busy){Check(store.AdsRemoved,"Remove Ads purchase sets durable entitlement");int count=ui.equipment.Snapshot.fulfilledPurchases.Length;Check(count==2,"both transactions recorded");store.Restore();phase=4;}
  else if(phase==4&&!store.Busy){Check(ui.equipment.Cash==before+1000,"restore never regrants consumable cash");Check(store.AdsRemoved&&!store.CanBuy(true),"owned Remove Ads cannot be bought again");var reloaded=new Bulldozer.Player.Persistence.MiningProgressStore(ui.equipment.SavePath);Check(reloaded.Current.coins==ui.equipment.Cash&&reloaded.Current.removeAds,"purchase state survives reopening save");bool proceeded=false;services.RoundBreak(()=>proceeded=true);Check(proceeded,"Remove Ads or unavailable SDK immediately continues round");ui.Open("shop");System.IO.File.WriteAllLines("Maintenance/Services/play-results.txt",report);EditorApplication.update-=tick;return;}
 }catch(System.Exception ex){report.Add("FAIL "+ex);System.IO.File.WriteAllLines("Maintenance/Services/play-results.txt",report);EditorApplication.update-=tick;}
};EditorApplication.update+=tick;return "Running isolated native-IAP-API fake store checks; no real payment or account IDs used.";


