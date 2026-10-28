using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
namespace Bulldozer.Player.Editor
{
    public static class QuarryGuidanceSetup
    {
        static RectTransform Rect(string name,Transform parent,Vector2 size,Vector2 position)
        {
            var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.sizeDelta=size;r.anchoredPosition=position;return r;
        }
        static TMP_Text Text(string name,Transform parent,string value,Vector2 size,Vector2 pos,float fontSize,TMP_FontAsset font)
        {
            var r=Rect(name,parent,size,pos);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSize=fontSize;t.text=value;t.color=Color.white;t.raycastTarget=false;return t;
        }
        static UnityEngine.UI.Image Image(string name,Transform parent,Vector2 size,Vector2 pos,Color color)
        {
            var r=Rect(name,parent,size,pos);var img=r.gameObject.AddComponent<UnityEngine.UI.Image>();img.color=color;img.raycastTarget=false;return img;
        }
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new System.Exception("Edit mode required");
            if(Object.FindFirstObjectByType<QuarryGuidance>())throw new System.Exception("Guidance already installed; edit it in place");
            var player=Object.FindFirstObjectByType<MinerController>();var equipment=player.visual.GetComponent<MinerEquipment>();var mining=player.visual.GetComponent<MinerMining>();var production=Object.FindFirstObjectByType<MiningProduction>();
            var root=new GameObject("Quarry Guidance");Undo.RegisterCreatedObjectUndo(root,"Add quarry guidance");var guide=root.AddComponent<QuarryGuidance>();guide.player=player;guide.equipment=equipment;guide.production=production;guide.upgrades=Object.FindFirstObjectByType<MinerUpgradeArea>();
            var surface=root.AddComponent<Unity.AI.Navigation.NavMeshSurface>();surface.collectObjects=Unity.AI.Navigation.CollectObjects.Volume;surface.center=new Vector3(0,1,1);surface.size=new Vector3(30,5,24);surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders;
            int agent=NavMesh.GetSettingsByIndex(0).agentTypeID;
            surface.agentTypeID=guide.agentTypeId=agent;surface.overrideVoxelSize=true;surface.voxelSize=.1f;
            var ignore=player.GetComponent<Unity.AI.Navigation.NavMeshModifier>();if(!ignore)ignore=player.gameObject.AddComponent<Unity.AI.Navigation.NavMeshModifier>();ignore.ignoreFromBuild=true;
            int obstacles=0;
            foreach(var rock in Object.FindObjectsByType<MineableRock>(FindObjectsSortMode.None))
            {
                var box=rock.GetComponent<BoxCollider>();var obstacle=rock.GetComponent<NavMeshObstacle>();if(!obstacle)obstacle=rock.gameObject.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Box;obstacle.center=box.center;obstacle.size=box.size;obstacle.carving=true;obstacle.carveOnlyStationary=true;obstacle.carvingTimeToStationary=.1f;obstacles++;
            }
            foreach(var truck in new[]{production.delivery,production.secondDelivery})
            {
                var box=(BoxCollider)truck.parkingCollider;var obstacle=truck.gameObject.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Box;obstacle.center=box.center;obstacle.size=box.size;obstacle.carving=true;obstacle.carveOnlyStationary=true;obstacle.carvingTimeToStationary=.5f;
            }
            surface.BuildNavMesh();AssetDatabase.CreateAsset(surface.navMeshData,"Assets/0_ProjectData/Player/Production/QuarryGuideNavMesh.asset");
            var path=new GameObject("Pooled Guidance Arrows",typeof(MeshFilter),typeof(MeshRenderer));path.transform.SetParent(root.transform,false);guide.pathFilter=path.GetComponent<MeshFilter>();guide.pathRenderer=path.GetComponent<MeshRenderer>();guide.pathRenderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;guide.pathRenderer.receiveShadows=false;
            var material=new Material(Shader.Find("Bulldozer/GuidanceUnlit"));AssetDatabase.CreateAsset(material,"Assets/0_ProjectData/Player/Production/GuidancePath.mat");guide.pathMaterial=material;
            var safe=mining.cargoLabel.transform.parent;var font=mining.cargoLabel.font;
            var panel=Rect("Foreman Guidance Dialog",safe,new Vector2(420,154),new Vector2(22,-88));guide.dialog=panel;guide.dialogGroup=panel.gameObject.AddComponent<CanvasGroup>();guide.dialogGroup.blocksRaycasts=false;guide.dialogGroup.interactable=false;
            var bg=panel.gameObject.AddComponent<UnityEngine.UI.Image>();bg.color=new Color(.035f,.075f,.1f,.94f);bg.raycastTarget=false;
            var shadow=panel.gameObject.AddComponent<UnityEngine.UI.Shadow>();shadow.effectColor=new Color(0,0,0,.3f);shadow.effectDistance=new Vector2(3,-4);
            guide.accent=Image("Accent",panel,new Vector2(5,154),Vector2.zero,new Color(1,.77f,.2f));
            guide.stepLabel=Text("Guide Step",panel,"QUARRY GUIDE  /  1 OF 5",new Vector2(380,22),new Vector2(18,-12),15,font);guide.stepLabel.color=new Color(.65f,.75f,.8f);
            guide.heading=Text("Objective",panel,"LET'S BREAK SOME ROCKS",new Vector2(384,32),new Vector2(18,-36),23,font);guide.heading.fontStyle=FontStyles.Bold;
            guide.message=Text("Guidance Dialogue",panel,"Follow the arrows. Stop beside the marked rock and your miner will swing automatically.",new Vector2(378,70),new Vector2(18,-73),18,font);
            Image("Progress Track",panel,new Vector2(384,3),new Vector2(18,-147),new Color(.15f,.25f,.3f));guide.progress=Image("Step Progress",panel,new Vector2(384,3),new Vector2(18,-147),new Color(1,.77f,.2f));guide.progress.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");guide.progress.type=UnityEngine.UI.Image.Type.Filled;guide.progress.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;guide.progress.fillAmount=.2f;
            guide.completionLabel=Text("Success Toast",safe,"NICE WORK! NEXT STEP",new Vector2(600,40),new Vector2(0,-252),24,font);var toast=guide.completionLabel.rectTransform;toast.anchorMin=toast.anchorMax=new Vector2(.5f,1);toast.pivot=new Vector2(.5f,1);guide.completionLabel.alignment=TextAlignmentOptions.Center;guide.completionLabel.gameObject.SetActive(false);
            mining.guidanceOwnsInstructions=true;EditorUtility.SetDirty(mining);
            mining.cargoLabel.text="CARGO 0/5  |  CASH $0  |  Follow your quarry guide";
            EditorSceneManager.MarkSceneDirty(root.scene);EditorSceneManager.SaveScene(root.scene);AssetDatabase.SaveAssets();
            return "Guidance installed: baked navmesh, "+obstacles+" rock obstacles, two stationary truck obstacles, one shared arrow mesh/material, non-blocking dialog. Nav triangles="+NavMesh.CalculateTriangulation().indices.Length/3;
        }
    }
}



