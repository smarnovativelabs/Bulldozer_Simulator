using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace Bulldozer.Player.Editor
{
    public static class MiningParticlesSetup
    {
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new System.Exception("Edit mode required");
            var drops=Object.FindFirstObjectByType<OreDropSystem>();Undo.RegisterFullObjectHierarchyUndo(drops.gameObject,"Polish mining particles");
            if(!drops.sparks){var go=new GameObject("Pickaxe Sparks");go.transform.SetParent(drops.transform,false);drops.sparks=go.AddComponent<ParticleSystem>();}
            var sparks=drops.sparks;sparks.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=sparks.main;main.loop=false;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=48;main.gravityModifier=1.1f;main.startSpeed=0;main.startLifetime=.35f;main.startSize=.06f;
            var emission=sparks.emission;emission.enabled=false;var shape=sparks.shape;shape.enabled=false;var collision=sparks.collision;collision.enabled=false;var lights=sparks.lights;lights.enabled=false;var trails=sparks.trails;trails.enabled=false;
            var fade=sparks.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(new Color(1,.55f,.15f),1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(1,.35f),new GradientAlphaKey(0,1)});fade.color=gradient;
            var size=sparks.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,1,1,0));
            const string path="Assets/0_ProjectData/Player/Production/MiningSpark.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!mat){mat=new Material(drops.dust.GetComponent<ParticleSystemRenderer>().sharedMaterial);mat.name="MiningSpark";mat.SetTexture("_BaseMap",Texture2D.whiteTexture);if(mat.HasProperty("_BaseColor"))mat.SetColor("_BaseColor",Color.white);AssetDatabase.CreateAsset(mat,path);}
            var renderer=sparks.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=mat;renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.lengthScale=2;renderer.velocityScale=.12f;renderer.cameraVelocityScale=0;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.motionVectorGenerationMode=MotionVectorGenerationMode.ForceNoMotion;
            var dust=drops.dust;dust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);main=dust.main;main.maxParticles=24;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;main.gravityModifier=-.025f;
            var dustSize=dust.sizeOverLifetime;dustSize.enabled=true;dustSize.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.35f),new Keyframe(.4f,.85f),new Keyframe(1,1.35f)));
            var dustFade=dust.colorOverLifetime;dustFade.enabled=true;var dg=new Gradient();dg.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.12f),new GradientAlphaKey(.65f,.4f),new GradientAlphaKey(0,1)});dustFade.color=dg;
            EditorUtility.SetDirty(drops);EditorUtility.SetDirty(dust);EditorUtility.SetDirty(sparks);
            EditorSceneManager.MarkSceneDirty(drops.gameObject.scene);EditorSceneManager.SaveScene(drops.gameObject.scene);AssetDatabase.SaveAssets();
            return "Sparks: 8 per hit / 14 on break, cap 48. Dust: 14 on completion only, cap 24. Existing chip cap 32; no new collisions, lights or instantiated bursts.";
        }
    }
}
