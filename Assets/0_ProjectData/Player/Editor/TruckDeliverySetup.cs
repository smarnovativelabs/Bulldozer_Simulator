using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Bulldozer.Player.Editor
{
    public static class TruckDeliverySetup
    {
        const string Folder = "Assets/0_ProjectData/Player/Production/";
        public static string Apply()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit mode required");
            var production = UnityEngine.Object.FindFirstObjectByType<MiningProduction>();
            var root = production.truckVisual;
            const string path = "Assets/0_ProjectData/Player/Art/Truck/TruckDelivery.fbx";
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.importAnimation = false;
            importer.isReadable = false;
            importer.skinWeights = ModelImporterSkinWeights.Custom;
            importer.maxBonesPerVertex = 1;
            importer.SaveAndReimport();
            foreach (Transform child in root.Cast<Transform>().ToArray())
                if (child != production.truckStack) UnityEngine.Object.DestroyImmediate(child.gameObject);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var model = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            model.name = "MAN Truck - single renderer wheel rig";
            model.transform.SetParent(root, false);
            model.transform.localScale = asset.transform.localScale;
            model.transform.localRotation = Quaternion.Euler(0,90,0) * asset.transform.localRotation;
            var renderer = model.GetComponentInChildren<SkinnedMeshRenderer>();
            var b = renderer.bounds;
            model.transform.localScale *= 3.8f / Mathf.Max(b.size.x,b.size.z);
            b = renderer.bounds;
            model.transform.position += new Vector3(root.position.x-b.center.x,-b.min.y,root.position.z-b.center.z);
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Folder+"MobileTruck.mat");
            renderer.quality = SkinQuality.Bone1;
            renderer.updateWhenOffscreen = false;
            renderer.skinnedMotionVectors = false;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            var animator = model.GetComponent<Animator>();
            if (animator) UnityEngine.Object.DestroyImmediate(animator);
            var delivery = root.GetComponent<TruckDelivery>();
            if (!delivery) delivery = root.gameObject.AddComponent<TruckDelivery>();
            delivery.body = renderer;
            delivery.wheels = renderer.bones.Where(t=>t.name.StartsWith("wheel_")).ToArray();
            if (delivery.wheels.Length!=6) throw new Exception("Expected six wheel bones");
            delivery.cargo = production.truckStack;
            delivery.parkingCollider = root.GetComponent<BoxCollider>();
            var front = delivery.wheels.Where(t=>t.name.Contains("front")).Aggregate(Vector3.zero,(sum,t)=>sum+t.position)/2;
            var rear = delivery.wheels.Where(t=>t.name.Contains("back")).Aggregate(Vector3.zero,(sum,t)=>sum+t.position)/4;
            var direction = front-rear; direction.y=0;
            delivery.localTravelDirection = root.parent.InverseTransformDirection(direction.normalized);
            delivery.wheelRadius = (front.y+rear.y)/2;
            production.delivery = delivery;
            production.stockMaterials = new Material[12];
            for(int i=0;i<12;i++)
            {
                string matPath=Folder+"Stock"+i+".mat";
                var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if(!mat){mat=new Material(production.effects.itemMaterial);AssetDatabase.CreateAsset(mat,matPath);}
                mat.color=MiningResources.Color(i);mat.enableInstancing=true;EditorUtility.SetDirty(mat);
                production.stockMaterials[i]=mat;
            }
            // Only immovable scenery is statically batched: never the truck, ore or pooled cargo.
            var scenery = production.GetComponentsInChildren<MeshRenderer>(true)
                .Where(r=>r.enabled&&r.gameObject.activeInHierarchy&&!r.transform.IsChildOf(root)
                    &&!r.GetComponentInParent<MineableRock>()&&!r.transform.IsChildOf(production.inputStack)
                    &&!r.transform.IsChildOf(production.outputStack)&&!r.GetComponent<TMPro.TMP_Text>()).ToArray();
            foreach(var r in scenery) GameObjectUtility.SetStaticEditorFlags(r.gameObject,
                GameObjectUtility.GetStaticEditorFlags(r.gameObject)|StaticEditorFlags.BatchingStatic);
            foreach(var t in root.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(t.gameObject,0);
            PrefabUtility.SaveAsPrefabAsset(production.gameObject,Folder+"MiningProductionSite.prefab");
            EditorSceneManager.MarkSceneDirty(production.gameObject.scene);
            EditorSceneManager.SaveScene(production.gameObject.scene);AssetDatabase.SaveAssets();
            return "Truck: "+renderer.sharedMesh.triangles.Length/3+" triangles, "+renderer.sharedMaterials.Length+" material, "+delivery.wheels.Length+" wheels. Direction="+delivery.localTravelDirection+" radius="+delivery.wheelRadius+". Static scenery renderers="+scenery.Length;
        }
    }
}
