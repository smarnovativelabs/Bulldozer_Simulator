using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
using ConsentStatus = Firebase.Analytics.ConsentStatus;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace Bulldozer.Services
{
    public sealed class QuarryOnlineServices : MonoBehaviour
    {
        public QuarryServiceSettings settings;
        public static QuarryOnlineServices Instance { get; private set; }
        public string Status { get; private set; } = "Services prepared — initialization not requested";
        public bool InitializationRequested { get; private set; }
        public bool PrivacyRequired => InitializationRequested && settings && settings.adApplicationIdsConfigured && ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
        public bool ConsentBusy { get; private set; }
        readonly ConcurrentQueue<Action> callbacks = new ConcurrentQueue<Action>();
        bool alive, firebaseReady, firebaseStarting, maxStarting, maxReady, consentAttempted, consentReady;
        bool maxSubscribed;
        BaseTenjin tenjin;
        float lastAd, retryAt, nextRetry, lastConnect = float.NegativeInfinity;
        int retries;
        Action adFinished;
        public bool Analytics => PlayerPrefs.GetInt(Prefix + "Analytics", 0) == 1;
        public bool CrashReports => PlayerPrefs.GetInt(Prefix + "CrashReports", 0) == 1;
        public bool Attribution => PlayerPrefs.GetInt(Prefix + "Attribution", 0) == 1;
        bool TrackingPermitted {
            get {
                if(!Attribution || !settings || settings.underAgeOfConsent)return false;
#if UNITY_IOS && !UNITY_EDITOR
                return QuarryTrackingStatus() == 3;
#else
                return true;
#endif
            }
        }
        string Prefix => Bulldozer.UI.QuarryUIFeedback.PreferencePrefix + "Privacy.";
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void ResetInstance() { Instance = null; }
        void Awake() { if (Instance && Instance != this) { enabled = false; return; } Instance = this; alive = true; lastAd = Time.realtimeSinceStartup; }
        // Deliberately not called by Awake, Start, focus, or UI retry buttons.
        public void Initialize()
        {
            if (InitializationRequested) return;
            InitializationRequested = true;
            Retry();
        }
        public void Retry()
        {
            if (!InitializationRequested) return;
            if (!settings || Time.realtimeSinceStartup < nextRetry) return;
            nextRetry = Time.realtimeSinceStartup + 5;
            InitFirebase(); ApplyChoices();
            if (settings.adApplicationIdsConfigured && !ConsentBusy) GatherConsent();
            else Status = "Add Firebase config, ad app IDs and Tenjin key in Service Settings";
        }
        void GatherConsent()
        {
            ConsentBusy = true;
            try { ConsentInformation.Update(new ConsentRequestParameters { TagForUnderAgeOfConsent = settings.underAgeOfConsent }, error => callbacks.Enqueue(() => {
                consentAttempted = true;
                if (error != null) { FinishConsent(error.Message); return; }
                ConsentForm.LoadAndShowConsentFormIfRequired(e => callbacks.Enqueue(() => FinishConsent(e?.Message)));
            })); } catch (Exception) { FinishConsent("Consent service unavailable — retry later"); }
        }
        void FinishConsent(string error)
        {
            ConsentBusy = false; consentReady = consentAttempted && ConsentInformation.CanRequestAds();
            Status = error ?? "Ad privacy choices updated";
            if (consentReady) InitAds();
        }
        public void PrivacyOptions()
        {
            if (!InitializationRequested) return;
            if (ConsentBusy) return;
            if (!PrivacyRequired) { Retry(); return; }
            ConsentBusy = true;
            ConsentForm.ShowPrivacyOptionsForm(error => callbacks.Enqueue(() => FinishConsent(error?.Message)));
        }
        async void InitFirebase()
        {
            if (!settings.firebaseConfigurationAdded || firebaseReady || firebaseStarting) return;
            firebaseStarting = true;
            try {
                var status = await FirebaseApp.CheckAndFixDependenciesAsync();
                if (!alive) return;
                if (status != DependencyStatus.Available) { Status = "Firebase dependencies unavailable"; return; }
                var app = FirebaseApp.DefaultInstance;
                firebaseReady = true; Crashlytics.ReportUncaughtExceptionsAsFatal = true; ApplyChoices();
            } catch (Exception) { Status = "Firebase configuration could not initialize"; }
            finally { firebaseStarting = false; }
        }
        public void SetAnalytics(bool value) { SaveChoice("Analytics", value); }
        public void SetCrashReports(bool value) { SaveChoice("CrashReports", value); }
        public void SetAttribution(bool value) { SaveChoice("Attribution", value); }
        void SaveChoice(string key, bool value) { PlayerPrefs.SetInt(Prefix + key, value ? 1 : 0); PlayerPrefs.Save(); ApplyChoices(); }
        void ApplyChoices()
        {
            if (!InitializationRequested) return;
            if (firebaseReady) {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(false);
                FirebaseAnalytics.SetConsent(new Dictionary<ConsentType, ConsentStatus> {
                    {ConsentType.AnalyticsStorage, Analytics ? ConsentStatus.Granted : ConsentStatus.Denied},
                    {ConsentType.AdStorage, ConsentStatus.Denied}, {ConsentType.AdUserData, ConsentStatus.Denied}, {ConsentType.AdPersonalization, ConsentStatus.Denied}
                });
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(Analytics);
                Crashlytics.IsCrashlyticsCollectionEnabled = CrashReports;
            }
            if (!Attribution || settings.underAgeOfConsent) { tenjin?.OptOut(); return; }
#if UNITY_IOS && !UNITY_EDITOR
            if (QuarryTrackingStatus() != 3) { tenjin?.OptOut(); QuarryRequestTracking(); return; }
#endif
            if (tenjin == null && !string.IsNullOrWhiteSpace(settings.TenjinKey)) {
                var child = new GameObject("Tenjin"); child.transform.SetParent(transform, false);
#if UNITY_ANDROID && !UNITY_EDITOR
                tenjin = child.AddComponent<AndroidTenjin>();
#elif UNITY_IOS && !UNITY_EDITOR
                tenjin = child.AddComponent<IosTenjin>();
#else
                tenjin = child.AddComponent<DebugTenjin>();
#endif
                tenjin.Init(settings.TenjinKey);
            }
            if (tenjin != null) { tenjin.OptIn(); if (Time.realtimeSinceStartup - lastConnect > 10) { lastConnect = Time.realtimeSinceStartup; tenjin.Connect(); } }
        }
#if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern int QuarryTrackingStatus();
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern void QuarryRequestTracking();
#endif
        public void Event(string name, long value = 0)
        {
            if (!InitializationRequested) return;
            try {
                if (firebaseReady && Analytics) FirebaseAnalytics.LogEvent(name, "value", value);
                if (tenjin != null && TrackingPermitted) tenjin.SendEvent(name);
            } catch (Exception) { Status = "Telemetry event could not be sent"; }
        }
        bool AdsAllowed => consentReady && !settings.underAgeOfConsent && !(QuarryStore.Instance && QuarryStore.Instance.AdsRemoved);
        void InitAds()
        {
            if (!AdsAllowed || string.IsNullOrWhiteSpace(settings.maxSdkKey) || string.IsNullOrWhiteSpace(settings.InterstitialId)) return;
            if(maxReady) { if(!MaxSdk.IsInterstitialReady(settings.InterstitialId)){retries=0;LoadAd();} return; }
            if(maxStarting)return;
            maxStarting = true;
            if(!maxSubscribed) {
            maxSubscribed=true;
            MaxSdkCallbacks.OnSdkInitializedEvent += MaxInitialized;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += AdHidden;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += AdFailed;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += LoadFailed;
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += AdLoaded;
            }
            // MAX reads the CMP's stored TCF consent. CanRequestAds is not a personalized-consent flag.
            try { MaxSdk.SetSdkKey(settings.maxSdkKey); MaxSdk.InitializeSdk(); }
            catch (Exception) { maxStarting=false; Status = "Ad SDK initialization failed; gameplay remains available"; }
        }
        void MaxInitialized(MaxSdkBase.SdkConfiguration _) { callbacks.Enqueue(() => { maxStarting=false;maxReady = true; LoadAd(); }); }
        void LoadAd() { if (maxReady && AdsAllowed) MaxSdk.LoadInterstitial(settings.InterstitialId); }
        void AdLoaded(string id, MaxSdkBase.AdInfo _) { callbacks.Enqueue(() => { if (id == settings.InterstitialId) { retries = 0; retryAt = 0; } }); }
        void LoadFailed(string id, MaxSdkBase.ErrorInfo _) { callbacks.Enqueue(() => { if (id == settings.InterstitialId && ++retries <= 3) retryAt = Time.realtimeSinceStartup + Mathf.Pow(2, retries); }); }
        public void RoundBreak(Action proceed)
        {
            if (!maxReady || !AdsAllowed || adFinished != null || Time.realtimeSinceStartup - lastAd < settings.interstitialCooldown || !MaxSdk.IsInterstitialReady(settings.InterstitialId)) { proceed(); return; }
            adFinished = proceed; lastAd = Time.realtimeSinceStartup;
            try { MaxSdk.ShowInterstitial(settings.InterstitialId, "next_excavation"); }
            catch (Exception) { FinishAd(); }
        }
        void AdHidden(string id, MaxSdkBase.AdInfo _) { callbacks.Enqueue(() => { if (id == settings.InterstitialId) FinishAd(); }); }
        void AdFailed(string id, MaxSdkBase.ErrorInfo _, MaxSdkBase.AdInfo __) { callbacks.Enqueue(() => { if (id == settings.InterstitialId) FinishAd(); }); }
        void FinishAd() { var next = adFinished; adFinished = null; next?.Invoke(); LoadAd(); }
        void Update()
        {
            for (int i = 0; i < 8 && callbacks.TryDequeue(out var callback); i++) { try { if (alive) callback(); } catch (Exception) { Status = "Online service callback failed; retry is available"; } }
            if (retryAt > 0 && Time.realtimeSinceStartup >= retryAt) { retryAt = 0; LoadAd(); }
            if (adFinished != null && Application.isFocused && Time.realtimeSinceStartup - lastAd > 120) FinishAd();
        }
        void OnApplicationFocus(bool focus) { if (focus && settings) ApplyChoices(); }
        void OnDestroy()
        {
            alive = false; if (Instance == this) Instance = null;
            if (!maxSubscribed) return;
            MaxSdkCallbacks.OnSdkInitializedEvent -= MaxInitialized; MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= AdHidden;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= AdFailed; MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= LoadFailed;
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= AdLoaded;
        }
    }
}

