using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Bulldozer.Player;
public static class OuterBoundaryChecks {
public static string Main(){
if(!Application.isPlaying)throw new Exception("Play mode required");
var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();var cart=UnityEngine.Object.FindFirstObjectByType<QuarryWheelbarrow>();var cc=p.GetComponent<CharacterController>();
var original=p.transform.position;var rotation=p.visual.rotation;bool enabled=p.enabled;p.enabled=false;p.touchControls.Move=Vector2.zero;
var log=new List<string>();int failures=0;
Action<Vector3> place=v=>{cc.enabled=false;p.transform.position=v;cc.enabled=true;Physics.SyncTransforms();};
try{
foreach(bool pushing in new[]{false,true})foreach(float height in new[]{.05f,4f,8f})foreach(float z in new[]{-9f,-6f,0f,8f,13f})foreach(int side in new[]{-1,1}){
place(new Vector3(side<0?-15.8f:23.8f,height,z));p.visual.rotation=Quaternion.LookRotation(Vector3.right*side);
for(int i=0;i<120;i++){var move=Vector3.right*side*.2f;var turn=p.visual.rotation;if(pushing)cart.ConstrainMovement(ref move,ref turn);cc.Move(move);}
bool ok=p.transform.position.x>-17&&p.transform.position.x<25;if(!ok)failures++;log.Add((ok?"PASS ":"FAIL ")+(pushing?"Cart sweep":"Player")+" side="+side+" z="+z+" height="+height+" stopped="+p.transform.position);
}
foreach(bool pushing in new[]{false,true})foreach(float x in new[]{-16f,0f,16f,24f})foreach(int side in new[]{-1,1}){
place(new Vector3(x,.05f,side<0?-9f:12.5f));p.visual.rotation=Quaternion.LookRotation(Vector3.forward*side);
for(int i=0;i<120;i++){var move=Vector3.forward*side*.2f;var turn=p.visual.rotation;if(pushing)cart.ConstrainMovement(ref move,ref turn);cc.Move(move);}
bool ok=p.transform.position.z>-10.6f&&p.transform.position.z<14;if(!ok)failures++;log.Add((ok?"PASS ":"FAIL ")+(pushing?"Cart sweep":"Player")+" end="+side+" x="+x+" stopped="+p.transform.position);
}
var rock=GameObject.Find("QuarryEnvironment").transform.Find("Cut Rock").GetComponent<MeshCollider>();bool solid=rock&&rock.enabled&&!rock.isTrigger&&rock.sharedMesh;if(!solid)failures++;log.Add((solid?"PASS ":"FAIL ")+"Decorative combined rock mesh has static solid collision");
log.Add("COMPLETE checks="+(log.Count)+" failures="+failures);File.WriteAllLines("Maintenance/Sprint1Release/outer-boundary-checks.txt",log);if(failures>0)throw new Exception(log[log.Count-1]);return log[log.Count-1];
}finally{place(original);p.visual.rotation=rotation;p.enabled=enabled;p.touchControls.Move=Vector2.zero;}
}
}
