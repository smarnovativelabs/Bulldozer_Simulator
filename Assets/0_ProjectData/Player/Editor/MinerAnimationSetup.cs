using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using DitzelGames.FastIK;
namespace Bulldozer.Player.Editor
{
 public static class MinerAnimationSetup
 {
  const string Root="Assets/0_ProjectData/Player/";
  const string Male="Assets/Kevin Iglesias/Human Animations/Animations/Male/";
  static AnimationClip Clip(string suffix)=>AssetDatabase.LoadAllAssetsAtPath(Male+suffix+".fbx").OfType<AnimationClip>().First(c=>!c.name.StartsWith("__preview__"));
  static AnimatorStateTransition Link(AnimatorState a,AnimatorState b,string parameter,bool value,float duration=.12f)
  {var t=a.AddTransition(b);t.hasExitTime=false;t.duration=duration;t.AddCondition(value?AnimatorConditionMode.If:AnimatorConditionMode.IfNot,0,parameter);return t;}
  [MenuItem("Tools/Bulldozer/Install Mining Animations and Equipment")]
  public static void Apply()
  {
   if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play mode first.");
   MinerUpgrade.Apply();
   var player=Object.FindFirstObjectByType<MinerController>();var visual=player.visual.gameObject;
   PrefabUtility.UnpackPrefabInstance(visual,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
   var animator=visual.GetComponent<Animator>();var driver=visual.GetComponent<MinerAnimator>();
   var equipment=visual.AddComponent<MinerEquipment>();driver.equipment=equipment;
   var tool=new GameObject("Pickaxe (hide or replace independently)");tool.transform.SetParent(visual.transform,false);equipment.pickaxe=tool;
   var rack=new GameObject("Wooden Carrying Rack");rack.transform.SetParent(visual.transform,false);
   foreach(var r in visual.GetComponentsInChildren<SkinnedMeshRenderer>())
   {
    if(r.name.Contains("Pickaxe")||r.name.Contains("PickHead"))r.transform.SetParent(tool.transform,true);
    else if(r.name.StartsWith("Rack")||r.name.StartsWith("Harness"))r.transform.SetParent(rack.transform,true);
   }
   var cargo=new GameObject("Cargo Slots").transform;cargo.SetParent(animator.GetBoneTransform(HumanBodyBones.Chest),false);
   cargo.position=visual.transform.TransformPoint(new Vector3(0,1.04f,-.43f));cargo.rotation=visual.transform.rotation;equipment.cargoRoot=cargo;
   string meshPath=Root+"CargoRock.asset";var mesh=AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(meshPath);
   if(!mesh){mesh=new UnityEngine.Mesh{name="Low Poly Ore"};mesh.vertices=new[]{Vector3.up,Vector3.down,Vector3.left,Vector3.right,Vector3.forward,Vector3.back};mesh.triangles=new[]{0,4,3,0,3,5,0,5,2,0,2,4,1,3,4,1,5,3,1,2,5,1,4,2};mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,meshPath);}
   var ore=AssetDatabase.LoadAssetAtPath<Material>(Root+"CargoOre.mat");if(!ore){ore=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=new Color(.32f,.29f,.23f)};AssetDatabase.CreateAsset(ore,Root+"CargoOre.mat");}
   equipment.cargoSlots=new GameObject[5];
   for(int i=0;i<5;i++){var rock=new GameObject("Cargo "+(i+1));rock.transform.SetParent(cargo,false);rock.transform.localPosition=new Vector3((i%3-1)*.12f,i/3*.12f,0);rock.transform.localScale=new Vector3(.09f,.10f,.10f);rock.AddComponent<MeshFilter>().sharedMesh=mesh;rock.AddComponent<MeshRenderer>().sharedMaterial=ore;equipment.cargoSlots[i]=rock;}
   equipment.SetCargoCount(0);
   var idle=AssetDatabase.LoadAllAssetsAtPath(Root+"Art/Optimized/MinerMobile.fbx").OfType<AnimationClip>().First(c=>!c.name.StartsWith("__preview__"));
   idle.SampleAnimation(visual,0);
   var hand=animator.GetBoneTransform(HumanBodyBones.RightHand);
   driver.carryRotation=Quaternion.Inverse(visual.transform.rotation)*hand.rotation;
   driver.carryPosition=visual.transform.InverseTransformPoint(hand.position);
   var target=new GameObject("Right Grip IK Target").transform;target.SetParent(visual.transform,false);target.SetPositionAndRotation(hand.position,hand.rotation);driver.gripTarget=target;
   var pole=new GameObject("Right Elbow IK Pole").transform;pole.SetParent(visual.transform,false);pole.position=animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position;driver.elbowPole=pole;
   var ik=hand.gameObject.AddComponent<FastIKFabric>();ik.Target=target;ik.Pole=pole;ik.ChainLength=2;ik.Iterations=12;ik.SnapBackStrength=0;ik.enabled=false;driver.rightHandIK=ik;
   string path=Root+"MinerActions.controller";
   var ac=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
   if(ac){foreach(var layer in ac.layers)foreach(var state in layer.stateMachine.states)layer.stateMachine.RemoveState(state.state);ac.parameters=new AnimatorControllerParameter[0];}
   else ac=AnimatorController.CreateAnimatorControllerAtPath(path);
   ac.AddParameter("Speed",AnimatorControllerParameterType.Float);ac.AddParameter("Vertical",AnimatorControllerParameterType.Float);
   foreach(string p in new[]{"Grounded","Mining","MineWall"})ac.AddParameter(p,AnimatorControllerParameterType.Bool);
   var sm=ac.layers[0].stateMachine;var move=sm.AddState("Locomotion");sm.defaultState=move;
   var tree=new BlendTree{name="Idle Walk Run Sprint",blendType=BlendTreeType.Simple1D,blendParameter="Speed",useAutomaticThresholds=false};
   tree.AddChild(idle,0);tree.AddChild(Clip("Movement/Walk/HumanM@Walk01_Forward"),2);tree.AddChild(Clip("Movement/Run/HumanM@Run01_Forward"),4);tree.AddChild(Clip("Movement/Sprint/HumanM@Sprint01_Forward"),6);AssetDatabase.AddObjectToAsset(tree,ac);move.motion=tree;
   var air=sm.AddState("Airborne");var airTree=new BlendTree{name="Jump and Fall",blendParameter="Vertical",useAutomaticThresholds=false};airTree.AddChild(Clip("Movement/Jump/HumanM@Fall01"),-1);airTree.AddChild(Clip("Movement/Jump/HumanM@Jump01 - Begin"),1);AssetDatabase.AddObjectToAsset(airTree,ac);air.motion=airTree;
   var land=sm.AddState("Landing");land.motion=Clip("Movement/Jump/HumanM@Jump01 - Land");Link(move,air,"Grounded",false);Link(air,land,"Grounded",true,.07f);var recover=land.AddTransition(move);recover.hasExitTime=true;recover.exitTime=.65f;recover.duration=.12f;Link(land,air,"Grounded",false);
   var ground=sm.AddState("Mine Ground");ground.motion=Clip("Work/Mining/HumanM@MiningOneHand01_R - Ground");
   var wall=sm.AddState("Mine Wall");wall.motion=Clip("Work/Mining/HumanM@MiningOneHand01_R - Wall");
   foreach(var state in new[]{ground,wall}){var enter=Link(move,state,"Mining",true,.10f);enter.AddCondition(state==wall?AnimatorConditionMode.If:AnimatorConditionMode.IfNot,0,"MineWall");Link(state,move,"Mining",false,.14f);Link(state,air,"Grounded",false,.10f);}
   animator.runtimeAnimatorController=ac;animator.applyRootMotion=false;
   player.walkSpeed=2.2f;player.runSpeed=6;player.miningDuration=((AnimationClip)ground.motion).length;
   EditorUtility.SetDirty(ac);PrefabUtility.SaveAsPrefabAsset(player.gameObject,Root+"MinerPlayer.prefab");EditorSceneManager.MarkSceneDirty(player.gameObject.scene);EditorSceneManager.SaveScene(player.gameObject.scene);AssetDatabase.SaveAssets();
   Debug.Log("Miner ready: authored locomotion and mining; Fast IK grip; removable pickaxe; empty 5-slot cargo rack. Mining seconds="+player.miningDuration);
  }
 }
}
