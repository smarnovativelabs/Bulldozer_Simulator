using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
namespace Bulldozer.Player.Editor
{
 public static class MinerMiningSetup
 {
  const string Root="Assets/0_ProjectData/Player/";
  [MenuItem("Tools/Bulldozer/Connect Mining Rocks and Cargo")]
  public static void Apply()
  {
   if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play mode first.");
   var player=Object.FindFirstObjectByType<MinerController>();
   var visual=player.visual.gameObject;var equipment=visual.GetComponent<MinerEquipment>();
   var mining=visual.GetComponent<MinerMining>();if(!mining)mining=visual.AddComponent<MinerMining>();mining.controller=player;mining.equipment=equipment;
   var renderer=equipment.pickaxe.GetComponentInChildren<SkinnedMeshRenderer>();
   var mesh=renderer.sharedMesh;var uv=mesh.uv;var verts=mesh.vertices;var weights=mesh.boneWeights;
   var indices=Enumerable.Range(0,uv.Length).Where(i=>uv[i].y>.15f && uv[i].y<.30f).ToArray();
   if(indices.Length==0)throw new System.InvalidOperationException("No steel-head vertices in pickaxe palette.");
   int bone=weights[indices[0]].boneIndex0;
   if(indices.Any(i=>weights[i].boneIndex0!=bone || weights[i].weight0<.999f))throw new System.InvalidOperationException("Tool head must be rigidly bound to one hand.");
   var points=indices.Select(i=>mesh.bindposes[bone].MultiplyPoint3x4(verts[i])).ToArray();
   Vector3 center=points.Aggregate(Vector3.zero,(a,b)=>a+b)/points.Length;
   Vector3 tipA=points.OrderByDescending(v=>(v-center).sqrMagnitude).First();Vector3 tipB=points.OrderByDescending(v=>(v-tipA).sqrMagnitude).First();
   if(mining.headProbes!=null)foreach(var t in mining.headProbes)if(t)Object.DestroyImmediate(t.gameObject);
   mining.headProbes=new Transform[3];var positions=new[]{center,tipA,tipB};
   for(int i=0;i<3;i++){var t=new GameObject("Pickaxe Contact "+i).transform;t.SetParent(renderer.bones[bone],false);t.localPosition=positions[i];mining.headProbes[i]=t;}
   var ac=(AnimatorController)visual.GetComponent<Animator>().runtimeAnimatorController;
   foreach(string variant in new[]{"Ground","Wall"})
   {
    string source="Assets/Kevin Iglesias/Human Animations/Animations/Male/Work/Mining/HumanM@MiningOneHand01_R - "+variant+".fbx";
    var original=AssetDatabase.LoadAllAssetsAtPath(source).OfType<AnimationClip>().First(c=>!c.name.StartsWith("__preview"));
    string path=Root+"Mining"+variant+".anim";var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
    if(!clip){clip=Object.Instantiate(original);clip.name="Mining "+variant+" With Contact";AssetDatabase.CreateAsset(clip,path);}
    var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=false;AnimationUtility.SetAnimationClipSettings(clip,settings);
    AnimationUtility.SetAnimationEvents(clip,new[]{new AnimationEvent{functionName="BeginMiningContact",time=clip.length*.30f},new AnimationEvent{functionName="EndMiningContact",time=clip.length*.65f}});
    ac.layers[0].stateMachine.states.First(s=>s.state.name=="Mine "+variant).state.motion=clip;EditorUtility.SetDirty(clip);
   }
   var safe=player.touchControls.safeArea;var existing=safe.Find("CargoStatus");TMPro.TextMeshProUGUI text;
   if(existing)text=existing.GetComponent<TMPro.TextMeshProUGUI>();
   else
   {
    var go=new GameObject("CargoStatus",typeof(RectTransform),typeof(TMPro.TextMeshProUGUI));go.transform.SetParent(safe,false);text=go.GetComponent<TMPro.TextMeshProUGUI>();
    var rt=(RectTransform)go.transform;rt.anchorMin=rt.anchorMax=new Vector2(.5f,1);rt.pivot=new Vector2(.5f,1);rt.anchoredPosition=new Vector2(0,-54);rt.sizeDelta=new Vector2(1100,36);
    text.font=safe.GetComponentInChildren<TMPro.TMP_Text>().font;text.fontSize=21;text.alignment=TMPro.TextAlignmentOptions.Center;text.color=new Color(1,.82f,.35f);text.raycastTarget=false;
   }
   text.text="CARGO 0/5  |  Approach a rock and press SWING";mining.cargoLabel=text;
   if(!GameObject.Find("MineableRocks"))
   {
    var root=new GameObject("MineableRocks");
    var material=AssetDatabase.LoadAssetAtPath<Material>(Root+"MineableStone.mat");if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=new Color(.40f,.37f,.31f);material.SetFloat("_Smoothness",.05f);AssetDatabase.CreateAsset(material,Root+"MineableStone.mat");}
    var source=AssetDatabase.LoadAllAssetsAtPath("Assets/1_Plugins/Kenney/kenney_nature-kit/Models/FBX format/rock_largeA.fbx").OfType<UnityEngine.Mesh>().First();
    for(int i=0;i<6;i++)
    {
     bool wall=i>=4;string path=Root+(wall?"MiningWallRock.asset":"MiningGroundRock.asset");var rockMesh=AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(path);
     if(!rockMesh){rockMesh=Object.Instantiate(source);var bounds=source.bounds;var size=new Vector3(.85f,wall?1.25f:.55f,.75f);rockMesh.vertices=source.vertices.Select(v=>new Vector3((v.x-bounds.center.x)/bounds.size.x*size.x,(v.y-bounds.min.y)/bounds.size.y*size.y,(v.z-bounds.center.z)/bounds.size.z*size.z)).ToArray();rockMesh.RecalculateBounds();rockMesh.RecalculateNormals();AssetDatabase.CreateAsset(rockMesh,path);}
     var go=new GameObject((wall?"Wall Ore ":"Ground Ore ")+(i+1));go.transform.SetParent(root.transform,false);go.transform.position=new[]{new Vector3(0,0,1.15f),new Vector3(1.8f,0,1.2f),new Vector3(-1.8f,0,1.2f),new Vector3(2,0,4),new Vector3(-2,0,4),new Vector3(0,0,4.5f)}[i];
     var shape=new GameObject("Rock Mesh");shape.transform.SetParent(go.transform,false);shape.AddComponent<MeshFilter>().sharedMesh=rockMesh;var r=shape.AddComponent<MeshRenderer>();r.sharedMaterial=material;
     var collider=go.AddComponent<MeshCollider>();collider.sharedMesh=rockMesh;collider.convex=true;
     var rock=go.AddComponent<MineableRock>();rock.rockRenderer=r;rock.maxHealth=3;rock.wallMining=wall;
    }
   }
   EditorUtility.SetDirty(ac);PrefabUtility.SaveAsPrefabAsset(player.gameObject,Root+"MinerPlayer.prefab");PrefabUtility.SaveAsPrefabAsset(player.touchControls.gameObject,Root+"MinerControls.prefab");EditorSceneManager.MarkSceneDirty(player.gameObject.scene);EditorSceneManager.SaveScene(player.gameObject.scene);AssetDatabase.SaveAssets();
   Debug.Log("Mining connected: physical head contact, 3-hit rocks, 5 cargo slots and cargo HUD.");
  }
 }
}
