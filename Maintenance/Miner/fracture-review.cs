if(!UnityEditor.EditorApplication.isPlaying||string.IsNullOrEmpty(UnityEditor.SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new System.Exception("Isolated Play required");
var p=UnityEngine.Object.FindFirstObjectByType<Bulldozer.Player.MinerController>();var e=p.visual.GetComponent<Bulldozer.Player.MinerEquipment>();var m=p.visual.GetComponent<Bulldozer.Player.MinerMining>();var fx=e.drops.fracture;var cam=p.followCamera;
var rock=UnityEngine.Object.FindObjectsByType<Bulldozer.Player.MineableRock>(UnityEngine.FindObjectsSortMode.None).First(r=>r.blockId=="quarry-v1-0-0");p.autoMine=false;p.CancelMining();p.touchControls.Move=UnityEngine.Vector2.zero;
var motor=p.GetComponent<UnityEngine.CharacterController>();motor.enabled=false;p.transform.position=rock.transform.position-UnityEngine.Vector3.forward*1.15f+UnityEngine.Vector3.up*.03f;motor.enabled=true;p.visual.rotation=UnityEngine.Quaternion.identity;UnityEngine.Physics.SyncTransforms();
System.Action<string> capture=name=>{var c=UnityEngine.Camera.main;var oldTarget=c.targetTexture;var rt=new UnityEngine.RenderTexture(1280,720,24);var old=UnityEngine.RenderTexture.active;var tex=new UnityEngine.Texture2D(1280,720,UnityEngine.TextureFormat.RGB24,false);try{c.targetTexture=rt;c.Render();UnityEngine.RenderTexture.active=rt;tex.ReadPixels(new UnityEngine.Rect(0,0,1280,720),0,0);tex.Apply();System.IO.File.WriteAllBytes("Maintenance/Miner/"+name+".png",tex.EncodeToPNG());}finally{c.targetTexture=oldTarget;UnityEngine.RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(tex);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}};
cam.enabled=false;cam.distance=8;cam.startPitch=48;cam.startYaw=0;cam.enabled=true;
float started=UnityEngine.Time.time,stageTime=started,changed=-1,breakTime=-1;int stage=0,previous=rock.Health,frame=UnityEngine.Time.frameCount,hits=m.SuccessfulHits,shots=0,maxPieces=0,baseBatches=0,peakBatches=0;int originalObjects=fx.transform.childCount;
var rockBounds=rock.HitCollider.bounds;var report=new System.Collections.Generic.List<string>();UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{
 if(!UnityEditor.EditorApplication.isPlaying||!p){UnityEditor.EditorApplication.update-=tick;return;}
 float now=UnityEngine.Time.time;
 if(now-started>20){report.Add("FAIL timeout stage "+stage);stage=4;}
 if(stage==0&&now-stageTime>.5f){capture("camera-before-8m");cam.enabled=false;cam.distance=5.8f;cam.startPitch=46;cam.startYaw=-22;cam.enabled=true;stage=1;stageTime=now;}
 else if(stage==1&&now-stageTime>.5f){capture("camera-after-6m");baseBatches=UnityEditor.UnityStats.batches;p.autoMine=true;stage=2;}
 else if(stage==2){
  if(rock.Health!=previous){previous=rock.Health;changed=now;}
  if(changed>0&&now-changed>.28f&&!rock.Depleted){capture("fracture-cracks-"+rock.Health);changed=-1;}
  if(rock.Depleted){breakTime=now;p.autoMine=false;stage=3;}
 }
 else if(stage==3){
  maxPieces=System.Math.Max(maxPieces,fx.ActiveCount);peakBatches=System.Math.Max(peakBatches,UnityEditor.UnityStats.batches);
  float age=now-breakTime;
  if(shots==0&&age>.16f){capture("fracture-lift");shots++;}
  else if(shots==1&&age>.38f){capture("fracture-apex");shots++;}
  else if(shots==2&&age>.75f){capture("fracture-fall");shots++;}
  if(age>1.5f){capture("fracture-settled");System.Action<bool,string> check=(ok,label)=>report.Add((ok?"PASS ":"FAIL ")+label);
   check(m.SuccessfulHits-hits==3,"Three physical pickaxe strikes break the block");check(maxPieces==8,"Eight large pieces replace the block");check(fx.HighestLift>.25f,"Visible upward lift "+fx.HighestLift.ToString("F3")+" m");check(fx.ActiveCount==0,"Temporary fragments finish and renderer releases");check(e.Snapshot.groundDrops.Length==3&&e.CargoCount==0,"Exactly three saved collectible ore pieces remain");check(!rock.HitCollider.enabled,"Broken block collider removed");check(UnityEngine.Time.frameCount-frame>90,"Advancing Play frames "+(UnityEngine.Time.frameCount-frame));
   for(int i=0;i<20;i++)fx.Burst(rockBounds,UnityEngine.Vector3.back);check(fx.ActiveCount==24&&fx.transform.childCount==originalObjects,"Repeated breaks reuse a fixed 24-piece pool");
   report.Add("BUDGET combined fracture renderer: "+fx.GetComponent<UnityEngine.MeshFilter>().sharedMesh.vertexCount+" vertices / "+fx.GetComponent<UnityEngine.MeshFilter>().sharedMesh.triangles.Length/3+" triangles; one shared material; no rigidbodies");report.Add("EDITOR batches before="+baseBatches+" effect peak="+peakBatches+" (includes animation/crack changes, not device FPS)");stage=4;
  }
 }
 if(stage==4){p.autoMine=false;System.IO.File.WriteAllLines("Maintenance/Miner/fracture-review-verification.txt",report);UnityEditor.EditorApplication.update-=tick;}
};UnityEditor.EditorApplication.update+=tick;return "Camera comparison and physical fracture review running";
