using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Bulldozer.Player.Editor
{
 public static class MiningVerification
 {
  static MinerController player;static MinerMining mining;static MinerEquipment equipment;static MineableRock rock;
  static List<string> report;static int stage,swings,healthBefore,firstFrame;static float began;static bool earlyChecked;static int hitsBefore;
  static void Check(bool pass,string message){report.Add((pass?"PASS ":"FAIL ")+message);}
  static void Place(Vector3 position){var motor=player.GetComponent<CharacterController>();motor.enabled=false;player.transform.position=position;motor.enabled=true;player.visual.rotation=Quaternion.identity;Physics.SyncTransforms();}
  public static void Start() {
            if(string.IsNullOrEmpty(UnityEditor.SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new System.InvalidOperationException("Prepare an isolated save with DepositVerification.Prepare before running destructive player tests.");
   if(!EditorApplication.isPlaying)throw new InvalidOperationException("Enter Play mode first");
   player=UnityEngine.Object.FindFirstObjectByType<MinerController>();mining=player.visual.GetComponent<MinerMining>();equipment=mining.equipment;rock=UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsSortMode.None).First(r=>r.blockId=="quarry-v1-0-0");
   player.autoMine=false;equipment.SetCargoCount(0);player.touchControls.Move=Vector2.zero;player.touchControls.Sprinting=false;Place(rock.transform.position-Vector3.forward*1.15f+Vector3.up*.02f);report=new List<string>();stage=0;swings=0;firstFrame=Time.frameCount;began=Time.time;EditorApplication.update-=Tick;EditorApplication.update+=Tick;
  }
  static void Begin(){healthBefore=rock.Health;hitsBefore=mining.SuccessfulHits;earlyChecked=false;player.Mine();began=Time.time;}
  static void Tick()
  {
   if(!EditorApplication.isPlaying || !player){EditorApplication.update-=Tick;return;}
   float elapsed=Time.time-began;
   if(stage==0 && elapsed>.6f){Begin();stage=1;}
   else if(stage==1)
   {
    if(!earlyChecked && elapsed>.15f){Check(rock.Health==healthBefore,"No damage before strike "+swings);earlyChecked=true;}
    if(elapsed<1.5f)return;
    Check(rock.Health==healthBefore-1,"One damage per block swing "+swings+" health="+rock.Health+" status="+mining.Status);
    Check(mining.SuccessfulHits==hitsBefore+1,"One contact event per swing "+swings);
    if(++swings<3){Begin();return;}
    Check(rock.Depleted && !rock.HitCollider.enabled && !rock.rockRenderer.enabled,"Depleted rock removes collision and mesh");
    Check(equipment.CargoCount==0 && equipment.Snapshot.groundDrops.Count(d=>d.id.StartsWith(rock.blockId+"/"))==3,"Broken rock creates exactly three ground pickups");
    Check(UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsSortMode.None).Where(r=>r!=rock).All(r=>r.Health==r.maxHealth),"Nearby rocks were not damaged");
    rock=UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsSortMode.None).First(r=>r.blockId=="quarry-v1-0-1");Place(rock.transform.position-Vector3.forward*1.15f+Vector3.up*.02f);began=Time.time;stage=2;
   }
   else if(stage==2 && elapsed>.5f){equipment.SetPickaxeVisible(false);healthBefore=rock.Health;player.Mine();began=Time.time;stage=3;}
   else if(stage==3 && elapsed>1.2f){Check(rock.Health==healthBefore && !player.IsMining,"Hidden pickaxe cannot damage rocks");equipment.SetPickaxeVisible(true);Begin();stage=4;}
   else if(stage==4 && elapsed>1.5f){Check(player.MineWall && rock.Health==healthBefore-1,"Wall rock selects wall animation and receives contact damage");equipment.SetCargoCount(equipment.Capacity);healthBefore=rock.Health;player.Mine();began=Time.time;stage=5;}
   else if(stage==5 && elapsed>1.2f){Check(!player.IsMining && rock.Health==healthBefore && equipment.CargoCount==equipment.Capacity,"Full rack blocks damage without losing ore");equipment.SetCargoCount(1);Place(new Vector3(0,.02f,-5));healthBefore=rock.Health;hitsBefore=mining.SuccessfulHits;player.Mine();began=Time.time;stage=6;}
   else if(stage==6 && elapsed>1.5f)
   {
    Check(rock.Health==healthBefore && mining.SuccessfulHits==hitsBefore && equipment.CargoCount==1,"Out-of-range swing cannot damage or collect");
    Check(Time.frameCount-firstFrame>30,"Player loop advanced "+(Time.frameCount-firstFrame)+" frames");
    File.WriteAllLines("Maintenance/Miner/mining-verification.txt",report);Debug.Log(string.Join("\n",report));EditorApplication.update-=Tick;
   }
  }
 }
}


