var ui=Bulldozer.UI.CommonUI.Instance;
var service=ui.SceneLoading;
var panel=ui.GetComponentInChildren<Bulldozer.UI.LoadingPanel>(true);
var checks=new System.Collections.Generic.List<string>();
void Check(bool condition,string name){if(!condition)throw new System.Exception(name);checks.Add(name);}
System.Collections.IEnumerator Run(){
 float oldScale=UnityEngine.Time.timeScale;
 try{
  float deadline=UnityEngine.Time.realtimeSinceStartup+15;
  while(service.IsLoading && UnityEngine.Time.realtimeSinceStartup<deadline)yield return null;
  Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="1_Gameplay","MainMenu automatically opens Gameplay");
  Check(!ui.BlocksGameplayInput && !panel.IsVisible,"Overlay hidden and input released after startup");
  UnityEngine.Time.timeScale=0f;
  Check(service.TryLoadScene("Assets/0_ProjectData/0_Scenes/1_Gameplay.unity"),"Reload accepted at timeScale zero");
  Check(!service.TryLoadScene("Assets/0_ProjectData/0_Scenes/1_Gameplay.unity"),"Overlapping scene request rejected");
  Check(ui.BlocksGameplayInput,"Loading blocks input immediately");
  deadline=UnityEngine.Time.realtimeSinceStartup+15;
  while(service.IsLoading && UnityEngine.Time.realtimeSinceStartup<deadline)yield return null;
  Check(!service.IsLoading && !ui.BlocksGameplayInput,"Load and reveal complete at timeScale zero");
  UnityEngine.Time.timeScale=oldScale;
  Check(service.TryLoadScene("Assets/0_ProjectData/0_Scenes/0_MainMenu.unity"),"Return to bootstrap scene accepted");
  deadline=UnityEngine.Time.realtimeSinceStartup+15;
  do{yield return null;}while((service.IsLoading || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="1_Gameplay") && UnityEngine.Time.realtimeSinceStartup<deadline);
  Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="1_Gameplay" && !service.IsLoading,"Reentering MainMenu automatically returns to Gameplay");
  Check(UnityEngine.Object.FindObjectsByType<Bulldozer.UI.CommonUI>(UnityEngine.FindObjectsSortMode.None).Length==1,"Exactly one persistent CommonUI after repeated transitions");
  Check(UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(UnityEngine.FindObjectsSortMode.None).Length==1,"Exactly one active EventSystem");
  // Validate the public rejection path and the authored retry button callback.
  Check(!service.TryLoadScene("__CommonUI_MissingScene_Test__"),"Missing scene rejected before starting an operation");
  Check(!service.IsLoading && panel.IsVisible && ui.BlocksGameplayInput,"Failed request presents a recoverable modal");
  var button=panel.GetComponentInChildren<UnityEngine.UI.Button>(true);
  Check(button.gameObject.activeInHierarchy,"Retry button available after rejected request");
  typeof(Bulldozer.UI.SceneLoadingService).GetField("lastTarget",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(service,"Assets/0_ProjectData/0_Scenes/1_Gameplay.unity");
  button.onClick.Invoke();
  Check(service.IsLoading,"Authored retry button starts a new valid request");
  deadline=UnityEngine.Time.realtimeSinceStartup+15;
  while(service.IsLoading && UnityEngine.Time.realtimeSinceStartup<deadline)yield return null;
  Check(!service.IsLoading && service.LastError==null && !ui.BlocksGameplayInput,"Recovery completes and clears error state");
  System.IO.File.WriteAllText("Planning/CommonUI/runtime-validation.txt","PASS\n"+string.Join("\n",checks));
 }finally{UnityEngine.Time.timeScale=oldScale;}
}
service.StartCoroutine(Run());return "Validation running; one missing-scene error is intentional.";
