using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using Bulldozer.Player;
using Bulldozer.Player.Persistence;
public static class ResetForMatrix {public static string Main()=>ResetTesterFixture.Run("matrix-progress");}
public static class ResetForCart {public static string Main()=>ResetTesterFixture.Run("cart-progress");}
public static class ResetTesterFixture {
 public static string Run(string stage){
  if(!Application.isPlaying||!SessionState.GetString("Bulldozer.Mining.TestSavePath","").Contains("TesterReadiness"))throw new Exception("Isolated play required");
  var ids=SessionState.GetString("ManualPlaytest.InputDevices","").Split(',');foreach(var d in UnityEngine.InputSystem.InputSystem.devices)if(ids.Contains(d.deviceId.ToString()))UnityEngine.InputSystem.InputSystem.EnableDevice(d);SessionState.EraseString("ManualPlaytest.InputDevices");
  var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();if(p){p.autoMine=false;p.CancelMining();p.touchControls.Move=Vector2.zero;}
  var s=MiningSnapshot.Create(0,1000);s.roundIndex=3;s.shortFirstShift=true;s.gradualShifts=true;s.firstRefineryDeposit=true;s.guidanceSeenMask=15;s.copperRidgeUnlocked=true;s.wheelbarrowUnlocked=true;
  string file=Path.GetFullPath("Maintenance/TesterReadiness/"+stage+".json");File.WriteAllText(file,JsonUtility.ToJson(s,true));SessionState.SetString("Bulldozer.Mining.TestSavePath",file);SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().path);return "Loading clean fixture "+stage;
 }
}
