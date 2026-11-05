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
using TMPro;
public static class AuthorCartRelease {
 const string Folder="Assets/0_ProjectData/Player/Production/CartRelease/";
 static T Save<T>(T asset,string filename)where T:UnityEngine.Object{string path=Folder+filename;var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old){EditorUtility.CopySerialized(asset,old);UnityEngine.Object.DestroyImmediate(asset);EditorUtility.SetDirty(old);return old;}AssetDatabase.CreateAsset(asset,path);return asset;}
 static AnimationClip Source(string path)=>AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().First(c=>!c.name.StartsWith("__"));
 static AnimationClip Bake(MinerController p,QuarryWheelbarrow cart,AnimationClip source,string name,float lean){
  var clone=UnityEngine.Object.Instantiate(p.visual.gameObject);clone.name="Temporary animation baker";clone.transform.SetPositionAndRotation(p.visual.localPosition,Quaternion.identity);
  foreach(var behaviour in clone.GetComponentsInChildren<MonoBehaviour>(true))behaviour.enabled=false;
  var a=clone.GetComponent<Animator>();a.applyRootMotion=false;a.Rebind();a.Update(0);
  var clip=UnityEngine.Object.Instantiate(source);clip.name=name;
  var names=HumanTrait.MuscleName;var selected=Enumerable.Range(0,names.Length).Where(i=>names[i].Contains("Arm")||names[i].Contains("Forearm")||names[i].Contains("Hand ")||names[i].Contains("Shoulder")||names[i]=="Spine Front-Back").ToArray();
  var curves=selected.ToDictionary(i=>i,i=>new AnimationCurve());
  var targets=new Transform[2];var hands=new Transform[2];var solvers=new FastIKFabric[2];
  try{source.SampleAnimation(clone,0);
   for(int side=0;side<2;side++){
    hands[side]=a.GetBoneTransform(side==0?HumanBodyBones.LeftHand:HumanBodyBones.RightHand);
    var grip=side==0?cart.leftGrip:cart.rightGrip;
    targets[side]=new GameObject("Bake wrist").transform;targets[side].SetParent(clone.transform,false);
    targets[side].position=Vector3.forward*.95f+Vector3.up*.05f+cart.transform.InverseTransformPoint(grip.position);
    targets[side].rotation=Quaternion.Inverse(cart.transform.rotation)*grip.rotation;
    solvers[side]=hands[side].GetComponent<FastIKFabric>();if(!solvers[side])solvers[side]=hands[side].gameObject.AddComponent<FastIKFabric>();
    solvers[side].enabled=false;solvers[side].Target=targets[side];solvers[side].Iterations=16;
    var pole=new GameObject("Bake elbow").transform;pole.SetParent(clone.transform,false);pole.localPosition=new Vector3(side==0?-.65f:.65f,1.05f,-.12f);solvers[side].Pole=pole;
   }
   using(var handler=new HumanPoseHandler(a.avatar,clone.transform)){
    int samples=Mathf.Max(12,Mathf.CeilToInt(source.length*30));
    for(int frame=0;frame<=samples;frame++){
     float time=source.length*frame/samples;source.SampleAnimation(clone,time);
     var pose=new HumanPose();handler.GetHumanPose(ref pose);pose.muscles[Array.IndexOf(names,"Spine Front-Back")]+=lean;handler.SetHumanPose(ref pose);
     for(int side=0;side<2;side++){solvers[side].Solve();hands[side].rotation=targets[side].rotation;}
     handler.GetHumanPose(ref pose);foreach(int i in selected)curves[i].AddKey(time,pose.muscles[i]);
    }
   }
   foreach(int i in selected)AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(Animator),names[i]),curves[i]);
   var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;settings.loopBlend=true;AnimationUtility.SetAnimationClipSettings(clip,settings);
   clip.EnsureQuaternionContinuity();return Save(clip,name+".anim");
  }finally{UnityEngine.Object.DestroyImmediate(clone);}
 }
 public static object Main(){
  if(Application.isPlaying)throw new Exception("Edit mode required");Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
  var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();var cart=UnityEngine.Object.FindFirstObjectByType<QuarryWheelbarrow>();var a=p.visual.GetComponent<Animator>();
  var root=a.runtimeAnimatorController as AnimatorController;if(!root)throw new Exception("Animator controller required");
  var idle=Bake(p,cart,Source("Assets/Kevin Iglesias/Human Animations/Animations/Male/Idles/HumanM@Idle01.fbx"),"CartIdle",0);
  var push=Bake(p,cart,Source("Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Walk/HumanM@Walk01_Forward.fbx"),"CartPush",.08f);
  var pull=Bake(p,cart,Source("Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Walk/HumanM@Walk01_Backward.fbx"),"CartPull",-.06f);
  if(!root.parameters.Any(x=>x.name=="Cart"))root.AddParameter("Cart",AnimatorControllerParameterType.Bool);
  if(!root.parameters.Any(x=>x.name=="CartSpeed"))root.AddParameter("CartSpeed",AnimatorControllerParameterType.Float);
  var machine=root.layers[0].stateMachine;var state=machine.states.Select(x=>x.state).FirstOrDefault(x=>x.name=="Cart");
  if(!state){state=machine.AddState("Cart");var enter=machine.AddAnyStateTransition(state);enter.hasExitTime=false;enter.duration=.18f;enter.canTransitionToSelf=false;enter.AddCondition(AnimatorConditionMode.If,0,"Cart");var leave=state.AddTransition(machine.states.First(x=>x.state.name=="Locomotion").state);leave.hasExitTime=false;leave.duration=.18f;leave.AddCondition(AnimatorConditionMode.IfNot,0,"Cart");}
  var tree=new BlendTree{name="Cart signed motion",blendType=BlendTreeType.Simple1D,blendParameter="CartSpeed",useAutomaticThresholds=false};tree.AddChild(pull,-1);tree.AddChild(idle,0);tree.AddChild(push,1);tree=Save(tree,"CartMotion.asset");state.motion=tree;
  p.visual.GetComponent<MinerAnimator>().cartAnimations=true;a.applyRootMotion=false;EditorUtility.SetDirty(root);
  var rocks=UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>!r.requiresCopperRidge&&!string.IsNullOrEmpty(r.blockId)).OrderBy(r=>r.transform.position.z).ThenBy(r=>r.transform.position.x).ToArray();
  for(int i=0;i<rocks.Length;i++)rocks[i].firstRampShift=i<24?2:i<48?3:4;
  var expansion=UnityEngine.Object.FindFirstObjectByType<QuarryExpansion>();var sign=expansion.sign;sign.transform.position=new Vector3(22.8f,2.0f,1.25f);sign.fontSize=1.1f;sign.rectTransform.sizeDelta=new Vector2(2.5f,.75f);sign.text="COPPER RIDGE\nOPENS AFTER SHIFT 3";
  var existing=GameObject.Find("Copper return lane");if(existing)UnityEngine.Object.DestroyImmediate(existing);
  var lane=new GameObject("Copper return lane");var verts=new List<Vector3>();var tris=new List<int>();
  Action<Vector3,Vector3,Vector3,Vector3> quad=(v0,v1,v2,v3)=>{int n=verts.Count;verts.AddRange(new[]{v0,v1,v2,v3});tris.AddRange(new[]{n,n+2,n+1,n,n+3,n+2});};
  Vector3[][] routes={new[]{new Vector3(20.45f,.045f,9.5f),new Vector3(20.45f,.045f,-3.8f),new Vector3(12,.045f,-3.8f)},new[]{new Vector3(16.7f,.045f,9.5f),new Vector3(16.7f,.045f,-3.8f)}};
  foreach(var route in routes)for(int i=1;i<route.Length;i++){
   var start=route[i-1];var d=(route[i]-start).normalized;var side=Vector3.Cross(Vector3.up,d);float length=Vector3.Distance(start,route[i]);
   // Dashed edges and directional chevrons, combined into a single static renderer.
   for(float t=.2f;t<length-.3f;t+=1.2f){var mid=start+d*t;foreach(float edge in new[]{-.40f,.40f}){var q=mid+side*edge;quad(q-side*.025f,q+side*.025f,q+side*.025f+d*.55f,q-side*.025f+d*.55f);}}
   for(float t=.8f;t<length-.4f;t+=2.4f){var q=start+d*t;quad(q-side*.18f-d*.12f,q-side*.18f-d*.04f,q+d*.16f,q+d*.08f);quad(q+d*.08f,q+d*.16f,q+side*.18f-d*.04f,q+side*.18f-d*.12f);}
  }
  var mesh=new Mesh{name="Copper return markings"};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh=Save(mesh,"ReturnLane.asset");
  var material=new Material(Shader.Find("Universal Render Pipeline/Unlit")){name="Return lane paint",color=new Color(.9f,.73f,.36f)};material=Save(material,"ReturnLane.mat");
  lane.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=lane.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;lane.isStatic=true;
  foreach(var pos in new[]{new Vector3(20.45f,.055f,2.3f),new Vector3(16.7f,.055f,-2.8f)}){var label=UnityEngine.Object.Instantiate(sign,lane.transform);label.name="Return lane label";label.text="EXIT\nREFINERY";label.fontSize=.8f;label.rectTransform.sizeDelta=new Vector2(1.5f,.8f);label.transform.SetPositionAndRotation(pos,Quaternion.Euler(90,0,0));}
  AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);
  return new {rocks=rocks.Length,second=rocks.Count(r=>r.firstRampShift<=2),third=rocks.Count(r=>r.firstRampShift<=3),laneTriangles=mesh.GetIndexCount(0)/3,clips=new[]{idle.name,push.name,pull.name}};
 }
}
