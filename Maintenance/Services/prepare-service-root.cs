if(EditorApplication.isPlaying)throw new System.Exception("Edit Mode required");
string commonPath="Assets/0_ProjectData/UI/Common/CommonUI.prefab";
string prefabPath="Assets/0_ProjectData/Services/Prefabs/QuarryServices.prefab";
if(!AssetDatabase.IsValidFolder("Assets/0_ProjectData/Services/Prefabs"))AssetDatabase.CreateFolder("Assets/0_ProjectData/Services","Prefabs");
var common=PrefabUtility.LoadPrefabContents(commonPath);
var prepared=new GameObject("Persistent Systems");
try {
 var owner=prepared.AddComponent<Bulldozer.Services.QuarryServicesRoot>();
 owner.settings=AssetDatabase.LoadAssetAtPath<Bulldozer.Services.QuarryServiceSettings>("Assets/Resources/QuarryServiceSettings.asset");
 GameObject Child(string name){var g=new GameObject(name);g.transform.SetParent(prepared.transform,false);return g;}
 owner.purchasing=Child("Purchasing - Unity IAP").AddComponent<Bulldozer.Services.QuarryStore>();
 owner.onlineServices=Child("Online Services - Firebase Ads Consent Tenjin").AddComponent<Bulldozer.Services.QuarryOnlineServices>();
 owner.connectivity=Child("Internet Connectivity").AddComponent<Bulldozer.Services.InternetConnectivity>();
 var oldStore=common.GetComponent<Bulldozer.Services.QuarryStore>();if(oldStore)EditorUtility.CopySerialized(oldStore,owner.purchasing);
 var oldOnline=common.GetComponent<Bulldozer.Services.QuarryOnlineServices>();if(oldOnline)EditorUtility.CopySerialized(oldOnline,owner.onlineServices);
 var oldInternet=common.GetComponent<Bulldozer.Services.InternetConnectivity>();if(oldInternet)EditorUtility.CopySerialized(oldInternet,owner.connectivity);
 owner.purchasing.settings=owner.settings;owner.onlineServices.settings=owner.settings;
 owner.purchasing.enabled=owner.onlineServices.enabled=owner.connectivity.enabled=true;
 PrefabUtility.SaveAsPrefabAsset(prepared,prefabPath);
 if(oldStore)UnityEngine.Object.DestroyImmediate(oldStore);if(oldOnline)UnityEngine.Object.DestroyImmediate(oldOnline);if(oldInternet)UnityEngine.Object.DestroyImmediate(oldInternet);
 PrefabUtility.SaveAsPrefabAsset(common,commonPath);
}finally{UnityEngine.Object.DestroyImmediate(prepared);PrefabUtility.UnloadPrefabContents(common);}
var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
foreach(string path in new[]{"Assets/0_ProjectData/0_Scenes/0_MainMenu.unity","Assets/0_ProjectData/0_Scenes/1_Gameplay.unity"}) {
 var scene=UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);bool opened=!scene.isLoaded;
 if(opened)scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path,UnityEditor.SceneManagement.OpenSceneMode.Additive);
 var existing=scene.GetRootGameObjects().FirstOrDefault(g=>g.GetComponent<Bulldozer.Services.QuarryServicesRoot>());
 if(!existing)existing=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);
 UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
 if(!opened&&path.EndsWith("0_MainMenu.unity")){Selection.activeGameObject=existing;EditorGUIUtility.PingObject(existing);}
 if(opened)UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene,true);
}
AssetDatabase.SaveAssets();return "Prepared Persistent Systems in MainMenu and Gameplay. CommonUI now owns UI only. No initialization call wired.";
