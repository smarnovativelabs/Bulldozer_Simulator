if(!EditorApplication.isPlaying)throw new System.Exception("Play required");
EditorApplication.isPaused=true;Time.captureDeltaTime=1f/30;double deadline=EditorApplication.timeSinceStartup+60;
int frames=Time.frameCount;EditorApplication.CallbackFunction tick=null;
tick=()=>{
 if(!EditorApplication.isPlaying){EditorApplication.update-=tick;return;}
 EditorApplication.Step();
 if(EditorApplication.timeSinceStartup>deadline){EditorApplication.update-=tick;System.IO.File.WriteAllText("Maintenance/Services/prepared-services-results.txt","FAIL startup timeout");return;}
 var ui=Bulldozer.UI.QuarryGameUI.Instance;var root=Bulldozer.Services.QuarryServicesRoot.Instance;
 if(!ui||!root||Time.frameCount<frames+40||Bulldozer.UI.CommonUI.Instance.BlocksGameplayInput)return;
 var report=new System.Collections.Generic.List<string>();void Check(bool ok,string name)=>report.Add((ok?"PASS ":"FAIL ")+name);
 Check(ui.gameObject.scene.path.EndsWith("1_Gameplay.unity"),"MainMenu entry transitions to gameplay");
 Check(UnityEngine.Object.FindObjectsByType<Bulldozer.Services.QuarryServicesRoot>(FindObjectsSortMode.None).Length==1,"one persistent services root after scene transition");
 Check(root.purchasing&&root.onlineServices&&root.connectivity&&root.settings,"all service references assigned");
 Check(root.purchasing.enabled&&root.onlineServices.enabled&&root.connectivity.enabled,"components remain enabled");
 Check(!root.InitializationRequested&&!root.purchasing.InitializationRequested&&!root.onlineServices.InitializationRequested,"startup did not request initialization");
 Check(!root.connectivity.IsReady,"connectivity polling did not initialize");
 ui.Open("shop");ui.shopPage.GetComponent<Bulldozer.Services.QuarryShopPanel>().Retry();
 root.onlineServices.Retry();root.onlineServices.PrivacyOptions();root.onlineServices.SetAttribution(true);root.onlineServices.SendMessage("OnApplicationFocus",true);
 Check(!root.purchasing.InitializationRequested&&!root.onlineServices.InitializationRequested,"UI retry, privacy and focus cannot start services");
 var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
 Check(root.purchasing.GetType().GetField("store",flags).GetValue(root.purchasing)==null,"IAP controller not created");
 Check(root.onlineServices.GetType().GetField("tenjin",flags).GetValue(root.onlineServices)==null,"Tenjin not initialized despite preference toggle");
 Check(!(bool)root.onlineServices.GetType().GetField("firebaseStarting",flags).GetValue(root.onlineServices)&&!(bool)root.onlineServices.GetType().GetField("maxStarting",flags).GetValue(root.onlineServices)&&!root.onlineServices.ConsentBusy,"Firebase, MAX and UMP initialization not running");
 Check(!ui.shopPage.GetComponent<Bulldozer.Services.QuarryShopPanel>().cashButton.interactable,"purchase unavailable until explicit initialization");
 Check(!Bulldozer.UI.CommonUI.Instance.GetComponent<Bulldozer.Services.QuarryStore>()&&!Bulldozer.UI.CommonUI.Instance.GetComponent<Bulldozer.Services.QuarryOnlineServices>(),"CommonUI no longer owns online SDK services");
 System.IO.File.WriteAllLines("Maintenance/Services/prepared-services-results.txt",report);EditorApplication.update-=tick;Time.captureDeltaTime=0;
};EditorApplication.update+=tick;return "Checking prepared-only startup and all implicit initialization paths";
