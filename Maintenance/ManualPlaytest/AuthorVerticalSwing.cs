using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using Bulldozer.Player;
public static class AuthorVerticalSwing {
 const string Folder="Assets/0_ProjectData/Player/Production/VerticalMining/";
 static float SourceTime(float normalized){
  // Lift back along the strike path. The source clip's later recovery contains
  // a wrist roll, so it must never be sampled by this two-handed mining loop.
  if(normalized<.10f)return .18f;
  if(normalized<.40f)return Mathf.Lerp(.18f,.42f,Mathf.SmoothStep(0,1,(normalized-.10f)/.30f));
  if(normalized<.55f)return .42f;
  if(normalized<.85f)return Mathf.Lerp(.42f,.18f,Mathf.SmoothStep(0,1,(normalized-.55f)/.30f));
  return .18f;
 }
 static void Arm(Transform shoulder,Transform elbow,Transform hand,Vector3 target,Vector3 pole){
  float upper=Vector3.Distance(shoulder.position,elbow.position),lower=Vector3.Distance(elbow.position,hand.position);
  Vector3 delta=target-shoulder.position;float distance=Mathf.Clamp(delta.magnitude,.02f,upper+lower-.001f);Vector3 direction=delta.normalized;
  Vector3 bend=Vector3.ProjectOnPlane(pole-shoulder.position,direction).normalized;
  float along=(upper*upper-lower*lower+distance*distance)/(2*distance),height=Mathf.Sqrt(Mathf.Max(0,upper*upper-along*along));
  Vector3 elbowTarget=shoulder.position+direction*along+bend*height;
  shoulder.rotation=Quaternion.FromToRotation(elbow.position-shoulder.position,elbowTarget-shoulder.position)*shoulder.rotation;
  elbow.rotation=Quaternion.FromToRotation(hand.position-elbow.position,target-elbow.position)*elbow.rotation;
 }
 public static object Main(){
  if(Application.isPlaying)throw new Exception("Edit mode required");
  if(!AssetDatabase.IsValidFolder(Folder.TrimEnd('/')))AssetDatabase.CreateFolder("Assets/0_ProjectData/Player/Production","VerticalMining");
  var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();var ac=(AnimatorController)p.visual.GetComponent<Animator>().runtimeAnimatorController;
  var report=new List<string>();
  foreach(string variant in new[]{"Ground","Wall"}){
   var source=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/0_ProjectData/Player/Mining"+variant+".anim");
   var clone=UnityEngine.Object.Instantiate(p.visual.gameObject);clone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);foreach(var script in clone.GetComponentsInChildren<MonoBehaviour>(true))script.enabled=false;
   var a=clone.GetComponent<Animator>();a.Rebind();a.Update(0);var d=clone.GetComponent<MinerAnimator>();var mine=clone.GetComponent<MinerMining>();
   var right=a.GetBoneTransform(HumanBodyBones.RightHand);var left=a.GetBoneTransform(HumanBodyBones.LeftHand);var elbow=a.GetBoneTransform(HumanBodyBones.LeftLowerArm);var shoulder=a.GetBoneTransform(HumanBodyBones.LeftUpperArm);
   Vector3 localUp=mine.headProbes[0].localPosition.normalized,localSide=(mine.headProbes[1].localPosition-mine.headProbes[2].localPosition).normalized;
   Quaternion localBasis=Quaternion.LookRotation(Vector3.Cross(localSide,localUp),localUp);
   var names=HumanTrait.MuscleName;var selected=Enumerable.Range(0,names.Length).Where(i=>(names[i].StartsWith("Right ")&&(names[i].Contains("Arm")||names[i].Contains("Forearm")||names[i].Contains("Hand ")))||names[i].StartsWith("Left ")).ToArray();
   var curves=selected.ToDictionary(i=>i,i=>new AnimationCurve());var clip=UnityEngine.Object.Instantiate(source);clip.name="Vertical two hand mining "+variant;clip.frameRate=60;
   float maxError=0;
   try{using(var handler=new HumanPoseHandler(a.avatar,clone.transform)){
    int frames=Mathf.CeilToInt(source.length*60);
    for(int f=0;f<=frames;f++){
     float t=f*source.length/frames;source.SampleAnimation(clone,t);
     // Keep the original overhead arm/body arc, but put the steel head in the
     // forward vertical plane instead of sideways across the player's face.
     Vector3 up=d.toolGrip.TransformDirection(localUp);up.x=0;up.Normalize();
     Vector3 side=Vector3.Cross(Vector3.right,up).normalized;
     Quaternion toolRotation=Quaternion.LookRotation(Vector3.Cross(side,up),up)*Quaternion.Inverse(localBasis);
     Vector3 rightTarget=right.position;rightTarget.x=.08f;
     Arm(a.GetBoneTransform(HumanBodyBones.RightUpperArm),a.GetBoneTransform(HumanBodyBones.RightLowerArm),right,rightTarget,new Vector3(.65f,1.05f,.05f));
     right.rotation=toolRotation*Quaternion.Inverse(d.toolGrip.localRotation);
     Vector3 target=d.toolGrip.TransformPoint(-localUp*.22f);
     Arm(shoulder,elbow,left,target,new Vector3(-.65f,1.05f,.05f));
     left.rotation=right.rotation*Quaternion.Euler(0,180,0);
     maxError=Mathf.Max(maxError,Vector3.Distance(left.position,target));
     var pose=new HumanPose();handler.GetHumanPose(ref pose);
     foreach(int i in selected){if(names[i].StartsWith("Left ")&&names[i].Contains("Stretched"))pose.muscles[i]=names[i].Contains("Thumb")?-.35f:-.8f;curves[i].AddKey(t,pose.muscles[i]);}
    }
   }
   foreach(int i in selected)AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(Animator),names[i]),curves[i]);
   // Reverse the already-baked pose curves, so recovery uses exactly the same
   // solved grip as the strike, without a second humanoid pose conversion.
   foreach(var binding in AnimationUtility.GetCurveBindings(clip)){
    var originalCurve=AnimationUtility.GetEditorCurve(clip,binding);var retimed=new AnimationCurve();int samples=120;
    for(int f=0;f<=samples;f++){float normalized=f/(float)samples;retimed.AddKey(normalized*source.length,originalCurve.Evaluate(SourceTime(normalized)*source.length));}
    for(int k=0;k<retimed.length;k++){AnimationUtility.SetKeyLeftTangentMode(retimed,k,AnimationUtility.TangentMode.Linear);AnimationUtility.SetKeyRightTangentMode(retimed,k,AnimationUtility.TangentMode.Linear);}
    AnimationUtility.SetEditorCurve(clip,binding,retimed);
   }
   // Humanoid root retiming changes the shoulder frame. Refit the supporting
   // hand against the final clip, keeping the right-hand/tool path untouched.
   var supportIndices=Enumerable.Range(0,names.Length).Where(i=>(names[i].StartsWith("Left ")||names[i].StartsWith("Right "))&&(names[i].Contains("Arm")||names[i].Contains("Forearm")||names[i].Contains("Hand ")||names[i].Contains("Shoulder"))).ToArray();
   var supportCurves=supportIndices.ToDictionary(i=>i,i=>new AnimationCurve());
   using(var handler=new HumanPoseHandler(a.avatar,clone.transform))for(int f=0;f<=120;f++){
    float t=source.length*f/120f;clip.SampleAnimation(clone,t);
    Quaternion wrist=right.rotation;Vector3 centered=new Vector3(0,Mathf.Clamp(right.position.y,.70f,1.60f),right.position.z*.85f);
    Arm(a.GetBoneTransform(HumanBodyBones.RightUpperArm),a.GetBoneTransform(HumanBodyBones.RightLowerArm),right,centered,new Vector3(.65f,1.05f,.05f));right.rotation=wrist;
    Vector3 target=d.toolGrip.TransformPoint(-localUp*.22f);
    Arm(shoulder,elbow,left,target,clone.transform.TransformPoint(new Vector3(-.65f,1.05f,.05f)));left.rotation=right.rotation*Quaternion.Euler(0,180,0);
    var pose=new HumanPose();handler.GetHumanPose(ref pose);foreach(int i in supportIndices)supportCurves[i].AddKey(t,pose.muscles[i]);
   }
   foreach(int i in supportIndices)AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(Animator),names[i]),supportCurves[i]);
   var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=false;AnimationUtility.SetAnimationClipSettings(clip,settings);
   AnimationUtility.SetAnimationEvents(clip,new[]{new AnimationEvent{functionName="BeginMiningContact",time=clip.length*.28f},new AnimationEvent{functionName="EndMiningContact",time=clip.length*.50f}});
   string path=Folder+"Vertical"+variant+".anim";var old=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);if(old){EditorUtility.CopySerialized(clip,old);UnityEngine.Object.DestroyImmediate(clip);clip=old;EditorUtility.SetDirty(clip);}else AssetDatabase.CreateAsset(clip,path);
   ac.layers[0].stateMachine.states.Single(x=>x.state.name=="Mine "+variant).state.motion=clip;
   report.Add(variant+" baked at 60fps; max pre-retarget support error="+maxError.ToString("F4"));
   }finally{UnityEngine.Object.DestroyImmediate(clone);}
  }
  EditorUtility.SetDirty(ac);EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();return string.Join("\n",report);
 }
}
