using UnityEngine;

namespace Bulldozer.UI
{
    // Applies the saved frame rate before the first scene loads, then watches real frame times.
    // When the player has not picked a mode and the device cannot hold 60 FPS for two windows in a
    // row (slow GPU or thermal throttling), it switches to 30 FPS and remembers that for later
    // launches. Settings > Reset clears the flag so the device is measured again.
    public sealed class QuarryFrameRateGuard : MonoBehaviour
    {
        public const float WarmupSeconds = 12f, WindowSeconds = 8f, LowFps = 45f, HitchSeconds = .25f;
        public const int StrikesToFallBack = 2;
        // Editor frame rates say nothing about phones, so the guard only runs in builds unless a test enables it.
        public static bool EnabledInEditor;
        public static QuarryFrameRateGuard Instance { get; private set; }
        public float LastWindowFps { get; private set; }
        public int Strikes { get; private set; }
        public int WindowsMeasured { get; private set; }
        float warmup, sampled;
        int frames;
        bool watching = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            EnabledInEditor = false;
            QuarryUIFeedback.ApplySettings();
            if (Instance) return;
            var go = new GameObject("Frame Rate Guard");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<QuarryFrameRateGuard>();
        }

        // Restart measuring, e.g. after the player changes the setting.
        public void Restart(float warmupSeconds = WarmupSeconds)
        {
            warmup = WarmupSeconds - warmupSeconds; sampled = 0; frames = 0; Strikes = 0; watching = true;
        }

        void OnApplicationPause(bool paused) { if (!paused) Restart(2f); }

        void Update()
        {
            if (!watching || (Application.isEditor && !EnabledInEditor)) return;
            float dt = Time.unscaledDeltaTime;
            if (warmup < WarmupSeconds) { warmup += dt; return; }
            // Loading screens and app switches are not gameplay frame rate.
            if (dt > HitchSeconds) return;
            sampled += dt; frames++;
            if (sampled < WindowSeconds) return;
            LastWindowFps = frames / sampled; WindowsMeasured++; sampled = 0; frames = 0;
            // PlayerPrefs is read once per window, never per frame.
            if (QuarryUIFeedback.PerformanceChosen || QuarryUIFeedback.PerformanceMode == 0) { watching = false; return; }
            Strikes = LastWindowFps < LowFps ? Strikes + 1 : 0;
            if (Strikes < StrikesToFallBack) return;
            PlayerPrefs.SetInt(QuarryUIFeedback.PreferencePrefix + "AutoLowFps", 1); PlayerPrefs.Save();
            QuarryUIFeedback.ApplySettings();
            watching = false;
        }
    }
}
