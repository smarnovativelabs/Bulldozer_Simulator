using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Bulldozer.Player;
public static class SetupFirstSession {
public static string Main(){
if(EditorApplication.isPlaying)throw new System.Exception("Edit mode required");
var player=Object.FindFirstObjectByType<MinerController>();
var rocks=Object.FindObjectsByType<MineableRock>(FindObjectsSortMode.None);
var starter=rocks.Where(r=>r.blockId.StartsWith("quarry-v1-0-")||r.blockId.StartsWith("quarry-v1-1-")).OrderBy(r=>(r.transform.position-player.transform.position).sqrMagnitude).ThenBy(r=>r.blockId).ToArray();
if(starter.Length!=12)throw new System.Exception("Expected 12 starter rocks");
foreach(var rock in rocks){Undo.RecordObject(rock,"First shift pacing");rock.starterHealth=0;EditorUtility.SetDirty(rock);}
for(int i=0;i<starter.Length;i++)starter[i].starterHealth=i<3?2:i<8?4:6;
var guide=Object.FindFirstObjectByType<QuarryGuidance>();
var camera=guide.GetComponent<QuarryCameraGuidance>();if(!camera)camera=Undo.AddComponent<QuarryCameraGuidance>(guide.gameObject);
camera.holdSeconds=2f;camera.guide=guide;camera.cart=Object.FindFirstObjectByType<QuarryWheelbarrow>();EditorUtility.SetDirty(camera);
EditorSceneManager.MarkSceneDirty(player.gameObject.scene);EditorSceneManager.SaveScene(player.gameObject.scene);
return "Saved 12-rock starter shift: 3 x 2 HP, 5 x 4 HP, 4 x 6 HP. First upgrade changes hits to 1/2/3. One-time Cinemachine guidance wired.";
}}

