using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
namespace Bulldozer.Player.Editor
{
    public static class MinerUpgrade
    {
        const string Root="Assets/0_ProjectData/Player/";
        const string Art=Root+"Art/Optimized/";
        [MenuItem("Tools/Bulldozer/Apply Optimized Miner")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play mode first.");
            var player=Object.FindFirstObjectByType<MinerController>();
            if(!player)throw new System.InvalidOperationException("Open the gameplay scene first.");
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(Art+"MinerMobile.fbx");
            var idle=AssetDatabase.LoadAllAssetsAtPath(Art+"MinerMobile.fbx").OfType<AnimationClip>().First(c=>!c.name.StartsWith("__preview__"));
            var walk=AssetDatabase.LoadAllAssetsAtPath(Art+"WorkerMotions.fbx").OfType<AnimationClip>().First(c=>!c.name.StartsWith("__preview__")&&c.name.EndsWith("Walk"));
            string path=Root+"MinerLocomotion.controller";
            var ac=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if(!ac)
            {
                ac=AnimatorController.CreateAnimatorControllerAtPath(path);ac.AddParameter("Speed",AnimatorControllerParameterType.Float);
                var state=ac.layers[0].stateMachine.AddState("Locomotion");var tree=new BlendTree { name="Idle Walk Run",blendType=BlendTreeType.Simple1D,blendParameter="Speed",useAutomaticThresholds=false };
                tree.AddChild(idle,0);tree.AddChild(walk,3.2f);tree.AddChild(walk,5.5f);var children=tree.children;children[2].timeScale=1.65f;tree.children=children;
                AssetDatabase.AddObjectToAsset(tree,ac);state.motion=tree;ac.layers[0].stateMachine.defaultState=state;
            }
            var visual=(GameObject)PrefabUtility.InstantiatePrefab(model);visual.name="MinerMobileVisual";visual.transform.SetParent(player.transform,false);
            var animator=visual.GetComponent<Animator>();animator.runtimeAnimatorController=ac;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            var driver=visual.AddComponent<MinerAnimator>();driver.controller=player;
            foreach(var r in visual.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                r.sharedMaterials=r.sharedMaterials.Select(m=>MakeMaterial(m.name)).ToArray();r.updateWhenOffscreen=false;
            }
            if(player.visual)Object.DestroyImmediate(player.visual.gameObject);
            player.visual=visual.transform;
            PrefabUtility.SaveAsPrefabAsset(player.gameObject,Root+"MinerPlayer.prefab");
            EditorSceneManager.MarkSceneDirty(player.gameObject.scene);EditorSceneManager.SaveScene(player.gameObject.scene);AssetDatabase.SaveAssets();Selection.activeGameObject=player.gameObject;
            Debug.Log("Optimized miner applied with humanoid breathing and walk animations.");
        }
        static Material MakeMaterial(string name)
        {
            string path=Art+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.color=Color.white;m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+name+".png");m.SetFloat("_Smoothness",.15f);
            if(name=="WorkerHair") {m.SetFloat("_AlphaClip",1);m.EnableKeyword("_ALPHATEST_ON");m.SetFloat("_Cutoff",.4f);m.SetFloat("_Cull",0);m.renderQueue=2450;}
            EditorUtility.SetDirty(m);return m;
        }
    }
}
