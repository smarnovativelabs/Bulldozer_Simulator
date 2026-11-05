using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using Bulldozer.Player;
using DitzelGames.FastIK;
public static class BoundaryAnimationAuthor {
 const string Folder="Assets/0_ProjectData/Player/Production/BoundaryAnimation/";
 public static string Navigation(){
  foreach(string name in new[]{"Copper exterior gap blocker","Copper exterior north cap"}){var go=GameObject.Find(name);if(!go)throw new Exception("Missing blocker "+name);var obstacle=go.GetComponent<UnityEngine.AI.NavMeshObstacle>();if(!obstacle)obstacle=go.AddComponent<UnityEngine.AI.NavMeshObstacle>();obstacle.shape=UnityEngine.AI.NavMeshObstacleShape.Box;obstacle.center=Vector3.zero;obstacle.size=Vector3.one;obstacle.carving=true;obstacle.carveOnlyStationary=true;}
  if(!Application.isPlaying){var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}
  return "Both gap blockers also carve navigation";
 }
 static T Save<T>(T asset,string file)where T:UnityEngine.Object{string path=Folder+file;var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old){EditorUtility.CopySerialized(asset,old);UnityEngine.Object.DestroyImmediate(asset);EditorUtility.SetDirty(old);return old;}AssetDatabase.CreateAsset(asset,path);return asset;}
 static AnimationClip Bake(MinerController p,string name,float motion){
  var driver=p.visual.GetComponent<MinerAnimator>();var clone=UnityEngine.Object.Instantiate(p.visual.gameObject);clone.name="Temporary carry baker";clone.transform.SetPositionAndRotation(p.visual.localPosition,Quaternion.identity);
  foreach(var script in clone.GetComponentsInChildren<MonoBehaviour>(true))script.enabled=false;
  var a=clone.GetComponent<Animator>();a.applyRootMotion=false;a.Rebind();a.Update(0);
  var idle=AssetDatabase.LoadAllAssetsAtPath("Assets/Kevin Iglesias/Human Animations/Animations/Male/Idles/HumanM@Idle01.fbx").OfType<AnimationClip>().First(c=>!c.name.StartsWith("__"));
  var names=HumanTrait.MuscleName;var selected=Enumerable.Range(0,names.Length).Where(i=>names[i].StartsWith("Right ")&&(names[i].Contains("Arm")||names[i].Contains("Forearm")||names[i].Contains("Hand ")||names[i].Contains("Shoulder"))).ToArray();
  var curves=selected.ToDictionary(i=>i,i=>new AnimationCurve());var clip=new AnimationClip{name=name,frameRate=30};
  try{idle.SampleAnimation(clone,0);var hand=a.GetBoneTransform(HumanBodyBones.RightHand);var shoulder=a.GetBoneTransform(HumanBodyBones.RightUpperArm);var elbow=a.GetBoneTransform(HumanBodyBones.RightLowerArm);float length=Vector3.Distance(shoulder.position,elbow.position)+Vector3.Distance(elbow.position,hand.position);
   var target=new GameObject("Bake carry target").transform;target.SetParent(clone.transform,false);target.localPosition=driver.carryPosition;target.localRotation=driver.carryRotation*driver.carryToolRotation;
   var pole=new GameObject("Bake elbow").transform;pole.SetParent(clone.transform,false);pole.localPosition=new Vector3(.65f,1.05f,-.18f);
   var ik=hand.GetComponent<FastIKFabric>();if(!ik)ik=hand.gameObject.AddComponent<FastIKFabric>();ik.enabled=false;ik.Target=target;ik.Pole=pole;ik.Iterations=16;
   using(var handler=new HumanPoseHandler(a.avatar,clone.transform))for(int frame=0;frame<=30;frame++){
    float t=frame/30f;idle.SampleAnimation(clone,0);Vector3 carry=clone.transform.TransformPoint(driver.carryPosition+new Vector3(0,Mathf.Sin(t*Mathf.PI*2)*motion,Mathf.Cos(t*Mathf.PI*2)*motion*.4f));target.position=shoulder.position+Vector3.ClampMagnitude(carry-shoulder.position,length*.92f);ik.Solve();hand.rotation=target.rotation;
    var pose=new HumanPose();handler.GetHumanPose(ref pose);foreach(int i in selected)curves[i].AddKey(t,pose.muscles[i]);
   }
   foreach(int i in selected)AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(Animator),names[i]),curves[i]);
   var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;settings.loopBlend=true;AnimationUtility.SetAnimationClipSettings(clip,settings);return Save(clip,name+".anim");
  }finally{UnityEngine.Object.DestroyImmediate(clone);}
 }
 public static object Main(){
  if(Application.isPlaying)throw new Exception("Edit mode required");Directory.CreateDirectory(Folder);AssetDatabase.Refresh();var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();var driver=p.visual.GetComponent<MinerAnimator>();var a=driver.GetComponent<Animator>();var controller=(AnimatorController)a.runtimeAnimatorController;
  var idle=Bake(p,"PickaxeCarryIdle",.004f);var walk=Bake(p,"PickaxeCarryWalk",.014f);var run=Bake(p,"PickaxeCarryRun",.022f);
  var mask=new AvatarMask{name="Right arm carry only"};for(int i=0;i<(int)AvatarMaskBodyPart.LastBodyPart;i++)mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i,i==(int)AvatarMaskBodyPart.RightArm);mask=Save(mask,"PickaxeCarry.mask");
  var tree=new BlendTree{name="Pickaxe carry speed",blendParameter="Speed",useAutomaticThresholds=false};tree.AddChild(idle,0);tree.AddChild(walk,2);tree.AddChild(run,5.5f);tree=Save(tree,"PickaxeCarry.asset");
  int layerIndex=Array.FindIndex(controller.layers,l=>l.name=="Pickaxe Carry");if(layerIndex<0){controller.AddLayer("Pickaxe Carry");layerIndex=controller.layers.Length-1;}
  var layers=controller.layers;var layer=layers[layerIndex];layer.defaultWeight=1;layer.avatarMask=mask;layer.blendingMode=AnimatorLayerBlendingMode.Override;var state=layer.stateMachine.states.Select(x=>x.state).FirstOrDefault();if(!state)state=layer.stateMachine.AddState("Carry");state.motion=tree;layer.stateMachine.defaultState=state;layers[layerIndex]=layer;controller.layers=layers;EditorUtility.SetDirty(controller);
  // Source IK is used only by this offline baker; no solver remains on the live miner.
  foreach(var ik in p.GetComponentsInChildren<FastIKFabric>(true))UnityEngine.Object.DestroyImmediate(ik);driver.rightHandIK=null;
  if(driver.gripTarget)UnityEngine.Object.DestroyImmediate(driver.gripTarget.gameObject);if(driver.elbowPole)UnityEngine.Object.DestroyImmediate(driver.elbowPole.gameObject);driver.gripTarget=null;driver.elbowPole=null;
  var expansion=UnityEngine.Object.FindFirstObjectByType<QuarryExpansion>();var material=expansion.gate.GetComponent<Renderer>().sharedMaterial;
  foreach(var spec in new[]{new Vector3(24.7f,2,1.5f),new Vector3(24.7f,2,10.5f)}){
   string name=spec.z<2?"Copper exterior gap blocker":"Copper exterior north cap";var go=GameObject.Find(name);if(!go){go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(expansion.transform,false);}go.transform.position=spec;go.transform.localScale=new Vector3(2.8f,4,.45f);go.GetComponent<Renderer>().sharedMaterial=material;go.isStatic=true;
  }
  int labels=0;foreach(var t in UnityEngine.Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsSortMode.None).Where(t=>t.name=="Return lane label")){
   var hits=Physics.RaycastAll(t.transform.position+Vector3.up*5,Vector3.down,10,~0,QueryTriggerInteraction.Ignore);var ground=hits.Where(h=>h.collider.name=="Quarry Ground"||h.collider.name=="Ground").OrderByDescending(h=>h.point.y).First();t.transform.SetPositionAndRotation(ground.point+ground.normal*.006f,Quaternion.FromToRotation(Vector3.up,ground.normal)*Quaternion.Euler(90,0,0));labels++;
  }
  Navigation();EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();
  return new{labels,clips=new[]{idle.name,walk.name,run.name},runtimeSolvers=p.GetComponentsInChildren<FastIKFabric>(true).Length,colliders=2};
 }
}
