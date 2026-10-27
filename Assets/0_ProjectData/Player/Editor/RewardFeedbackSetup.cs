using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace Bulldozer.Player.Editor
{
    public static class RewardFeedbackSetup
    {
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Edit mode required");
            var controls=UnityEngine.Object.FindFirstObjectByType<MinerTouchControls>();
            var mining=UnityEngine.Object.FindFirstObjectByType<MinerMining>();
            var production=UnityEngine.Object.FindFirstObjectByType<MiningProduction>();
            var child=controls.safeArea.Find("RewardCoins");
            var go=child?child.gameObject:new GameObject("RewardCoins",typeof(RectTransform),typeof(Canvas),typeof(CanvasRenderer),typeof(RewardCoinFeedback));
            if(!go.GetComponent<CanvasRenderer>())go.AddComponent<CanvasRenderer>();
            var rect=go.GetComponent<RectTransform>();rect.SetParent(controls.safeArea,false);rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;rect.SetAsLastSibling();
            // Separate rebuild boundary, same overlay hierarchy; no raycaster or layout components.
            var feedback=go.GetComponent<RewardCoinFeedback>();feedback.balanceText=mining.cargoLabel;feedback.worldCamera=Camera.main;feedback.raycastTarget=false;
            production.rewardFeedback=feedback;
            EditorSceneManager.MarkSceneDirty(production.gameObject.scene);EditorSceneManager.SaveScene(production.gameObject.scene);
            PrefabUtility.SaveAsPrefabAsset(controls.gameObject,"Assets/0_ProjectData/Player/MinerControls.prefab");
            AssetDatabase.SaveAssets();return "Reward overlay wired: fixed eight-coin buffer, one graphic, no raycasts, cosmetic post-commit call.";
        }
    }
}
