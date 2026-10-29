if(EditorApplication.isPlaying)throw new System.Exception("Edit mode required");
string path="Assets/0_ProjectData/0_Scenes/1_Gameplay.unity";var scene=UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);bool opened=!scene.isLoaded;if(opened)scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path,UnityEditor.SceneManagement.OpenSceneMode.Additive);
try{var ui=scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<Bulldozer.UI.QuarryGameUI>(true)).Single();
var rect=(RectTransform)ui.volume.transform;Undo.RecordObject(rect,"Expand slider touch area");float height=rect.rect.height;if(height<56){rect.sizeDelta=new Vector2(rect.sizeDelta.x,56);rect.anchoredPosition+=Vector2.up*((56-height)/2);}
var hit=ui.volume.GetComponent<UnityEngine.UI.Image>();if(!hit)hit=Undo.AddComponent<UnityEngine.UI.Image>(ui.volume.gameObject);Undo.RecordObject(hit,"Slider touch surface");hit.color=Color.clear;hit.raycastTarget=true;
foreach(var toggle in new[]{ui.soundToggle,ui.vibrationToggle}){Undo.RecordObject(toggle.track,"Expand toggle touch bounds");toggle.track.raycastPadding=new Vector4(0,-7,0,-7);}
foreach(var patch in scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<MeshRenderer>(true)).Where(x=>x.name=="Embedded Minerals"&&x.GetComponentInParent<Bulldozer.Player.MineableRock>())){Undo.RecordObject(patch,"Reduce mineral patch shadow passes");patch.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;PrefabUtility.RecordPrefabInstancePropertyModifications(patch);}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
}finally{if(opened)UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene,true);}
return "Slider has 56-unit transparent touch surface; toggles have 56-unit effective height, artwork unchanged";

