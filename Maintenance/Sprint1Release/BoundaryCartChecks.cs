using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bulldozer.Player;
public static class BoundaryCartChecks {
public static string Main(){
if(!Application.isPlaying)throw new Exception("Play mode required");
var e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();if(!e.SavePath.Contains("BoundaryCartTest-"))throw new Exception("Isolated test save required");
new GameObject("Boundary cart test runner").AddComponent<BoundaryCartRunner>().Begin();return "Live input/camera tests started";
}}
public class BoundaryCartRunner:MonoBehaviour {
MinerController p;QuarryWheelbarrow cart;Vector3 original;Quaternion originalFacing;bool autoMine;readonly List<string> log=new List<string>();int failures;readonly Collider[] hits=new Collider[64];
void Note(bool ok,string text){log.Add((ok?"PASS ":"FAIL ")+text);if(!ok)failures++;}
public void Begin(){p=UnityEngine.Object.FindFirstObjectByType<MinerController>();cart=UnityEngine.Object.FindFirstObjectByType<QuarryWheelbarrow>();original=p.transform.position;originalFacing=p.visual.rotation;autoMine=p.autoMine;p.autoMine=false;p.CancelMining();StartCoroutine(Guard(Run()));}
IEnumerator Guard(IEnumerator source){var stack=new Stack<IEnumerator>();stack.Push(source);while(stack.Count>0){object value=null;bool more=false;try{more=stack.Peek().MoveNext();if(more)value=stack.Peek().Current;else stack.Pop();}catch(Exception ex){failures++;log.Add("ERROR "+ex);break;}if(more){if(value is IEnumerator nested)stack.Push(nested);else yield return value;}}
p.touchControls.Move=Vector2.zero;yield return new WaitForSeconds(.5f);if(cart.Pushing)cart.Interact();Place(original,originalFacing*Vector3.forward);p.autoMine=autoMine;log.Add("COMPLETE failures="+failures+" frame="+Time.frameCount);File.WriteAllLines("Maintenance/Sprint1Release/boundary-cart-checks.txt",log);Destroy(gameObject);}
void Place(Vector3 position,Vector3 facing){p.touchControls.Move=Vector2.zero;p.enabled=false;var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=position;cc.enabled=true;p.visual.rotation=Quaternion.LookRotation(facing);p.enabled=true;Physics.SyncTransforms();}
void Input(Vector3 direction){var local=Quaternion.Euler(0,-p.followCamera.Yaw,0)*direction;p.touchControls.Move=new Vector2(local.x,local.z);}
bool Penetrates(){int n=Physics.OverlapBoxNonAlloc(cart.transform.position+Vector3.up*.55f,new Vector3(.37f,.31f,.54f),hits,cart.transform.rotation,~0,QueryTriggerInteraction.Ignore);for(int i=0;i<n;i++)if(!hits[i].transform.IsChildOf(p.transform)&&!hits[i].transform.IsChildOf(cart.transform))return true;return false;}
IEnumerator Drive(Vector3 direction,float seconds){float end=Time.time+seconds;while(Time.time<end){Input(direction);yield return null;}p.touchControls.Move=Vector2.zero;}
IEnumerator Run(){
var starts=new[]{new Vector3(-13,.05f,-6),new Vector3(21,.05f,-6),new Vector3(12,.05f,-7),new Vector3(-14,.05f,-7)};
var dirs=new[]{Vector3.left,Vector3.right,Vector3.back,new Vector3(-1,0,-1).normalized};var names=new[]{"West entrance","East entrance","South rail","South-west corner"};
for(int test=0;test<starts.Length;test++){
if(cart.Pushing){cart.Interact();yield return new WaitForSeconds(.5f);}Place(starts[test],dirs[test]);cart.transform.position=p.transform.position-dirs[test];Physics.SyncTransforms();yield return new WaitForSeconds(.5f);cart.Interact();Note(cart.Pushing,names[test]+" cart attaches");if(!cart.Pushing)continue;
yield return Drive(dirs[test],3f);
// Maintain pressure into the wall and measure only after camera zoom has settled.
yield return Drive(dirs[test],1f);float min=float.PositiveInfinity,max=0,maxStep=0;int overlap=0,samples=0;Vector3 previous=p.followCamera.transform.position-p.transform.position;float end=Time.time+1f;
while(Time.time<end){Input(dirs[test]);yield return new WaitForEndOfFrame();var offset=p.followCamera.transform.position-p.transform.position;float dist=Vector3.Distance(p.followCamera.transform.position,p.transform.position+Vector3.up*1.35f);min=Mathf.Min(min,dist);max=Mathf.Max(max,dist);maxStep=Mathf.Max(maxStep,(offset-previous).magnitude);previous=offset;if(Penetrates())overlap++;samples++;}
Note(samples>5&&max-min<.035f&&maxStep<.035f,names[test]+" stationary wall camera: samples="+samples+" distanceRange="+(max-min).ToString("F4")+" offsetStep="+maxStep.ToString("F4"));
Note(overlap==0,names[test]+" cart penetration frames="+overlap);
Vector3 contact=p.transform.position;float start=Time.time,response=-1,turnTime=-1;end=Time.time+3f;overlap=0;int zoomReversals=0,lastSign=0;float lastDistance=Vector3.Distance(p.followCamera.transform.position,p.transform.position+Vector3.up*1.35f);
while(Time.time<end){Input(-dirs[test]);yield return new WaitForEndOfFrame();if(response<0&&Vector3.Dot(p.transform.position-contact,-dirs[test])>.2f)response=Time.time-start;if(turnTime<0&&Vector3.Angle(p.visual.forward,-dirs[test])<15)turnTime=Time.time-start;if(Penetrates())overlap++;float currentDistance=Vector3.Distance(p.followCamera.transform.position,p.transform.position+Vector3.up*1.35f);float delta=currentDistance-lastDistance;if(Mathf.Abs(delta)>.015f){int sign=delta>0?1:-1;if(lastSign!=0&&sign!=lastSign)zoomReversals++;lastSign=sign;}lastDistance=currentDistance;}
p.touchControls.Move=Vector2.zero;float escaped=Vector3.Dot(p.transform.position-contact,-dirs[test]);Note(response>=0&&response<.9f&&turnTime>=0&&turnTime<1.8f&&escaped>2,names[test]+" turn back: response="+response.ToString("F3")+" turn="+turnTime.ToString("F3")+" escape="+escaped.ToString("F2"));Note(overlap==0,names[test]+" turning penetration frames="+overlap);Note(zoomReversals<=2,names[test]+" turn camera zoom reversals="+zoomReversals);
yield return new WaitForSeconds(.5f);
}
}
}
