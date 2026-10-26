// Execute through Unity Pipeline script eval_file. Creates only new Sprint1 assets; refuses overwrite.
var root="Assets/Sprint1Foundation";
var scenePath=root+"/CharacterAnimationTest.unity";
if(System.IO.File.Exists(scenePath)) throw new System.InvalidOperationException("Test scene already exists; refusing overwrite.");
if(UnityEditor.EditorApplication.isPlaying || Enumerable.Range(0,UnityEngine.SceneManagement.SceneManager.sceneCount).Any(i=>UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)) throw new System.InvalidOperationException("Require edit mode and no unsaved scenes.");
System.IO.Directory.CreateDirectory(root+"/Generated");UnityEditor.AssetDatabase.Refresh();
var model=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(root+"/Art/QuaterniusWorker/Worker.fbx");
var clips=UnityEditor.AssetDatabase.LoadAllAssetsAtPath(root+"/Art/QuaterniusWorker/WorkerTestMotions.fbx").OfType<UnityEngine.AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
var avatar=UnityEditor.AssetDatabase.LoadAllAssetsAtPath(root+"/Art/QuaterniusWorker/Worker.fbx").OfType<UnityEngine.Avatar>().First();
if(!avatar.isHuman || !avatar.isValid || clips.Length!=4 || clips.Any(c=>!c.isHumanMotion))throw new System.Exception("Invalid humanoid animation source");
var original=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
var scene=UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,UnityEditor.SceneManagement.NewSceneMode.Additive);
UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
var shader=UnityEngine.Shader.Find("Universal Render Pipeline/Lit");
var materials=new System.Collections.Generic.Dictionary<string,UnityEngine.Material>();
foreach(var src in model.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>().SelectMany(r=>r.sharedMaterials).Distinct()){
 var m=new UnityEngine.Material(shader);m.name=src.name;m.SetColor("_BaseColor",src.color);m.SetFloat("_Smoothness",0.15f);
 UnityEditor.AssetDatabase.CreateAsset(m,root+"/Generated/"+m.name+".mat");materials.Add(src.name,m);
}
var names=new[]{"Idle","Walk","Run","Interact"};
for(int index=0;index<4;index++){
 var clip=clips.First(c=>c.name.EndsWith("Sprint1_"+names[index]));
 var controller=UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(root+"/Generated/"+names[index]+".controller");
 var state=controller.layers[0].stateMachine.AddState(names[index]);state.motion=clip;controller.layers[0].stateMachine.defaultState=state;
 var obj=(UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(model,scene);obj.name=names[index]+" Worker";obj.transform.position=new UnityEngine.Vector3((index-1.5f)*1.6f,0,0);
 var animator=obj.GetComponent<UnityEngine.Animator>();animator.avatar=avatar;animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=UnityEngine.AnimatorCullingMode.AlwaysAnimate;
 foreach(var renderer in obj.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>())renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>materials[m.name]).ToArray();
 if(index==0){var old=obj.transform.position;obj.transform.position=UnityEngine.Vector3.zero;UnityEditor.PrefabUtility.SaveAsPrefabAsset(obj,root+"/Generated/WorkerTest.prefab");obj.transform.position=old;}
}
var cam=new UnityEngine.GameObject("Animation Test Camera").AddComponent<UnityEngine.Camera>();cam.tag="MainCamera";cam.transform.position=new UnityEngine.Vector3(0,2.4f,-8);cam.transform.LookAt(new UnityEngine.Vector3(0,.95f,0));cam.orthographic=true;cam.orthographicSize=3.4f;cam.clearFlags=UnityEngine.CameraClearFlags.SolidColor;cam.backgroundColor=new UnityEngine.Color(.08f,.12f,.16f);cam.nearClipPlane=.1f;cam.farClipPlane=30;cam.gameObject.AddComponent<UnityEngine.AudioListener>();
var light=new UnityEngine.GameObject("Test Light").AddComponent<UnityEngine.Light>();light.type=UnityEngine.LightType.Directional;light.transform.rotation=UnityEngine.Quaternion.Euler(45,-35,0);light.intensity=2;
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene,scenePath);UnityEditor.AssetDatabase.SaveAssets();
UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene,true);UnityEngine.SceneManagement.SceneManager.SetActiveScene(original);
return new{scenePath,avatarValid=avatar.isValid,clips=clips.Select(c=>new{c.name,c.length}).ToArray(),materials=materials.Count};
