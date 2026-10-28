if(!EditorApplication.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new System.Exception("Isolated play required");
var p=UnityEngine.Object.FindFirstObjectByType<Bulldozer.Player.MiningProduction>();var e=UnityEngine.Object.FindFirstObjectByType<Bulldozer.Player.MinerEquipment>();var player=UnityEngine.Object.FindFirstObjectByType<Bulldozer.Player.MinerController>();player.autoMine=false;player.CancelMining();p.enabled=false;
var report=new System.Collections.Generic.List<string>();System.Action<bool,string> check=(ok,msg)=>report.Add((ok?"PASS ":"FAIL ")+msg);
var s=e.Snapshot;s.items=new[]{0,1};s.coins=0;e.Commit(s);
check(p.DepositRaw()&&p.DepositRaw()&&e.CargoCount==0&&e.RefineryInputCount==2,"Raw items first enter persistent refinery input buffer");
check(p.ProcessOne()&&p.ProcessOne()&&p.CollectOutput()&&p.CollectOutput(),"Refinery converts and returns matching finished items");
check(p.LoadTruck()&&p.LoadTruck()&&e.Snapshot.loadingQueue.SequenceEqual(new[]{6,7})&&e.Cash==0,"Loading pad buffers cargo before truck payment");
check(p.TransferToTruck()&&e.Cash==0&&p.TransferToTruck()&&e.Cash==25&&p.delivery.IsBusy,"First truck fills first and pays once");
s=e.Snapshot;s.loadingQueue=new[]{8,9,10,6,6,6,6};e.Commit(s);
check(!p.TransferToTruck()&&e.Cash==25,"Second truck waits for first to clear shared loading bay");
foreach(var t in new[]{p.delivery,p.secondDelivery})check(t.singleLane&&t.body.sharedMesh.triangles.Length/3<5000&&t.body.sharedMaterials.Length==1&&t.wheels.Length==6,t.name+" uses shared lane and bounded single-renderer rig");
p.enabled=true;int start=Time.frameCount;float minGap=999;int phase=0;float refineTime=0;int batches=0,setPass=0;
EditorApplication.isPaused=true;Time.captureDeltaTime=1f/30;EditorApplication.CallbackFunction tick=null;tick=()=>{
 if(!EditorApplication.isPlaying){EditorApplication.update-=tick;return;}
 batches=Mathf.Max(batches,UnityStats.batches);setPass=Mathf.Max(setPass,UnityStats.setPassCalls);
 if(p.delivery.body.enabled&&p.secondDelivery.body.enabled)minGap=Mathf.Min(minGap,Vector3.Distance(p.delivery.transform.position,p.secondDelivery.transform.position));
 int f=Time.frameCount-start;
 if(phase==0&&f>=900){check(e.TrucksCompleted==3&&e.Cash==170&&e.Snapshot.loadingQueue.Length==0,"Three sequential deliveries pay $170 including third-delivery bonus");check(minGap>=4.9f,"Visible trucks retain shared-lane spacing; min="+minGap);check(p.delivery.DistanceTravelled>60&&p.secondDelivery.DistanceTravelled>60,"Both trucks move out and return across actual frames");var disk=new Bulldozer.Player.Persistence.MiningProgressStore(e.SavePath).Current;check(disk.coins==170&&disk.trucksCompleted==3,"Production cash survives disk reload");var state=e.Snapshot;state.refineryInput=new[]{2};e.Commit(state);refineTime=Time.time;phase=1;}
 if(phase==1&&Time.time-refineTime>=1f){check(p.refineryTimer.text.Contains("s")&&e.RefineryOutputCount==0,"Refinery shows current stone countdown before completion");phase=2;}
 if(phase==2&&Time.time-refineTime>=3f){check(e.RefineryInputCount==0&&e.Snapshot.refineryOutput.SequenceEqual(new[]{8}),"Timed production finishes exactly one correct item");check(p.effects.ActiveFlights<=12,"Item transfer effects stay inside pool of 12");report.Add("EDITOR ONLY max batches="+batches+", SetPass="+setPass+"; not device profiling");System.IO.File.WriteAllLines("Maintenance/Miner/sprint1-production-verification.txt",report);p.enabled=false;EditorApplication.update-=tick;Time.captureDeltaTime=0;return;}
 EditorApplication.Step();};EditorApplication.update+=tick;return "Running 1010+ actual frames of shared-lane production";
