using System;
using UnityEngine;
using GoogleMobileAds.Ump.Api;
using Bulldozer.Player;

namespace Bulldozer.Services
{
    public sealed partial class QuarryOnlineServices : MonoBehaviour
    {
        public QuarryServiceSettings settings;
        public static QuarryOnlineServices Instance { get; private set; }
        public string Status { get; private set; } = "Starting services";
        public bool InitializationRequested { get; private set; }
        public bool PrivacyRequired => consent && ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
        public bool ConsentBusy => consent && consent.Busy;
        public bool Analytics => PlayerPrefs.GetInt(Prefix + "Analytics", 1) == 1;
        public bool CrashReports => PlayerPrefs.GetInt(Prefix + "CrashReports", 1) == 1;
        public bool Attribution => PlayerPrefs.GetInt(Prefix + "Attribution", 1) == 1;
        string Prefix => Bulldozer.UI.QuarryUIFeedback.PreferencePrefix + "Privacy.";
        QuarryConsent consent;
        bool maxReady, maxStarting, subscribed, paused, adShowing, trackingRequested;
        double consentRetry = double.PositiveInfinity, maxRetry, rewardRetry, interRetry, telemetryRetry, trackingDeadline;
        int consentAttempts, maxAttempts;
        float interstitialRemaining, countdown = -1;
        public float AdCountdownSeconds => countdown;
        public bool AdShowing => adShowing;
        bool Online => !QuarryServicesRoot.Instance || QuarryServicesRoot.Instance.connectivity.Status == ConnectivityStatus.Online;
        bool AdConsent => consent && consent.CanRequestAds && settings && !settings.underAgeOfConsent;
        bool ForcedAdsAllowed => AdConsent && !(QuarryStore.Instance && QuarryStore.Instance.AdsRemoved);
        public bool RewardReady => maxReady && AdConsent && Online && !adShowing && rewardToken == null && !string.IsNullOrWhiteSpace(settings.RewardedId) && MaxSdk.IsRewardedAdReady(settings.RewardedId);
        string rewardToken;
        bool rewardGranted;
        int rewardCash;
        double rewardSaveAt, rewardCloseAt = double.PositiveInfinity, adDeadline;
        public string AdDiagnostics => !consent ? "Starting" : consent.Describe() + " | MAX " + (maxReady ? "ready" : "starting") + " | reward " + (RewardReady ? "ready" : "loading");
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void ResetInstance() => Instance = null;
        void Awake() { if (Instance && Instance != this) { enabled = false; return; } Instance = this; }
        public static double RetryDelay(int attempt) => Math.Min(60, 5 * Math.Pow(2, Math.Min(4, Math.Max(0, attempt))));
        public void Initialize()
        {
            if (InitializationRequested || !settings) return;
            InitializationRequested = true;
            InitializeTelemetry();
            RecoverReward();
            consent = gameObject.AddComponent<QuarryConsent>();
            consent.Changed += ConsentChanged;
            ResetAdClock();
            Retry();
        }
        public void Retry()
        {
            if (!InitializationRequested || !settings) return;
            InitTelemetry();
            if (!consent.Busy && !consent.CanRequestAds && Online) consent.Gather(settings.underAgeOfConsent);
            else if (consent.CanRequestAds) InitializeAds();
            if (maxReady) { LoadInterstitial(); LoadReward(); }
        }
        void ConsentChanged()
        {
            Status = consent.Status;
            if (!consent.CanRequestAds)
            {
                countdown = -1;
                if (!consent.Busy) consentRetry = Time.realtimeSinceStartupAsDouble + RetryDelay(consentAttempts++);
                return;
            }
            consentRetry = double.PositiveInfinity; consentAttempts = 0; InitializeAds();
        }
        public void PrivacyOptions() { if (consent) consent.PrivacyOptions(); }
        public void SetAnalytics(bool value) => SaveChoice("Analytics", value);
        public void SetCrashReports(bool value) => SaveChoice("CrashReports", value);
        public void SetAttribution(bool value) => SaveChoice("Attribution", value);
        void SaveChoice(string key, bool value) { PlayerPrefs.SetInt(Prefix + key, value ? 1 : 0); PlayerPrefs.Save(); ApplyTelemetryChoices(); }
#if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern int QuarryTrackingStatus();
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern void QuarryRequestTracking();
#endif
        bool TrackingSettled()
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (!trackingRequested) { trackingRequested = true; trackingDeadline = Time.realtimeSinceStartupAsDouble + 10; try { QuarryRequestTracking(); } catch {} }
            try { return QuarryTrackingStatus() != 0 || Time.realtimeSinceStartupAsDouble >= trackingDeadline; } catch { return Time.realtimeSinceStartupAsDouble >= trackingDeadline; }
#else
            return true;
#endif
        }
        bool TrackingAuthorized
        {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                try { return QuarryTrackingStatus() == 3; } catch { return false; }
#else
                return true;
#endif
            }
        }
        void InitializeAds()
        {
            if (!AdConsent || maxReady || maxStarting || !Online || Time.realtimeSinceStartupAsDouble < maxRetry) return;
            if (string.IsNullOrWhiteSpace(settings.maxSdkKey) || string.IsNullOrWhiteSpace(settings.InterstitialId) || string.IsNullOrWhiteSpace(settings.RewardedId)) { Status = "Ad account configuration missing"; return; }
            if (!TrackingSettled()) return;
            if (!subscribed)
            {
                subscribed = true;
                MaxSdkBase.InvokeEventsOnUnityMainThread = true;
                MaxSdkCallbacks.OnSdkInitializedEvent += MaxInitialized;
                MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += InterstitialHidden;
                MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailed;
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += InterstitialLoadFailed;
                MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += Revenue;
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += RewardReceived;
                MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += RewardHidden;
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += RewardFailed;
                MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += RewardLoadFailed;
                MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += Revenue;
            }
            maxStarting = true; maxRetry = Time.realtimeSinceStartupAsDouble + 30 + RetryDelay(maxAttempts++);
            if (Debug.isDebugBuild) MaxSdk.SetVerboseLogging(true);
            if (settings.testDeviceAdvertisingIds != null && settings.testDeviceAdvertisingIds.Length > 0) MaxSdk.SetTestDeviceAdvertisingIdentifiers(settings.testDeviceAdvertisingIds);
            // SDK key comes from AppLovinSettings at build time, as in Lab Sorting.
            try { MaxSdk.InitializeSdk(); } catch { maxStarting = false; Status = "Ad SDK unavailable; retrying"; }
        }
        void MaxInitialized(MaxSdkBase.SdkConfiguration config) { if (maxReady) return; maxReady = true; maxStarting = false; maxAttempts = 0; Status = "Ads ready"; LoadInterstitial(); LoadReward(); }
        void LoadInterstitial() { if (maxReady && ForcedAdsAllowed && Online) MaxSdk.LoadInterstitial(settings.InterstitialId); }
        void LoadReward() { if (maxReady && AdConsent && Online) MaxSdk.LoadRewardedAd(settings.RewardedId); }
        void InterstitialLoadFailed(string id, MaxSdkBase.ErrorInfo error) { if (id == settings.InterstitialId) interRetry = Time.realtimeSinceStartupAsDouble + 30; }
        void RewardLoadFailed(string id, MaxSdkBase.ErrorInfo error) { if (id == settings.RewardedId) rewardRetry = Time.realtimeSinceStartupAsDouble + 15; }
        void ResetAdClock() { interstitialRemaining = settings ? Mathf.Max(30, settings.interstitialCooldown) : 120; countdown = -1; }
        bool GameplayMoment()
        {
            var ui = Bulldozer.UI.QuarryGameUI.Instance;
            return Online && !paused && Application.isFocused && Time.timeScale > 0 && ui && ui.player && !ui.player.InputBlocked && !(QuarryStore.Instance && QuarryStore.Instance.Busy);
        }
        void TickInterstitial()
        {
            if (!maxReady || !ForcedAdsAllowed || adShowing || rewardToken != null || !GameplayMoment()) { countdown = -1; return; }
            if (interstitialRemaining > 0) { interstitialRemaining -= Mathf.Min(Time.unscaledDeltaTime, .25f); return; }
            if (!MaxSdk.IsInterstitialReady(settings.InterstitialId)) { countdown = -1; return; }
            if (countdown < 0) countdown = Mathf.Clamp(settings.interstitialWarningSeconds, 1, 10);
            countdown -= Time.unscaledDeltaTime;
            if (countdown > 0) return;
            countdown = -1; adShowing = true; adDeadline = Time.realtimeSinceStartupAsDouble + 180;
            try { MaxSdk.ShowInterstitial(settings.InterstitialId, "active_gameplay"); } catch { FinishInterstitial(false); }
        }
        // Automatic active-play timing owns interstitials. Round transitions must always continue.
        public void RoundBreak(Action proceed) => proceed?.Invoke();
        void InterstitialHidden(string id, MaxSdkBase.AdInfo info) { if (id == settings.InterstitialId) FinishInterstitial(true); }
        void InterstitialFailed(string id, MaxSdkBase.ErrorInfo error, MaxSdkBase.AdInfo info) { if (id == settings.InterstitialId) FinishInterstitial(false); }
        void FinishInterstitial(bool shown) { adShowing = false; ResetAdClock(); if (!shown) interstitialRemaining = 10; LoadInterstitial(); }
        public bool WatchReward()
        {
            if (!RewardReady || !(Bulldozer.UI.QuarryGameUI.Instance && Bulldozer.UI.QuarryGameUI.Instance.equipment)) return false;
            rewardToken = "ad:" + Guid.NewGuid().ToString("N"); rewardCash = settings.rewardedCashAmount; rewardGranted = false;
            adShowing = true; rewardCloseAt = double.PositiveInfinity; adDeadline = Time.realtimeSinceStartupAsDouble + 180;
            Status = "Watching reward video";
            try { MaxSdk.ShowRewardedAd(settings.RewardedId); return true; }
            catch { adShowing = false; rewardToken = null; Status = "Video unavailable"; return false; }
        }
        void RewardReceived(string id, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo info)
        {
            if (id != settings.RewardedId || rewardToken == null || rewardGranted) return;
            rewardGranted = true; SaveReward();
        }
        void SaveReward()
        {
            if (!JournalReward()) { rewardSaveAt = Time.realtimeSinceStartupAsDouble + 2; Status = "Reward saving; retrying"; return; }
            var equipment = FindFirstObjectByType<MinerEquipment>();
            if (equipment && QuarryPurchaseGrant.Prepare(equipment.Snapshot, rewardToken, rewardCash, false, out var next) && equipment.Commit(next))
            {
                rewardSaveAt = 0; ClearRewardJournal(); Status = "Reward added"; Event("reward_ad_completed", rewardCash);
            }
            else { rewardSaveAt = Time.realtimeSinceStartupAsDouble + 2; Status = "Reward saving; retrying"; }
        }
        void RewardHidden(string id, MaxSdkBase.AdInfo info) { if (id != settings.RewardedId) return; ResetAdClock(); rewardCloseAt = Time.realtimeSinceStartupAsDouble + 2; LoadReward(); }
        void RewardFailed(string id, MaxSdkBase.ErrorInfo error, MaxSdkBase.AdInfo info) { if (id != settings.RewardedId) return; rewardCloseAt = Time.realtimeSinceStartupAsDouble + 2; ResetAdClock(); rewardRetry = Time.realtimeSinceStartupAsDouble + 5; }
        void FinishReward()
        {
            if (rewardSaveAt > 0) return;
            adShowing = false; rewardCloseAt = double.PositiveInfinity;
            if (!rewardGranted) Status = "Video closed; completion required for reward";
            rewardToken = null; rewardGranted = false;
        }
        public void ShowAdDebugger() { if (maxReady) MaxSdk.ShowMediationDebugger(); }
        void Update()
        {
            if (!InitializationRequested || !settings) return;
            double now = Time.realtimeSinceStartupAsDouble;
            if (now >= telemetryRetry) { telemetryRetry = now + 5; InitTelemetry(); }
            TickTelemetry(now);
            if (now >= consentRetry && Online) { consentRetry = double.PositiveInfinity; if (!consent.Busy) consent.Gather(settings.underAgeOfConsent); }
            if (!consent.Attempted && !consent.Busy && Online) consent.Gather(settings.underAgeOfConsent);
            if (maxStarting && now >= maxRetry) { maxStarting = false; if (MaxSdk.IsInitialized()) MaxInitialized(MaxSdk.GetSdkConfiguration()); }
            if (AdConsent && !maxReady) InitializeAds();
            if (Online && interRetry > 0 && now >= interRetry) { interRetry = 0; LoadInterstitial(); }
            if (Online && rewardRetry > 0 && now >= rewardRetry) { rewardRetry = 0; LoadReward(); }
            if (rewardSaveAt > 0 && now >= rewardSaveAt) SaveReward();
            if (now >= rewardCloseAt) FinishReward();
            if (adShowing && !paused && Application.isFocused && now >= adDeadline) { if (rewardToken != null) FinishReward(); else FinishInterstitial(false); }
            TickInterstitial();
        }
        void OnApplicationPause(bool value) { paused = value; countdown = -1; if (value) PersistTelemetry(); else { telemetryRetry = 0; ReconnectTenjin(); } }
        void OnApplicationFocus(bool focus) { if (!focus) countdown = -1; }
        void OnDestroy()
        {
            PersistTelemetry();
            if (Instance == this) Instance = null;
            if (consent) consent.Changed -= ConsentChanged;
            if (!subscribed) return;
            MaxSdkCallbacks.OnSdkInitializedEvent -= MaxInitialized;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= InterstitialHidden;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= InterstitialFailed;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= InterstitialLoadFailed;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= Revenue;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= RewardReceived;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= RewardHidden;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= RewardFailed;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= RewardLoadFailed;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= Revenue;
        }
    }
}
