if(!EditorApplication.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new System.Exception("Isolated Play session required");
var store=Bulldozer.Services.QuarryStore.Instance;var online=Bulldozer.Services.QuarryOnlineServices.Instance;var ui=Bulldozer.UI.QuarryGameUI.Instance;
int cash=ui.equipment.Cash,count=ui.equipment.Snapshot.fulfilledPurchases.Length,handle=ui.gameObject.scene.handle;bool removed=store.AdsRemoved;
int frames=Time.frameCount;ui.Close();double deadline=EditorApplication.timeSinceStartup+90;bool requested=false;
EditorApplication.isPaused=true;Time.captureDeltaTime=1f/30;
EditorApplication.CallbackFunction tick=null;tick=()=>{
 if(!EditorApplication.isPlaying){EditorApplication.update-=tick;return;}
 if(EditorApplication.timeSinceStartup>deadline){System.IO.File.WriteAllText("Maintenance/Services/reload-results.txt","FAIL scene reload timeout");EditorApplication.update-=tick;return;}
 EditorApplication.Step();
 if(!requested&&Time.frameCount>frames+20){requested=Bulldozer.UI.CommonUI.Instance.SceneLoading.TryLoadScene("Assets/0_ProjectData/0_Scenes/1_Gameplay.unity");}
 var current=Bulldozer.UI.QuarryGameUI.Instance;
 if(!requested||!current||current.gameObject.scene.handle==handle||Bulldozer.UI.CommonUI.Instance.BlocksGameplayInput)return;
 var report=new System.Collections.Generic.List<string>();void Check(bool ok,string name)=>report.Add((ok?"PASS ":"FAIL ")+name);
 Check(Bulldozer.Services.QuarryStore.Instance==store&&UnityEngine.Object.FindObjectsByType<Bulldozer.Services.QuarryStore>(FindObjectsSortMode.None).Length==1,"store persists without duplication after scene reload");
 Check(Bulldozer.Services.QuarryOnlineServices.Instance==online&&UnityEngine.Object.FindObjectsByType<Bulldozer.Services.QuarryOnlineServices>(FindObjectsSortMode.None).Length==1,"online services persist without duplication");
 Check(current.equipment.Cash==cash&&current.equipment.Snapshot.fulfilledPurchases.Length==count&&store.AdsRemoved==removed,"cash, receipts and Remove Ads survive reload");
 Check(UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length==1,"one active EventSystem after reload");
 Check(current.shopPage&&current.privacyPage,"shop and privacy panels rebind to new gameplay scene");
 System.IO.File.WriteAllLines("Maintenance/Services/reload-results.txt",report);Time.captureDeltaTime=0;EditorApplication.update-=tick;current.Open("shop");
};EditorApplication.update+=tick;return "Scene reload and singleton persistence check running";
