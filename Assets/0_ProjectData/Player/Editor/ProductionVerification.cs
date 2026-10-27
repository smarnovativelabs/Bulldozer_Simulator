using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Bulldozer.Player.Persistence;
namespace Bulldozer.Player.Editor
{
 public static class ProductionVerification
 {
  static List<string> checks;
  static void Check(bool value,string label){checks.Add((value?"PASS ":"FAIL ")+label);}
  static void Guard(){if(!EditorApplication.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new InvalidOperationException("Isolated Play mode required");}
  static void Place(MinerController player,Vector3 p){player.CancelMining();player.touchControls.Move=Vector2.zero;var cc=player.GetComponent<CharacterController>();cc.enabled=false;player.transform.position=p+Vector3.up*.02f;cc.enabled=true;Physics.SyncTransforms();}
  public static string StoreTests()
  {
   checks=new List<string>();var dir=Path.GetFullPath("Maintenance/Miner/SaveTests/production-"+DateTime.UtcNow.Ticks);Directory.CreateDirectory(dir);var path=Path.Combine(dir,"legacy.json");
   File.WriteAllText(path,"{\"format\":\"bulldozer-mining\",\"version\":1,\"cargo\":3,\"coins\":77}");
   var store=new MiningProgressStore(path);Check(store.Current.version==2&&store.Current.items.SequenceEqual(new[]{0,0,0})&&store.Current.coins==77,"V1 cargo and coins migrate without loss");
   var s=store.Current.Copy();s.damageLevel=2;s.capacityLevel=3;s.refineryInput=new[]{1,2};s.refineryOutput=new[]{9};s.truckItems=new[]{6};s.depletedBlocks=new[]{"test-block"};Check(store.TryCommit(s),"Full production snapshot commits");
   Check(store.TryCommit(2,100),"Legacy balance API remains compatible");var restored=new MiningProgressStore(path).Current;
   Check(restored.damageLevel==2&&restored.capacityLevel==3&&restored.refineryInput.Length==2&&restored.refineryOutput[0]==9&&restored.truckItems[0]==6&&restored.depletedBlocks[0]=="test-block","Balance changes preserve upgrades, queues, truck and mined blocks");
   var bad=restored.Copy();bad.items[0]=99;Check(!store.TryCommit(bad)&&store.Current.items.All(MiningResources.Valid),"Invalid item cannot corrupt inventory");
   string blocked=Path.Combine(dir,"blocked");Directory.CreateDirectory(blocked);var failing=new MiningProgressStore(blocked);var purchase=failing.Current.Copy();purchase.damageLevel=1;Check(!failing.TryCommit(purchase)&&failing.Current.damageLevel==0,"Write failure cannot grant an unsaved upgrade");
   File.WriteAllLines("Maintenance/Miner/production-store-verification.txt",checks);return string.Join("\n",checks);
  }
  public static string PlayTests()
  {
   Guard();checks=new List<string>();var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();var e=p.visual.GetComponent<MinerEquipment>();var machine=UnityEngine.Object.FindFirstObjectByType<MiningProduction>();var upgrade=UnityEngine.Object.FindFirstObjectByType<MinerUpgradeArea>();machine.enabled=false;
   Place(p,Vector3.zero);var initial=e.Snapshot;initial.items=new int[0];initial.coins=100;Check(e.Commit(initial),"Seed isolated economy");
   Check(!upgrade.Purchase(false)&&e.Damage==1,"Cannot purchase outside upgrade area");
   Place(p,upgrade.zone.transform.position);Check(upgrade.Purchase(false)&&e.Damage==2&&e.Coins==80,"Upgrade area purchases damage and spends exactly its cost");
   Check(upgrade.Purchase(true)&&e.Capacity==6&&e.Coins==65,"Rack upgrade increases capacity and spends once");Place(p,Vector3.zero);
   var empty=e.Snapshot;empty.coins=0;e.Commit(empty);Check(!e.TryUpgrade(false)&&e.Damage==2&&e.Coins==0,"Unaffordable upgrade cannot change stats or coins");empty=e.Snapshot;empty.coins=65;e.Commit(empty);
   foreach(int id in new[]{0,1,2})Check(e.TryCollect(id,p.transform.position),"Collect distinct raw resource "+id);
   Check(e.Snapshot.items.SequenceEqual(new[]{0,1,2}),"Cargo retains resource identities");
   Check(!machine.LoadTruck()&&e.CargoCount==3,"Truck rejects unrefined ore");
   for(int i=0;i<3;i++)machine.DepositRaw();Check(e.CargoCount==0&&e.Snapshot.refineryInput.SequenceEqual(new[]{0,1,2}),"Raw inventory transfers to buffered furnace input");
   for(int i=0;i<3;i++)machine.ProcessOne();Check(e.Snapshot.refineryInput.Length==0&&e.Snapshot.refineryOutput.SequenceEqual(new[]{6,7,8}),"Furnace produces corresponding ingot types without selling");
   for(int i=0;i<3;i++)machine.CollectOutput();Check(e.CargoCount==3&&e.Snapshot.items.SequenceEqual(new[]{6,7,8})&&e.Snapshot.refineryOutput.Length==0,"Output moves onto player rack exactly once");
   Check(machine.LoadTruck()&&e.Snapshot.truckItems.Length==1&&e.Coins==65,"Partial truck load waits without premature payment");Check(new MiningProgressStore(e.SavePath).Current.truckItems.Length==1,"Partial truck cargo survives disk reload");
   Check(machine.LoadTruck()&&e.Coins==90&&e.Snapshot.trucksCompleted==1&&e.Snapshot.truckItems.Length==0,"Full truck pays resource values exactly once");Check(!machine.LoadTruck()&&e.Coins==90,"Departing truck cannot pay twice");
   var clear=e.Snapshot;clear.items=new int[0];e.Commit(clear);for(int i=0;i<e.Capacity;i++)e.TryCollect(5,p.transform.position);
   var rock=UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsSortMode.None).First(r=>!r.Depleted&&r.resourceType==0);int health=rock.Health;
   Check(e.IsFull&&!rock.TryHit(e,rock.transform.position)&&rock.Health==health,"Full upgraded rack prevents ore loss");
   clear=e.Snapshot;clear.items=new int[0];e.Commit(clear);int before=e.CargoCount;
   Check(rock.TryHit(e,rock.transform.position)&&rock.Health==health-2,"Purchased damage is applied to rocks");rock.TryHit(e,rock.transform.position);
   Check(rock.Depleted&&e.CargoCount==before+1&&e.IsBlockDepleted(rock.blockId),"Overkill final strike awards one item and persists depletion");
   var save=e.Snapshot;save.items=new[]{1,8};save.refineryInput=new[]{2};save.refineryOutput=new[]{7};save.truckItems=new[]{6};Check(e.Commit(save),"Seed mixed state for restart verification");
   Place(p,new Vector3(0,0,-1));File.WriteAllLines("Maintenance/Miner/production-play-verification.txt",checks);return string.Join("\n",checks);
  }
  public static string VerifyRestart()
  {
   Guard();checks=new List<string>();var e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();var s=e.Snapshot;
   Check(e.Damage==2&&e.Capacity==6&&e.Coins==90,"New Play session restores upgrades and coins");
   Check(s.items.SequenceEqual(new[]{1,8})&&e.cargoSlots.Count(g=>g.activeSelf)==2,"Mixed raw and ingot cargo restores visually");
   Check(s.refineryInput.Length+s.refineryOutput.Length==2&&s.truckItems.SequenceEqual(new[]{6}),"Refinery and partial truck inventories survive restart");
   Check(UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsSortMode.None).Where(r=>s.depletedBlocks.Contains(r.blockId)).All(r=>r.Depleted&&!r.HitCollider.enabled),"Mined blocks remain depleted after restart");
   File.WriteAllLines("Maintenance/Miner/production-restart-verification.txt",checks);return string.Join("\n",checks);
  }
 }
}
