using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using TMPro;
namespace Bulldozer.Player.Editor
{
    public static class ProductionFlowSetup
    {
        static TMP_Text Label(string name,Transform parent,Vector3 local,float size,TMP_Text source)
        {
            var existing=parent.Find(name);var go=existing?existing.gameObject:new GameObject(name);
            go.transform.SetParent(parent,false);go.transform.localPosition=local;go.transform.localScale=Vector3.one;
            var text=go.GetComponent<TextMeshPro>();if(!text)text=go.AddComponent<TextMeshPro>();
            const string materialPath="Assets/0_ProjectData/Player/Production/ProductionLabel.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(!material){material=new Material(source.fontSharedMaterial);material.SetFloat(ShaderUtilities.ID_OutlineWidth,.18f);material.SetColor(ShaderUtilities.ID_OutlineColor,Color.black);AssetDatabase.CreateAsset(material,materialPath);}
            text.font=source.font;text.fontSharedMaterial=material;text.fontSize=size;text.alignment=TextAlignmentOptions.Center;
            text.color=Color.white;text.rectTransform.sizeDelta=new Vector2(3.4f,1.2f);text.enableWordWrapping=false;
            if(!go.GetComponent<ProductionBillboard>())go.AddComponent<ProductionBillboard>();
            return text;
        }
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new Exception("Edit mode required");
            var p=UnityEngine.Object.FindFirstObjectByType<MiningProduction>();Undo.RegisterFullObjectHierarchyUndo(p.gameObject,"Production flow improvements");
            p.inputStack.position=p.inputZone.transform.position+new Vector3(-.15f,.12f,.42f);
            if(!p.loadingStack){var go=new GameObject("Loading Pad Stock");go.transform.SetParent(p.transform,false);p.loadingStack=go.transform;}
            p.loadingStack.position=p.sellZone.transform.position+new Vector3(-.15f,.13f,.35f);
            if(!p.secondDelivery)
            {
                var second=UnityEngine.Object.Instantiate(p.delivery.gameObject,p.transform);second.name="Delivery Truck 2";p.secondDelivery=second.GetComponent<TruckDelivery>();
                second.transform.position=p.delivery.transform.position+new Vector3(0,0,-2.6f);
            }
            p.delivery.counter=Label("Truck Counter",p.delivery.transform,new Vector3(0,2.15f,0),5,p.truckLabel);
            p.secondDelivery.counter=Label("Truck Counter",p.secondDelivery.transform,new Vector3(0,2.15f,0),5,p.truckLabel);
            p.delivery.counter.text=p.secondDelivery.counter.text="0/2";
            // Independent parallel lanes avoid vehicles passing through each other on return.
            foreach(var truck in new[]{p.delivery,p.secondDelivery})
            {
                truck.travelSeconds=3f;
                foreach(var t in truck.GetComponentsInChildren<Transform>(true))GameObjectUtility.SetStaticEditorFlags(t.gameObject,0);
            }
            p.refineryTimer=Label("Processing Timer",p.effects.hopper.parent,new Vector3(0,3.1f,0),3,p.inputLabel);p.refineryTimer.text="ADD STONE";
            p.orderLabel=Label("Delivery Bonus",p.transform,new Vector3(-1.4f,1.2f,-3.2f),2.1f,p.truckLabel);p.orderLabel.text="DELIVERY BONUS 0/3\n3 DELIVERIES = +$30";
            p.truckLabel.text="LOADING PAD";p.inputLabel.text="REFINERY INPUT";
            if(p.rewardFeedback)p.rewardFeedback.coinSprite=null;
            EditorUtility.SetDirty(p);
            PrefabUtility.SaveAsPrefabAsset(p.gameObject,"Assets/0_ProjectData/Player/Production/MiningProductionSite.prefab");
            EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();
            return string.Join("\n",new[]{p.delivery,p.secondDelivery}.Select(t=>t.name+": "+t.body.sharedMesh.triangles.Length/3+" triangles, "+t.body.sharedMaterials.Length+" material, "+t.wheels.Length+" wheels"));
        }
        public static string Lighting()
        {
            if(EditorApplication.isPlaying)throw new Exception("Edit mode required");
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.78f,.84f,.9f);
            RenderSettings.ambientEquatorColor=new Color(.68f,.73f,.76f);
            RenderSettings.ambientGroundColor=new Color(.46f,.43f,.38f);
            var sun=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).First(l=>l.type==LightType.Directional);
            sun.intensity=1.45f;sun.color=new Color(1,.96f,.87f);sun.shadowStrength=.65f;sun.lightmapBakeType=LightmapBakeType.Mixed;EditorUtility.SetDirty(sun);
            var p=UnityEngine.Object.FindFirstObjectByType<MiningProduction>();
            int receivers=0;
            foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if(!r.enabled||r.GetComponent<TMP_Text>()||r.GetComponentInParent<MineableRock>()||r.GetComponentInParent<MinerController>()||r.GetComponentInParent<TruckDelivery>())continue;
                if(r.transform.IsChildOf(p.inputStack)||r.transform.IsChildOf(p.outputStack)||r.transform.IsChildOf(p.loadingStack))continue;
                if((GameObjectUtility.GetStaticEditorFlags(r.gameObject)&StaticEditorFlags.BatchingStatic)==0)continue;
                var f=r.GetComponent<MeshFilter>();if(!f||!f.sharedMesh)continue;
                // Generated environment meshes need a secondary UV set before baking.
                var mesh=f.sharedMesh;string path=AssetDatabase.GetAssetPath(mesh);
                if(path.StartsWith("Assets/0_ProjectData/Player/")&&path.EndsWith(".asset")&&mesh.uv2.Length==0){Unwrapping.GenerateSecondaryUVSet(mesh);EditorUtility.SetDirty(mesh);}
                GameObjectUtility.SetStaticEditorFlags(r.gameObject,GameObjectUtility.GetStaticEditorFlags(r.gameObject)|StaticEditorFlags.ContributeGI);
                r.receiveGI=ReceiveGI.Lightmaps;r.scaleInLightmap=.2f;EditorUtility.SetDirty(r);receivers++;
            }
            for(int i=0;i<2;i++)
            {
                string name="Baked Yard Fill "+i;var go=GameObject.Find(name);if(!go)go=new GameObject(name,typeof(Light));
                go.transform.position=i==0?new Vector3(3,6,-2):new Vector3(-4,6,3);
                var light=go.GetComponent<Light>();light.type=LightType.Point;light.lightmapBakeType=LightmapBakeType.Baked;light.range=18;light.intensity=3;light.color=new Color(1,.94f,.82f);light.shadows=LightShadows.Soft;
            }
            Lightmapping.TryGetLightingSettings(out var settings);
            if(!settings){settings=new LightingSettings();AssetDatabase.CreateAsset(settings,"Assets/0_ProjectData/Player/Environment/QuarryLighting.lighting");Lightmapping.lightingSettings=settings;}
            settings.bakedGI=true;settings.realtimeGI=false;settings.lightmapper=LightingSettings.Lightmapper.ProgressiveCPU;settings.lightmapResolution=8;settings.lightmapMaxSize=1024;settings.lightmapPadding=2;settings.directSampleCount=16;settings.indirectSampleCount=32;settings.environmentSampleCount=32;settings.mixedBakeMode=MixedLightingMode.Subtractive;
            EditorUtility.SetDirty(settings);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(p.gameObject.scene);
            bool started=Lightmapping.BakeAsync();return "Static lightmap receivers="+receivers+"; CPU bake started="+started;
        }
    }
}


