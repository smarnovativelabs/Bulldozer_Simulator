using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace Bulldozer.Player.Editor
{
 public static class OreDepositSetup
 {
  const string Root="Assets/0_ProjectData/Player/";
  [MenuItem("Tools/Bulldozer/Add Ore Deposit Area")]
  public static void Apply()
  {
   if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play mode first");
   var player=Object.FindFirstObjectByType<MinerController>();
   var root=GameObject.Find("OreDepositArea");
   if(!root)
   {
    root=new GameObject("OreDepositArea");root.transform.position=new Vector3(3.2f,0,-.6f);
    var zone=root.AddComponent<BoxCollider>();zone.isTrigger=true;zone.center=new Vector3(0,.8f,0);zone.size=new Vector3(2.4f,2,2.2f);
    var body=root.AddComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;
    root.AddComponent<OreDepositZone>().coinsPerItem=10;
    var gold=Material("DepotGold",new Color(1,.64f,.08f));var dark=Material("DepotBase",new Color(.16f,.23f,.20f));
    Box("Deposit Pad",root.transform,new Vector3(0,.025f,0),new Vector3(2.4f,.05f,2.2f),dark);
    foreach(float x in new[]{-1.15f,1.15f})Box("Yellow Edge",root.transform,new Vector3(x,.06f,0),new Vector3(.10f,.04f,2.2f),gold);
    foreach(float z in new[]{-1.05f,1.05f})Box("Yellow Edge",root.transform,new Vector3(0,.06f,z),new Vector3(2.4f,.04f,.10f),gold);
    Box("Sign Post",root.transform,new Vector3(0,.7f,1),new Vector3(.08f,1.4f,.08f),gold);
    Box("Sign Board",root.transform,new Vector3(0,1.45f,1),new Vector3(2.25f,.72f,.07f),dark);
    var label=new GameObject("Deposit Sign",typeof(TMPro.TextMeshPro));label.transform.SetParent(root.transform,false);label.transform.localPosition=new Vector3(0,1.45f,.95f);
    var text=label.GetComponent<TMPro.TextMeshPro>();text.font=player.visual.GetComponent<MinerMining>().cargoLabel.font;text.text="DEPOSIT ORE\n10 COINS EACH";text.fontSize=2;text.alignment=TMPro.TextAlignmentOptions.Center;text.color=new Color(1,.85f,.3f);text.rectTransform.sizeDelta=new Vector2(2.2f,.7f);
   }
   player.visual.GetComponent<MinerMining>().cargoLabel.text="CARGO 0/5  |  COINS 0  |  Walk onto the yellow pad to sell ore";
   PrefabUtility.SaveAsPrefabAsset(root,Root+"OreDepositArea.prefab");
   PrefabUtility.SaveAsPrefabAsset(player.gameObject,Root+"MinerPlayer.prefab");
   PrefabUtility.SaveAsPrefabAsset(player.touchControls.gameObject,Root+"MinerControls.prefab");
   EditorSceneManager.MarkSceneDirty(player.gameObject.scene);EditorSceneManager.SaveScene(player.gameObject.scene);AssetDatabase.SaveAssets();
  }
  static Material Material(string name,Color color){var m=AssetDatabase.LoadAssetAtPath<Material>(Root+name+".mat");if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=color;m.SetFloat("_Smoothness",.1f);AssetDatabase.CreateAsset(m,Root+name+".mat");}return m;}
  static void Box(string name,Transform parent,Vector3 position,Vector3 size,Material material){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(go.GetComponent<Collider>());}
 }
}

