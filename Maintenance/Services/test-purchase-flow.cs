if(!EditorApplication.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new System.Exception("Isolated Play session required");
var ui=Bulldozer.UI.QuarryGameUI.Instance;var store=Bulldozer.Services.QuarryStore.Instance;var services=Bulldozer.Services.QuarryOnlineServices.Instance;
var report=System.IO.File.ReadAllLines("Maintenance/Services/play-results.txt").Take(10).ToList();void Check(bool ok,string name){report.Add((ok?"PASS ":"FAIL ")+name);}
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
MonoBehaviour FakeWindow()=>Resources.FindObjectsOfTypeAll<MonoBehaviour>().FirstOrDefault(o=>o&&o.GetType().FullName=="UnityEngine.Purchasing.UIFakeStoreWindow");
var old=FakeWindow();if(old)old.GetType().GetMethod("OnCancelClicked",flags).Invoke(old,null);
int phase=0,before=ui.equipment.Cash,frame=Time.frameCount;float deadline=Time.realtimeSinceStartup+150;var approved=new System.Collections.Generic.HashSet<string>();
EditorApplication.CallbackFunction tick=null;tick=()=>{
 if(!EditorApplication.isPlaying){EditorApplication.update-=tick;return;}
 try{
  if(Time.realtimeSinceStartup>deadline)throw new System.Exception("Timed out: "+phase+" "+store.Status);
  if(phase==0&&Time.frameCount>frame+1&&!store.Busy){Check(true,"Play Mode advances with shop paused");Check(store.CanBuy(false),"IAP fake store products available");ui.Open("shop");store.Buy(false);phase=1;}
  var window=FakeWindow();if(window&&store.Busy){string key=window.GetInstanceID()+":"+window.GetType().GetField("m_QueryText",flags).GetValue(window);if(approved.Add(key))window.GetType().GetMethod("OnOkClicked",flags).Invoke(window,null);}
  if(phase==1&&!store.Busy){Check(ui.equipment.Cash==before+1000,"verified fake pending callback grants 1000 cash");Check(ui.equipment.Snapshot.fulfilledPurchases.Length==1,"cash receipt recorded atomically");store.Buy(true);phase=2;}
  else if(phase==2&&!store.Busy){Check(store.AdsRemoved,"Remove Ads purchase sets durable entitlement");Check(ui.equipment.Snapshot.fulfilledPurchases.Length==2,"both transaction IDs recorded");store.Restore();phase=3;}
  else if(phase==3&&!store.Busy){Check(ui.equipment.Cash==before+1000,"restore does not regrant consumable cash");Check(store.AdsRemoved&&!store.CanBuy(true),"owned Remove Ads cannot be repurchased");var saved=new Bulldozer.Player.Persistence.MiningProgressStore(ui.equipment.SavePath);Check(saved.Current.coins==ui.equipment.Cash&&saved.Current.removeAds,"purchase state reloads from disk");bool proceeded=false;services.RoundBreak(()=>proceeded=true);Check(proceeded,"round proceeds immediately when ads removed or unconfigured");ui.Open("shop");System.IO.File.WriteAllLines("Maintenance/Services/play-results.txt",report);EditorApplication.update-=tick;}
 }catch(System.Exception ex){report.Add("FAIL "+ex);System.IO.File.WriteAllLines("Maintenance/Services/play-results.txt",report);EditorApplication.update-=tick;}
};EditorApplication.update+=tick;return "Approving only Unity fake-store dialogs in isolated save fixture.";
