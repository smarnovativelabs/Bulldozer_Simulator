using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace Bulldozer.Player.Editor
{
    public static class ArcadeMiningSetup
    {
        const string Root="Assets/0_ProjectData/Player/";
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new Exception("Edit mode required");
            var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();var a=p.visual.GetComponent<Animator>();var driver=p.visual.GetComponent<MinerAnimator>();var e=driver.equipment;var mining=p.visual.GetComponent<MinerMining>();
            var skin=e.pickaxe.GetComponentInChildren<SkinnedMeshRenderer>();
            if(skin)
            {
                var mesh=skin.sharedMesh;var vertices=mesh.vertices;var normals=mesh.normals;var uv=mesh.uv;var weights=mesh.boneWeights;
                int bone=weights[0].boneIndex0;
                if(weights.Any(w=>w.boneIndex0!=bone||w.weight0<.999f))throw new Exception("Pickaxe must be rigidly weighted");
                Transform hand=skin.bones[bone];var bind=mesh.bindposes[bone];
                var palm=hand.InverseTransformPoint(a.GetBoneTransform(HumanBodyBones.RightMiddleProximal).position)*.65f;
                var points=vertices.Select(v=>bind.MultiplyPoint3x4(v)).ToArray();
                var head=Enumerable.Range(0,uv.Length).Where(i=>uv[i].y>.15f&&uv[i].y<.30f).Select(i=>points[i]).ToArray();
                var center=head.Aggregate(Vector3.zero,(sum,v)=>sum+v)/head.Length;
                var tool=new GameObject("Pickaxe Palm Grip",typeof(MeshFilter),typeof(MeshRenderer));tool.transform.SetParent(hand,false);tool.transform.localPosition=palm;
                var rigid=UnityEngine.Object.Instantiate(mesh);rigid.name="Rigid Pickaxe Palm Mesh";rigid.vertices=points.Select(v=>v-palm).ToArray();rigid.normals=normals.Select(n=>bind.inverse.transpose.MultiplyVector(n).normalized).ToArray();rigid.boneWeights=Array.Empty<BoneWeight>();rigid.bindposes=Array.Empty<Matrix4x4>();rigid.RecalculateBounds();
                AssetDatabase.CreateAsset(rigid,Root+"PickaxePalm.asset");tool.GetComponent<MeshFilter>().sharedMesh=rigid;tool.GetComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;
                driver.toolGrip=tool.transform;
                Vector3 desiredLocal=Quaternion.Inverse(p.visual.rotation*driver.carryRotation)*(p.visual.rotation*new Vector3(.1f,.7f,.35f).normalized);
                driver.carryToolRotation=Quaternion.FromToRotation(center-palm,desiredLocal);
                foreach(var probe in mining.headProbes){var local=probe.localPosition;probe.SetParent(tool.transform,false);probe.localPosition=local-palm;}
                skin.enabled=false;e.pickaxe=tool;
            }
            driver.carryPosition=new Vector3(.38f,1.08f,.22f);driver.rightHandIK.Iterations=8;
            p.autoMine=true;p.analogRun=true;p.walkSpeed=2.2f;p.runSpeed=4.2f;p.acceleration=24;
            mining.reach=.95f;mining.contactRadius=.12f;
            p.followCamera.distance=8;p.followCamera.startPitch=48;
            foreach(var zone in p.touchControls.GetComponentsInChildren<MinerTouchZone>(true))
            {
                if(zone.action==MinerTouchZone.Action.Mine||zone.action==MinerTouchZone.Action.Sprint||zone.action==MinerTouchZone.Action.Jump)zone.gameObject.SetActive(false);
                if(zone.action==MinerTouchZone.Action.Move)
                {
                    var bg=zone.GetComponent<UnityEngine.UI.Image>();var knob=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                    if(bg){bg.sprite=knob;bg.color=new Color(.1f,.13f,.14f,.32f);}
                    if(zone.knob){var image=zone.knob.GetComponent<UnityEngine.UI.Image>();image.sprite=knob;image.color=new Color(1,.73f,.25f,.75f);zone.knob.sizeDelta=new Vector2(70,70);}
                }
            }
            foreach(var text in p.touchControls.GetComponentsInChildren<TMPro.TMP_Text>(true))if(text.text.Contains("WASD"))text.text="Joystick / WASD: move   |   Stop near rocks: auto-mine   |   Walk over ore: collect";
            mining.ShowStatus("Move close to a rock and stop to mine");
            var drops=UnityEngine.Object.FindFirstObjectByType<OreDropSystem>();if(!drops)drops=new GameObject("Pooled Ore Drops").AddComponent<OreDropSystem>();
            drops.equipment=e;drops.player=p;drops.materials=UnityEngine.Object.FindFirstObjectByType<MiningProduction>().stockMaterials.Take(6).ToArray();e.drops=drops;
            if(!drops.dust)drops.dust=Particles(drops.transform,"Rock Dust",false,e);
            if(!drops.chips)drops.chips=Particles(drops.transform,"Rock Chips",true,e);
            PrefabUtility.SaveAsPrefabAsset(p.gameObject,Root+"MinerPlayer.prefab");PrefabUtility.SaveAsPrefabAsset(p.touchControls.gameObject,Root+"MinerControls.prefab");
            EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();
            return "Palm-mounted pickaxe; automatic mining; analog movement; pooled dust/chips; 96 saved ore slots, six instanced resource groups";
        }
        static ParticleSystem Particles(Transform parent,string name,bool chips,MinerEquipment e)
        {
            var go=new GameObject(name,typeof(ParticleSystem));go.transform.SetParent(parent,false);var ps=go.GetComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.playOnAwake=false;main.loop=false;main.maxParticles=chips?32:24;main.startLifetime=chips?.55f:.6f;main.startSpeed=chips?2.2f:.65f;main.startSize=chips?.22f:.32f;main.startColor=new Color(.69f,.62f,.48f,.8f);main.simulationSpace=ParticleSystemSimulationSpace.World;main.gravityModifier=chips?1:0;
            var emission=ps.emission;emission.rateOverTime=0;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.16f;
            var col=ps.colorOverLifetime;col.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(.7f,0),new GradientAlphaKey(0,1)});col.color=gradient;
            var r=ps.GetComponent<ParticleSystemRenderer>();r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;
            if(chips){r.renderMode=ParticleSystemRenderMode.Mesh;r.mesh=e.rawCargoMesh;r.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"Production/Stock0.mat");}
            else r.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"Production/Smoke.mat");
            return ps;
        }
    }
}
