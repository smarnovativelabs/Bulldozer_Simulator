if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play required");
var c=UnityEngine.Object.FindFirstObjectByType<Bulldozer.Player.QuarryWheelbarrow>();var p=c.player;var e=c.equipment;var prod=UnityEngine.Object.FindFirstObjectByType<Bulldozer.Player.MiningProduction>();var anim=p.visual.GetComponent<Bulldozer.Player.MinerAnimator>();var mine=p.visual.GetComponent<Bulldozer.Player.MinerMining>();
p.autoMine=false;p.CancelMining();p.touchControls.Move=Vector2.zero;
System.Action<Vector3> place=pos=>{var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=pos;cc.enabled=true;p.visual.rotation=Quaternion.identity;Physics.SyncTransforms();};
place(new Vector3(-3,.03f,-3));c.transform.position=new Vector3(-3,.05f,-1.95f);var s=e.Snapshot;s.coins=100;s.items=System.Array.Empty<int>();e.Commit(s);c.Interact();
s=e.Snapshot;s.groundDrops=new[]{new Bulldozer.Player.Persistence.GroundOreState{id="cart-forward-test",item=0,position=new Vector3(-3,.13f,-1.45f)}};e.Commit(s);e.drops.Sync(s.groundDrops,Vector3.zero,false);
var report=new System.Collections.Generic.List<string>();int start=Time.frameCount;int stage=0,shot=0;float maxGrip=0,maxAngle=0;var initialRot=anim.toolGrip.localRotation;int hits=mine.SuccessfulHits;
UnityEditor.EditorApplication.isPaused=true;Time.captureDeltaTime=1f/30;
UnityEditor.EditorApplication.CallbackFunction tick=null;tick=()=>{
 if(!UnityEditor.EditorApplication.isPlaying){UnityEditor.EditorApplication.update-=tick;return;}
 int f=Time.frameCount-start;maxGrip=Mathf.Max(maxGrip,anim.LastGripError);maxAngle=Mathf.Max(maxAngle,Quaternion.Angle(initialRot,anim.toolGrip.localRotation));
 if(stage==0&&f>=45){report.Add((e.CargoCount==1?"PASS":"FAIL")+" Cart collects ore 1.55m ahead of player; cargo="+e.CargoCount);Bulldozer.Player.Editor.HandMiningPolish.Capture("wheelbarrow-weathered",true);place(prod.inputZone.bounds.center-Vector3.forward*1.05f+Vector3.up*.02f);var pos=p.transform.position;pos.y=.03f;place(pos);stage++;}
 if(stage==1&&f>=100){report.Add((e.CargoCount==0&&e.RefineryInputCount+e.RefineryOutputCount>0?"PASS":"FAIL")+" Cart transfers at refinery trigger; carried="+e.CargoCount);c.Interact();stage++;}
 if(stage==2&&f>=120){foreach(var r in UnityEngine.Object.FindObjectsByType<Bulldozer.Player.MineableRock>(FindObjectsSortMode.None))if(r.blockId=="quarry-v1-0-0"){place(r.transform.position-Vector3.forward*1.15f+Vector3.up*.03f);break;}stage++;}
 if(stage==3&&f>=140){p.Mine();stage++;}
 if(stage==4&&f>=146+shot*5&&shot<5){Bulldozer.Player.Editor.HandMiningPolish.Capture("grip-checked-"+shot,true);shot++;}
 if(f>=210){report.Add((mine.SuccessfulHits>hits?"PASS":"FAIL")+" Swing contacts rock; hits="+(mine.SuccessfulHits-hits));report.Add("Maximum IK target error="+maxGrip+"m; pickaxe rotation relative to palm="+maxAngle+" degrees");System.IO.File.WriteAllLines("Maintenance/Miner/equipment-polish-verification.txt",report);UnityEditor.EditorApplication.update-=tick;Time.captureDeltaTime=0;return;}
 UnityEditor.EditorApplication.Step();};UnityEditor.EditorApplication.update+=tick;return "Started 210-frame cart and swing checks";
