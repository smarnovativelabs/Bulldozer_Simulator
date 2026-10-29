if(!EditorApplication.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new System.Exception("Isolated play required");
var p=UnityEngine.Object.FindFirstObjectByType<Bulldozer.Player.MinerController>();var a=p.visual.GetComponent<Bulldozer.Player.MinerAnimator>();var m=p.visual.GetComponent<Bulldozer.Player.MinerMining>();
var rocks=UnityEngine.Object.FindObjectsByType<Bulldozer.Player.MineableRock>(FindObjectsSortMode.None).Where(r=>r.naturalBoulder).OrderBy(r=>r.blockId).ToArray();
p.autoMine=false;p.CancelMining();p.touchControls.Move=Vector2.zero;
foreach(var r in UnityEngine.Object.FindObjectsByType<Bulldozer.Player.MineableRock>(FindObjectsSortMode.None))r.HitCollider.enabled=false;
var report=new System.Collections.Generic.List<string>();int test=0,phase=0,start=Time.frameCount,hits=0;float grip=0,angle=0,hitTime=-1;Quaternion tool=a.toolGrip.localRotation;float began=0;
EditorApplication.isPaused=true;Time.captureDeltaTime=1f/30;
EditorApplication.CallbackFunction tick=null;tick=()=>{if(!EditorApplication.isPlaying){EditorApplication.update-=tick;return;}int f=Time.frameCount-start;
 if(phase==0){var r=rocks[test/4];r.HitCollider.enabled=true;typeof(Bulldozer.Player.MineableRock).GetProperty("Health").SetValue(r,100);var dir=Quaternion.Euler(0,(test%4)*90,0)*Vector3.forward;var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=r.transform.position-dir*1.05f+Vector3.up*.03f;cc.enabled=true;p.visual.rotation=Quaternion.LookRotation(dir);Physics.SyncTransforms();phase=1;}
 if(phase==1&&f>=25){hits=m.SuccessfulHits;began=Time.time;p.Mine();phase=2;}
 if(phase==2){grip=Mathf.Max(grip,a.LastGripError);angle=Mathf.Max(angle,Quaternion.Angle(tool,a.toolGrip.localRotation));if(m.SuccessfulHits>hits&&hitTime<0)hitTime=Time.time-began;}
 if(phase==2&&f>=100){report.Add((m.SuccessfulHits-hits==1?"PASS ":"FAIL ")+rocks[test/4].blockId+" side="+(test%4)*90+" hits="+(m.SuccessfulHits-hits)+" time="+hitTime.ToString("F3")+" grip="+grip.ToString("F4")+" toolTwist="+angle.ToString("F2"));rocks[test/4].HitCollider.enabled=false;test++;phase=0;start=Time.frameCount;grip=angle=0;hitTime=-1;
 if(test==48){System.IO.File.WriteAllLines("Maintenance/Miner/interaction-all-natural-verification.txt",report);EditorApplication.update-=tick;Time.captureDeltaTime=0;return;}}
 EditorApplication.Step();};EditorApplication.update+=tick;return "Testing 48 animated natural-rock approaches";

