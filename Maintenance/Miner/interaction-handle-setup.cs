if(EditorApplication.isPlaying)throw new System.Exception("Edit mode required");
var cart=UnityEngine.Object.FindFirstObjectByType<Bulldozer.Player.QuarryWheelbarrow>();
foreach(var t in cart.GetComponentsInChildren<Transform>(true))
{
 if(t.name=="Extended ergonomic handle")
 {Undo.RecordObject(t,"Raise cart handle to comfortable reach");var pos=t.localPosition;pos.y=1f;pos.z=-.425f;t.localPosition=pos;t.localRotation=Quaternion.Euler(29,0,0);t.localScale=new Vector3(.06f,.06f,.66f);}
 if(t==cart.leftGrip||t==cart.rightGrip||t.name=="Safety grip")
 {Undo.RecordObject(t,"Align wrist to raised safety grip");var pos=t.localPosition;pos.y=1.15f;t.localPosition=pos;if(t.name=="Safety grip")t.localRotation=Quaternion.Euler(29,0,0);}
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(cart.gameObject.scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(cart.gameObject.scene);AssetDatabase.SaveAssets();return "Raised and angled both physical handles and wrist targets by 30 cm";
