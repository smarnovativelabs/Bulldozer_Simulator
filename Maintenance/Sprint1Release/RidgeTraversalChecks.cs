using System;
using System.IO;
using UnityEngine;
using Bulldozer.Player;
public static class RidgeTraversalChecks {
public static string Main(){
var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();var area=UnityEngine.Object.FindFirstObjectByType<QuarryExpansion>();
if(!Application.isPlaying||area.gate.activeSelf||!area.equipment.SavePath.Contains("Sprint1Progression-"))throw new Exception("Isolated open-ridge Play session required");
var cc=p.GetComponent<CharacterController>();var original=p.transform.position;bool enabled=p.enabled;p.enabled=false;
var lines=new System.Collections.Generic.List<string>();
Action<bool,string> check=(ok,message)=>{lines.Add((ok?"PASS ":"FAIL ")+message);File.WriteAllLines("Maintenance/Sprint1Release/ridge-traversal-checks.txt",lines);if(!ok)throw new Exception(message);};
Action<Vector3> place=v=>{cc.enabled=false;p.transform.position=v;cc.enabled=true;Physics.SyncTransforms();};
Action<Vector3,int> move=(v,n)=>{for(int i=0;i<n;i++)cc.Move(v);};
try{
place(new Vector3(12,.05f,-3));move(Vector3.right*.1f,85);check(p.transform.position.x>20,"Player reaches east approach around existing bank");
move(Vector3.forward*.1f,70);check(p.transform.position.z>3.5f,"Player physically passes through unlocked entrance");
place(new Vector3(20.5f,.05f,7));move(Vector3.left*.1f,80);check(p.transform.position.x>17.5f,"West ridge boundary blocks player");
place(new Vector3(20.5f,.05f,7));move(Vector3.right*.1f,80);check(p.transform.position.x<23.5f,"East ridge boundary blocks player");
place(new Vector3(20.5f,.05f,7));move(Vector3.forward*.1f,80);check(p.transform.position.z<10.5f,"North ridge boundary blocks player");
lines.Add("COMPLETE character-controller traversal and perimeter checks");File.WriteAllLines("Maintenance/Sprint1Release/ridge-traversal-checks.txt",lines);return lines[lines.Count-1];
}finally{place(original);p.enabled=enabled;}
}
}
