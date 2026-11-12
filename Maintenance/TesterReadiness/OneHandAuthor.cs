using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using Bulldozer.Player;
public static class OneHandAuthor {
 const string Folder="Assets/0_ProjectData/Player/Production/OneHandMining/";
 static float Ease(float x)=>Mathf.SmoothStep(0,1,Mathf.Clamp01(x));
 static Vector3 Palm(float t,bool ground=false){var hold=new Vector3(.23f,1.08f,.32f);var raised=new Vector3(.23f,1.40f,.32f);var hit=new Vector3(.23f,ground?.75f:1.0f,.65f);if(t<.22f)return Vector3.Lerp(hold,raised,Ease(t/.22f));if(t<.46f)return Vector3.Lerp(raised,hit,Ease((t-.22f)/.24f));if(t<.58f)return hit;return Vector3.Lerp(hit,hold,Ease((t-.58f)/.42f));}
 static float BodyTime(float t){if(t<.22f)return 0;if(t<.46f)return .42f*Ease((t-.22f)/.24f);if(t<.58f)return .42f;return .42f*(1-Ease((t-.58f)/.42f));}
 static void Arm(Transform upper,Transform lower,Transform hand,Vector3 target){float a=Vector3.Distance(upper.position,lower.position),b=Vector3.Distance(lower.position,hand.position);var delta=target-upper.position;float distance=Mathf.Clamp(delta.magnitude,.02f,a+b-.001f);var direction=delta.normalized;var bend=Vector3.ProjectOnPlane(new Vector3(.75f,1.05f,-.1f)-upper.position,direction).normalized;float along=(a*a-b*b+distance*distance)/(2*distance);var elbow=upper.position+direction*along+bend*Mathf.Sqrt(Mathf.Max(0,a*a-along*along));upper.rotation=Quaternion.FromToRotation(lower.position-upper.position,elbow-upper.position)*upper.rotation;lower.rotation=Quaternion.FromToRotation(hand.position-lower.position,target-lower.position)*lower.rotation;}
 static AnimationClip Save(AnimationClip clip,string name){string path=Folder+name+".anim";var old=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);if(old){EditorUtility.CopySerialized(clip,old);UnityEngine.Object.DestroyImmediate(clip);EditorUtility.SetDirty(old);return old;}AssetDatabase.CreateAsset(clip,path);return clip;}
 public static string Main(){
  if(Application.isPlaying)throw new Exception("Edit mode required");if(!AssetDatabase.IsValidFolder(Folder.TrimEnd('/')))AssetDatabase.CreateFolder("Assets/0_ProjectData/Player/Production","OneHandMining");
  var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();var driver=p.visual.GetComponent<MinerAnimator>();var controller=(AnimatorController)p.visual.GetComponent<Animator>().runtimeAnimatorController;
  var clone=UnityEngine.Object.Instantiate(p.visual.gameObject);clone.hideFlags=HideFlags.HideAndDontSave;clone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);foreach(var s in clone.GetComponentsInChildren<MonoBehaviour>(true))s.enabled=false;var a=clone.GetComponent<Animator>();a.Rebind();a.Update(0);var d=clone.GetComponent<MinerAnimator>();var mining=clone.GetComponent<MinerMining>();var hand=a.GetBoneTransform(HumanBodyBones.RightHand);var upper=a.GetBoneTransform(HumanBodyBones.RightUpperArm);var lower=a.GetBoneTransform(HumanBodyBones.RightLowerArm);
  Vector3 localUp=mining.headProbes[0].localPosition.normalized,localSide=(mining.headProbes[1].localPosition-mining.headProbes[2].localPosition).normalized;
  Vector3 up=new Vector3(0,.65f,.76f).normalized,side=Vector3.Cross(Vector3.right,up);Quaternion front=Quaternion.LookRotation(Vector3.Cross(side,up),up)*Quaternion.Inverse(Quaternion.LookRotation(Vector3.Cross(localSide,localUp),localUp));
  var idle=AssetDatabase.LoadAllAssetsAtPath("Assets/Kevin Iglesias/Human Animations/Animations/Male/Idles/HumanM@Idle01.fbx").OfType<AnimationClip>().First(c=>!c.name.StartsWith("__"));var source=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/0_ProjectData/Player/MiningGround.anim");var names=HumanTrait.MuscleName;
  Func<string,bool> arm=n=>n.Contains("Arm")||n.Contains("Forearm")||n.Contains("Hand ")||n.Contains("Shoulder");var right=Enumerable.Range(0,names.Length).Where(i=>names[i].StartsWith("Right ")&&arm(names[i])).ToArray();var left=Enumerable.Range(0,names.Length).Where(i=>names[i].StartsWith("Left ")&&(arm(names[i])||names[i].Contains("Stretched"))).ToArray();
  try{using(var handler=new HumanPoseHandler(a.avatar,clone.transform)){
   idle.SampleAnimation(clone,0);var neutral=new HumanPose();handler.GetHumanPose(ref neutral);
   var clips=new List<AnimationClip>();
   for(int variant=0;variant<5;variant++){
    bool hit=variant<2,ground=variant==0;float duration=hit?source.length:1f;var clip=hit?UnityEngine.Object.Instantiate(source):new AnimationClip{name="Front hold "+variant,frameRate=60};
    if(hit){clip.name=ground?"One hand low rock strike":"One hand upright rock strike";foreach(var binding in AnimationUtility.GetCurveBindings(source)){var original=AnimationUtility.GetEditorCurve(source,binding);var neutralCurve=AnimationUtility.GetEditorCurve(idle,binding);float rest=neutralCurve!=null?neutralCurve.Evaluate(0):original.Evaluate(0);var curve=new AnimationCurve();for(int f=0;f<=120;f++){float t=f/120f;float lean=ground?.35f*BodyTime(t)/.42f:0;curve.AddKey(t*duration,Mathf.Lerp(rest,original.Evaluate(BodyTime(t)*duration),lean));}AnimationUtility.SetEditorCurve(clip,binding,curve);}foreach(int i in left)AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(Animator),names[i]),AnimationCurve.Constant(0,duration,neutral.muscles[i]));}
    var curves=right.ToDictionary(i=>i,i=>new AnimationCurve());
    for(int f=0;f<=120;f++){
     float t=f/120f;if(hit)clip.SampleAnimation(clone,t*duration);else idle.SampleAnimation(clone,0);
     Vector3 target=hit?Palm(t,ground):Palm(0)+Vector3.up*(Mathf.Sin(t*Mathf.PI*2)*(variant==2?0:variant==3?.008f:.015f));Quaternion wrist=front*Quaternion.Inverse(d.toolGrip.localRotation);
     Arm(upper,lower,hand,target-wrist*d.toolGrip.localPosition);hand.rotation=wrist;
     var pose=new HumanPose();handler.GetHumanPose(ref pose);foreach(int i in right)curves[i].AddKey(t*duration,pose.muscles[i]);
    }
    foreach(int i in right)AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(Animator),names[i]),curves[i]);
    var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=!hit;settings.loopBlend=!hit;AnimationUtility.SetAnimationClipSettings(clip,settings);
    if(hit)AnimationUtility.SetAnimationEvents(clip,new[]{new AnimationEvent{functionName="BeginMiningContact",time=duration*.32f},new AnimationEvent{functionName="EndMiningContact",time=duration*.53f}});
    clips.Add(Save(clip,hit?(ground?"OneHandGround":"OneHandWall"):variant==2?"FrontHoldIdle":variant==3?"FrontHoldWalk":"FrontHoldRun"));
   }
   foreach(var state in controller.layers[0].stateMachine.states.Where(x=>x.state.name=="Mine Ground"||x.state.name=="Mine Wall"))state.state.motion=clips[state.state.name=="Mine Ground"?0:1];
   foreach(var rock in UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.naturalBoulder&&!string.IsNullOrEmpty(r.blockId))){rock.wallMining=false;PrefabUtility.RecordPrefabInstancePropertyModifications(rock);EditorUtility.SetDirty(rock);}
   var carry=controller.layers.First(l=>l.name=="Pickaxe Carry").stateMachine.states.First().state.motion as BlendTree;var children=carry.children;for(int i=0;i<children.Length;i++)children[i].motion=clips[Mathf.Min(i+2,4)];carry.children=children;EditorUtility.SetDirty(carry);
   driver.fixedPickaxeAngle=true;driver.pickaxeFrontRotation=front;PrefabUtility.RecordPrefabInstancePropertyModifications(driver);EditorUtility.SetDirty(driver);EditorUtility.SetDirty(controller);UnityEngine.Object.DestroyImmediate(clone);EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();return "One-hand strike and three front-hold clips saved; left arm relaxed; tool angle fixed relative to player";
  }}finally{if(clone)UnityEngine.Object.DestroyImmediate(clone);}
 }
}

