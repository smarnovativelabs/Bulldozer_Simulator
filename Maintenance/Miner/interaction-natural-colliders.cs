if(EditorApplication.isPlaying)throw new System.Exception("Edit mode required");
int count=0,triangles=0;
foreach(var rock in UnityEngine.Object.FindObjectsByType<Bulldozer.Player.MineableRock>(FindObjectsSortMode.None).Where(r=>r.naturalBoulder))
{
    Undo.RecordObject(rock,"Match natural rock contact to visible mesh");
    var visual=rock.rockRenderer.gameObject;var mesh=visual.GetComponent<MeshFilter>().sharedMesh;
    var surface=visual.GetComponent<MeshCollider>();if(!surface)surface=Undo.AddComponent<MeshCollider>(visual);
    Undo.RecordObject(surface,"Natural rock convex contact");surface.sharedMesh=mesh;surface.convex=true;surface.isTrigger=false;
    var box=rock.GetComponent<BoxCollider>();Undo.RecordObject(box,"Retire square boulder collider");box.enabled=false;
    rock.contactCollider=surface;EditorUtility.SetDirty(rock);count++;triangles+=mesh.triangles.Length/3;
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
return count+" static convex natural rock colliders, source mesh triangles total="+triangles+"; no rigidbodies added";
