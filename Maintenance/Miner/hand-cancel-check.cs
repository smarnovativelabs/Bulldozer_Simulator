if(!UnityEditor.EditorApplication.isPlaying||string.IsNullOrEmpty(UnityEditor.SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new System.Exception("Isolated Play required");
var p=UnityEngine.Object.FindFirstObjectByType<Bulldozer.Player.MinerController>();var a=p.visual.GetComponent<Bulldozer.Player.MinerAnimator>();var e=a.equipment;
e.SetCargoCount(0);p.CancelMining();p.touchControls.Move=UnityEngine.Vector2.zero;
var rock=UnityEngine.Object.FindObjectsByType<Bulldozer.Player.MineableRock>(UnityEngine.FindObjectsSortMode.None).First(r=>r.Health==r.maxHealth);
var motor=p.GetComponent<UnityEngine.CharacterController>();motor.enabled=false;p.transform.position=rock.transform.position-UnityEngine.Vector3.forward*1.15f+UnityEngine.Vector3.up*.03f;motor.enabled=true;p.visual.rotation=UnityEngine.Quaternion.identity;UnityEngine.Physics.SyncTransforms();p.autoMine=true;
float began=UnityEngine.Time.time,stageTime=began;int stage=0,health=rock.Health,before=0,peak=0,after=0;var start=p.transform.position;
var report=new System.Collections.Generic.List<string>();UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{
 if(!UnityEditor.EditorApplication.isPlaying||!p){UnityEditor.EditorApplication.update-=tick;return;}
 float now=UnityEngine.Time.time;
 if(now-began>10){report.Add("FAIL cancellation timed out");stage=4;}
 if(stage==0&&p.IsMining){start=p.transform.position;health=rock.Health;p.touchControls.Move=UnityEngine.Vector2.left;stage=1;stageTime=now;}
 else if(stage==1&&now-stageTime>.35f){report.Add((!p.IsMining&&UnityEngine.Vector3.Distance(start,p.transform.position)>.3f?"PASS ":"FAIL ")+"Joystick immediately cancels mining and moves player");report.Add((rock.Health==health?"PASS ":"FAIL ")+"Cancelled windup causes no damage");p.autoMine=false;p.touchControls.Move=UnityEngine.Vector2.zero;stage=2;stageTime=now;}
 else if(stage==2&&now-stageTime>.8f){report.Add((a.LastGripError<.002f?"PASS ":"FAIL ")+"Carry grip recovers after cancelled swing");before=UnityEditor.UnityStats.batches;e.drops.Impact(p.transform.position+p.visual.forward*.6f+UnityEngine.Vector3.up,UnityEngine.Vector3.up,true,0);stage=3;stageTime=now;}
 else if(stage==3){if(now-stageTime<.4f)peak=System.Math.Max(peak,UnityEditor.UnityStats.batches);if(now-stageTime>1){after=UnityEditor.UnityStats.batches;report.Add("EDITOR same-view batches: idle="+before+" effect peak="+peak+" after="+after+" (not device FPS)");stage=4;}}
 if(stage==4){p.autoMine=false;p.touchControls.Move=UnityEngine.Vector2.zero;System.IO.File.WriteAllLines("Maintenance/Miner/hand-cancel-verification.txt",report);UnityEditor.EditorApplication.update-=tick;}
};UnityEditor.EditorApplication.update+=tick;return "Testing cancellation and same-view effect batches";
