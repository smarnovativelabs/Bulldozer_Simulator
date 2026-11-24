using System.Collections;
using UnityEngine;

namespace Bulldozer.Services
{
    /// <summary>Persistent service ownership. All services start by default.</summary>
    [DefaultExecutionOrder(-10000), DisallowMultipleComponent]
    public sealed class QuarryServicesRoot : MonoBehaviour
    {
        [Header("Automatic online services")]
        public QuarryServiceSettings settings;
        public QuarryStore purchasing;
        [Tooltip("Owns Firebase Analytics/Crashlytics, MAX ads, Google UMP consent and Tenjin.")]
        public QuarryOnlineServices onlineServices;
        public InternetConnectivity connectivity;
        public static QuarryServicesRoot Instance { get; private set; }
        public bool InitializationRequested { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetInstance() => Instance = null;

        void Awake()
        {
            if (Instance && Instance != this) { gameObject.SetActive(false); Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start() => InitializeServices();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsureServices()
        {
            if (Instance) return;
            var owner = new GameObject("Quarry Services");
            var root = owner.AddComponent<QuarryServicesRoot>();
            root.settings = Resources.Load<QuarryServiceSettings>("QuarryServiceSettings");
            root.connectivity = owner.AddComponent<InternetConnectivity>();
            root.purchasing = owner.AddComponent<QuarryStore>();
            root.onlineServices = owner.AddComponent<QuarryOnlineServices>();
        }

        public void InitializeServices()
        {
            if (InitializationRequested || !settings || !purchasing || !onlineServices || !connectivity) return;
            InitializationRequested = true;
            purchasing.settings = settings;
            onlineServices.settings = settings;
            connectivity.ConfigureDefaultProbes();
            if (!GetComponent<QuarryServiceOverlay>()) gameObject.AddComponent<QuarryServiceOverlay>();
            StartCoroutine(InitializeRoutine());
        }

        IEnumerator InitializeRoutine()
        {
            yield return connectivity.InitializeRoutine();
            purchasing.Initialize();
            onlineServices.Initialize();
        }

        void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
