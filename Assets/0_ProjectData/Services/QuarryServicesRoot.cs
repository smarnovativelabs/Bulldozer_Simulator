using System.Collections;
using UnityEngine;

namespace Bulldozer.Services
{
    /// <summary>Prepared persistent ownership only. Startup is explicitly opt-in from future boot code.</summary>
    [DefaultExecutionOrder(-10000), DisallowMultipleComponent]
    public sealed class QuarryServicesRoot : MonoBehaviour
    {
        [Header("Prepared services — no automatic initialization")]
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

        // Intentionally has no callers in the current game or UI.
        public void InitializeServices()
        {
            if (InitializationRequested || !settings || !purchasing || !onlineServices || !connectivity) return;
            InitializationRequested = true;
            purchasing.settings = settings;
            onlineServices.settings = settings;
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
