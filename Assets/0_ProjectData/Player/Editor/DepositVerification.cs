using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Bulldozer.Player.Persistence;
namespace Bulldozer.Player.Editor
{
 public static class DepositVerification
 {
  const string Key="Bulldozer.Mining.TestSavePath";
  static List<string> checks;static MinerController player;static MinerEquipment equipment;static OreDepositZone zone;static int stage;static float began;
  static void Check(bool pass,string label){checks.Add((pass?"PASS ":"FAIL ")+label);}
  public static string Prepare()
  {
   if(EditorApplication.isPlaying)throw new InvalidOperationException("Prepare before Play mode");
   string path=Path.GetFullPath("Maintenance/Miner/SaveTests/"+DateTime.UtcNow.Ticks+"/play-progress.json");
   SessionState.SetString(Key,path);return path;
  }
  public static string StoreTests()
  {
   checks=new List<string>();string root=Path.GetFullPath("Maintenance/Miner/SaveTests/"+DateTime.UtcNow.Ticks);Directory.CreateDirectory(root);string path=Path.Combine(root,"progress.json");
   var store=new MiningProgressStore(path);Check(store.Current.cargo==0 && store.Current.coins==0,"Fresh game starts empty");
   Check(store.TryCommit(3,40),"Save cargo and coins together");store=new MiningProgressStore(path);Check(store.Current.cargo==3 && store.Current.coins==40,"Fresh store restores both balances");
   Check(store.TryCommit(0,70),"Deposit commits cleared cargo and payment in one snapshot");store=new MiningProgressStore(path);Check(store.Current.cargo==0 && store.Current.coins==70,"Deposit survives reload without duplicated cargo");
   File.WriteAllText(path,"broken JSON");store=new MiningProgressStore(path);Check(store.RecoveredBackup && store.Current.cargo==3 && store.Current.coins==40,"Corrupt primary recovers the previous backup");
   Check(store.TryCommit(0,70) && new MiningProgressStore(path).Current.coins==70,"Recovered save can be committed again");
   string future=Path.Combine(root,"future.json");string futureJson="{\"format\":\"bulldozer-mining\",\"version\":6,\"cargo\":2,\"coins\":999}";File.WriteAllText(future,futureJson);var newer=new MiningProgressStore(future);Check(!newer.TryCommit(0,0) && File.ReadAllText(future)==futureJson,"Future-version save is not overwritten");
   string invalid=Path.Combine(root,"invalid.json");File.WriteAllText(invalid,"{}");var bad=new MiningProgressStore(invalid);Check(!bad.TryCommit(0,0) && File.ReadAllText(invalid)=="{}","Unreadable save is preserved");
   string blocked=Path.Combine(root,"blocked");Directory.CreateDirectory(blocked);var failing=new MiningProgressStore(blocked);Check(!failing.TryCommit(0,30) && failing.Current.coins==0 && failing.Current.cargo==0,"Write failure leaves balances unchanged");
   File.WriteAllLines("Maintenance/Miner/save-store-verification.txt",checks);return string.Join("\n",checks);
  }
  public static void StartPlayTests()
  {
   if(string.IsNullOrEmpty(SessionState.GetString(Key,"")))throw new InvalidOperationException("Prepare an isolated save first");
   checks=new List<string>();player=UnityEngine.Object.FindFirstObjectByType<MinerController>();equipment=player.visual.GetComponent<MinerEquipment>();zone=UnityEngine.Object.FindFirstObjectByType<OreDepositZone>();
   Check(equipment.CargoCount==0 && equipment.Coins==0,"Isolated first launch is empty");
   Check(equipment.TryCollect(player.transform.position) && equipment.TryCollect(player.transform.position),"Collected two ore items");
   Check(!zone.TryDeposit(player) && equipment.CargoCount==2 && equipment.Coins==0,"Cannot sell from outside the deposit area");
   Place(zone.transform.position);began=Time.time;stage=0;EditorApplication.update-=Tick;EditorApplication.update+=Tick;
  }
  static void Place(Vector3 p){player.CancelMining();var cc=player.GetComponent<CharacterController>();cc.enabled=false;player.transform.position=p+Vector3.up*.02f;cc.enabled=true;Physics.SyncTransforms();}
  static void Tick()
  {
   if(!EditorApplication.isPlaying || !player){EditorApplication.update-=Tick;return;}
   if(Time.time-began<1.2f)return;
   if(stage==0)
   {
    Check(equipment.CargoCount==0 && equipment.Coins==20,"Walking into pad automatically sells two ore for 20 coins");
    Check(equipment.cargoSlots.All(s=>!s.activeSelf),"Rack stays empty after in-flight collection is cancelled by deposit");
    Check(!zone.TryDeposit(player) && equipment.Coins==20,"Repeated trigger/empty deposit cannot award duplicate coins");
    Place(Vector3.zero);Check(equipment.TryCollect(player.transform.position),"Collect more ore after unloading");began=Time.time;stage=1;
   }
   else
   {
    var disk=new MiningProgressStore(equipment.SavePath);Check(disk.Current.cargo==1 && disk.Current.coins==20,"Progress is on disk before quitting");
    File.WriteAllLines("Maintenance/Miner/deposit-play-verification.txt",checks);Debug.Log(string.Join("\n",checks));EditorApplication.update-=Tick;
   }
  }
  public static string VerifyRestart()
  {
   if(string.IsNullOrEmpty(SessionState.GetString(Key,"")))throw new InvalidOperationException("No isolated test save");
   var e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();bool pass=e.CargoCount==1 && e.Coins==20 && e.cargoSlots.Count(s=>s.activeSelf)==1;
   string result=(pass?"PASS ":"FAIL ")+"New Play session restores 1 cargo, 20 coins and one visible rack item";File.WriteAllText("Maintenance/Miner/deposit-restart-verification.txt",result);return result;
  }
 }
}



