if(!UnityEditor.EditorApplication.isPlaying||string.IsNullOrEmpty(UnityEditor.SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new System.Exception("Isolated Play required");
var p=UnityEngine.Object.FindFirstObjectByType<Bulldozer.Player.MiningProduction>();var e=UnityEngine.Object.FindFirstObjectByType<Bulldozer.Player.MinerEquipment>();var f=p.rewardFeedback;
var checks=new System.Collections.Generic.List<string>();System.Action<bool,string> check=(ok,label)=>checks.Add((ok?"PASS ":"FAIL ")+label);
p.enabled=false;UnityEditor.EditorApplication.isPaused=true;
var s=e.Snapshot;s.coins=int.MaxValue;s.trucksCompleted=0;s.truckItems=new[]{6};s.items=new[]{7};e.Commit(s);
int bursts=f.BurstsPlayed;check(!p.LoadTruck()&&f.BurstsPlayed==bursts&&!f.IsAnimating,"Rejected payment does not start reward feedback");
s=e.Snapshot;s.coins=0;s.truckItems=System.Array.Empty<int>();s.items=new[]{6,7,8};e.Commit(s);
check(p.LoadTruck()&&f.BurstsPlayed==bursts,"Partial load does not show payout coins");
check(p.LoadTruck()&&f.BurstsPlayed==bursts+1&&e.Coins==25,"Successful full truck starts one burst after payment");
var saved=new Bulldozer.Player.Persistence.MiningProgressStore(e.SavePath).Current;
check(saved.coins==25&&saved.truckItems.Length==0,"Coins are already on disk before the animation finishes");
check(!p.LoadTruck()&&f.BurstsPlayed==bursts+1&&e.Coins==25,"Busy truck cannot duplicate payment or coin burst");
string before=System.IO.File.ReadAllText(e.SavePath);int objects=UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(UnityEngine.FindObjectsSortMode.None).Length;
for(int i=0;i<100;i++)f.PlayReward(p.truckStack.position,1000000);
check(UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(UnityEngine.FindObjectsSortMode.None).Length==objects,"100 bursts create no extra GameObjects");
check(System.IO.File.ReadAllText(e.SavePath)==before&&e.Coins==25,"Repeated visual bursts cannot change saved money");
f.enabled=false;check(!f.IsAnimating&&e.Coins==25&&System.IO.File.ReadAllText(e.SavePath)==before,"Cancelling an effect preserves the completed payment");f.enabled=true;
f.PlayReward(p.truckStack.position,0);check(!f.IsAnimating,"Zero payment produces no animation");
check(!f.raycastTarget&&f.GetComponent<UnityEngine.UI.GraphicRaycaster>()==null,"Reward overlay cannot intercept player input");
e.GetComponent<Bulldozer.Player.MinerMining>().SendMessage("LateUpdate");f.balanceText.ForceMeshUpdate();f.PlayReward(p.truckStack.position,25);typeof(Bulldozer.Player.RewardCoinFeedback).GetField("age",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(f,.45f);f.SendMessage("LateUpdate");UnityEngine.Canvas.ForceUpdateCanvases();
var mesh=f.canvasRenderer.GetMesh();check(mesh.vertexCount>0&&mesh.vertexCount<=312&&mesh.triangles.Length/3<=288&&f.canvasRenderer.materialCount==1,"Eight-coin graphic stays within 312 vertices, 288 triangles and one material");
System.IO.File.WriteAllLines("Maintenance/ArtPolish/reward-feedback-checks.txt",checks);return string.Join("\n",checks);


