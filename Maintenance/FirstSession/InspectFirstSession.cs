using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Bulldozer.Player;
public static class InspectFirstSession {
public static string Main(){
if(EditorApplication.isPlaying)throw new System.Exception("Stop Play mode first");
var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if(scene.path!="Assets/0_ProjectData/0_Scenes/1_Gameplay.unity"){
 if(scene.isDirty)throw new System.Exception("Active scene has unsaved edits");
 EditorSceneManager.OpenScene("Assets/0_ProjectData/0_Scenes/1_Gameplay.unity");
}
var player=Object.FindFirstObjectByType<MinerController>();
return "PLAYER "+player.transform.position+" CAMERA "+player.followCamera.transform.position+" BRAINS "+Object.FindObjectsByType<Unity.Cinemachine.CinemachineBrain>(FindObjectsSortMode.None).Length+"\n"+string.Join("\n",Object.FindObjectsByType<MineableRock>(FindObjectsSortMode.None).OrderBy(r=>(r.transform.position-player.transform.position).sqrMagnitude).Take(24).Select(r=>r.blockId+" "+r.transform.position+" HP="+r.maxHealth+" ore="+r.resourceType));
}}
