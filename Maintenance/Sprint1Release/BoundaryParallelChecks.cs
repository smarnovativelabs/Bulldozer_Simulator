using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bulldozer.Player;
public static class BoundaryParallelChecks {
public static string Main(){
if(!Application.isPlaying)throw new Exception("Play mode required");
var e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();if(!e.SavePath.Contains("BoundaryCartTest-"))throw new Exception("Isolated test save required");
new GameObject("Boundary parallel test runner").AddComponent<BoundaryParallelRunner>().Begin();return "Live input/camera tests started";
}}
public class BoundaryParallelRunner:MonoBehaviour {
MinerController p;QuarryWheelbarrow cart;Vector3 original;Quaternion originalFacing;bool autoMine;readonly List<string> log=new List<string>();int failures;readonly Collider[] hits=new Collider[64];
void Note(bool ok,string text){log.Add((ok?"PASS ":"FAIL ")+text);if(!ok)failures++;}
public void Begin(){p=UnityEngine.Object.FindFirstObjectByType<MinerController>();cart=UnityEngine.Object.FindFirstObjectByType<QuarryWheelbarrow>();original=p.transform.position;originalFacing=p.visual.rotation;autoMine=p.autoMine;p.autoMine=false;p.CancelMining();StartCoroutine(Guard(Run()));}
IEnumerator Guard(IEnumerator source){var stack=new Stack<IEnumerator>();stack.Push(source);while(stack.Count>0){object value=null;bool more=false;try{more=stack.Peek().MoveNext();if(more)value=stack.Peek().Current;else stack.Pop();}catch(Exception ex){failures++;log.Add("ERROR "+ex);break;}if(more){if(value is IEnumerator nested)stack.Push(nested);else yield return value;}}
p.touchControls.Move=Vector2.zero;yield return new WaitForSeconds(.5f);if(cart.Pushing)cart.Interact();Place(original,originalFacing*Vector3.forward);p.autoMine=autoMine;log.Add("COMPLETE failures="+failures+" frame="+Time.frameCount);File.WriteAllLines("Maintenance/Sprint1Release/boundary-parallel-checks.txt",log);Destroy(gameObject);}
void Place(Vector3 position,Vector3 facing){p.touchControls.Move=Vector2.zero;p.enabled=false;var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=position;cc.enabled=true;p.visual.rotation=Quaternion.LookRotation(facing);p.enabled=true;Physics.SyncTransforms();}
void Input(Vector3 direction){var local=Quaternion.Euler(0,-p.followCamera.Yaw,0)*direction;p.touchControls.Move=new Vector2(local.x,local.z);}
bool Penetrates(){int n=Physics.OverlapBoxNonAlloc(cart.transform.position+Vector3.up*.55f,new Vector3(.37f,.31f,.54f),hits,cart.transform.rotation,~0,QueryTriggerInteraction.Ignore);for(int i=0;i<n;i++)if(!hits[i].transform.IsChildOf(p.transform)&&!hits[i].transform.IsChildOf(cart.transform))return true;return false;}
IEnumerator Drive(Vector3 direction,float seconds){float end=Time.time+seconds;while(Time.time<end){Input(direction);yield return null;}p.touchControls.Move=Vector2.zero;}
IEnumerator Run(){
var starts=new[]{new Vector3(-13,.05f,-6),new Vector3(21,.05f,-6),new Vector3(12,.05f,-7),new Vector3(-14,.05f,-7)};
var walls=new[]{Vector3.left,Vector3.right,Vector3.back,new Vector3(-1,0,-1).normalized};var tangents=new[]{Vector3.forward,Vector3.forward,Vector3.right,Vector3.right};
for(int i=0;i<4;i++){
if(cart.Pushing){cart.Interact();yield return new WaitForSeconds(.5f);}Place(starts[i],walls[i]);cart.transform.position=p.transform.position-walls[i];Physics.SyncTransforms();yield return new WaitForSeconds(.5f);cart.Interact();Note(cart.Pushing,"Case "+i+" attached");if(!cart.Pushing)continue;
yield return Drive(walls[i],4);var contact=p.transform.position;float begin=Time.time,turn=-1;int overlaps=0,reversals=0,lastSign=0;float previousDistance=Vector3.Distance(p.followCamera.transform.position,p.transform.position+Vector3.up*1.35f);while(Time.time<begin+2.5f){Input(tangents[i]);yield return new WaitForEndOfFrame();if(turn<0&&Vector3.Angle(p.visual.forward,tangents[i])<15)turn=Time.time-begin;if(Penetrates())overlaps++;float distance=Vector3.Distance(p.followCamera.transform.position,p.transform.position+Vector3.up*1.35f);float delta=distance-previousDistance;if(Mathf.Abs(delta)>.015f){int sign=delta>0?1:-1;if(lastSign!=0&&sign!=lastSign)reversals++;lastSign=sign;}previousDistance=distance;}p.touchControls.Move=Vector2.zero;
float travel=Vector3.Dot(p.transform.position-contact,tangents[i]);Note(turn>=0&&turn<1.8f&&travel>.6f,"Case "+i+" parallel turn: seconds="+turn.ToString("F3")+" travel="+travel.ToString("F3")+" angle="+Vector3.Angle(p.visual.forward,tangents[i]).ToString("F1"));Note(overlaps==0,"Case "+i+" overlap frames="+overlaps);Note(reversals<=2,"Case "+i+" parallel camera zoom reversals="+reversals);yield return new WaitForSeconds(.5f);
}
}
}