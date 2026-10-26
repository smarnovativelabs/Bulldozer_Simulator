using UnityEngine;

namespace Bulldozer.UI
{
    /// <summary>MainMenu is only an entry scene until a menu is explicitly designed.</summary>
    public sealed class MainMenuBootstrap : MonoBehaviour
    {
        [SerializeField] private CommonUI commonUIPrefab;
        [SerializeField] private string gameplayScene = "Assets/0_ProjectData/0_Scenes/1_Gameplay.unity";

        private System.Collections.IEnumerator Start()
        {
            CommonUI ui = CommonUI.Instance;
            if (ui == null) ui = Instantiate(commonUIPrefab);
            // Returning here through the shared loader must finish that transition first.
            while (ui.SceneLoading.IsLoading) yield return null;
            ui.SceneLoading.TryLoadScene(gameplayScene);
        }
    }
}
