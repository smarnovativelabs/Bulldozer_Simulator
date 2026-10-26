using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Bulldozer.UI
{
    [DefaultExecutionOrder(-1000)]
    public sealed class CommonUI : MonoBehaviour
    {
        [SerializeField] private SceneLoadingService sceneLoading;
        [SerializeField] private EventSystem sharedEventSystem;
        private LoadingPanel loadingPanel;
        public static CommonUI Instance { get; private set; }
        public SceneLoadingService SceneLoading => sceneLoading;
        public bool BlocksGameplayInput => sceneLoading.IsLoading || (loadingPanel != null && loadingPanel.BlocksInput);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }
            Instance = this;
            loadingPanel = GetComponentInChildren<LoadingPanel>(true);
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            ReconcileEventSystem();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ReconcileEventSystem();

        private void ReconcileEventSystem()
        {
            // A future destination can own its EventSystem; don't leave two active systems.
            bool external = false;
            foreach (var system in FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
                if (system != sharedEventSystem && system.isActiveAndEnabled) { external = true; break; }
            sharedEventSystem.gameObject.SetActive(!external);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this) Instance = null;
        }
    }
}
