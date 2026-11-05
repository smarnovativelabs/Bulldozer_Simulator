using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Bulldozer.Player;
using Bulldozer.Player.Persistence;
public static class OptionalUpgradeCheck {
 public static string Main(){
  if(!Application.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new Exception("Isolated Play mode required");
  var go=new GameObject("Optional upgrade test");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<OptionalUpgradeRunner>().Begin();return "Testing starter rocks without upgrades";
 }
}
public class OptionalUpgradeRunner:MonoBehaviour {
 public void Begin(){StartCoroutine(Run());}
 IEnumerator Run(){
 var e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();var s=MiningSnapshot.Create(0,0);s.firstRefineryDeposit=true;s.guidanceSeenMask=7;e.Commit(s);
 SceneManager.LoadSceneAsync(e.gameObject.scene.path);yield return new WaitForSecondsRealtime(3);
 e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();var player=UnityEngine.Object.FindFirstObjectByType<MinerController>();player.autoMine=false;
 var rocks=UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsSortMode.None).Where(r=>!string.IsNullOrEmpty(r.blockId)&&r.Available(e.Snapshot)).ToArray();
 int hits=0;foreach(var r in rocks)while(!r.Depleted){if(!r.TryHit(e,r.HitCollider.bounds.center))throw new Exception("Unupgraded mining failed: "+r.blockId);hits++;}
 bool ok=rocks.Length==12&&hits==50&&e.DamageLevel==0&&e.Snapshot.groundDrops.Length==36;
 File.WriteAllText("Maintenance/FirstSession/optional-upgrade-check.txt",(ok?"PASS":"FAIL")+" All 12 starter rocks mined in 50 hits with base pickaxe; no upgrade purchase; all 36 ore retained.\n");
 if(!ok)throw new Exception("Optional upgrade test failed");Destroy(gameObject);
 }
}
