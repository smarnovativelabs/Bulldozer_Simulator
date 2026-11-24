using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Bulldozer.Services.Editor
{
    public sealed class QuarryServiceBuildValidation : IPreprocessBuildWithReport
    {
        public int callbackOrder => -2000;
        public static List<string> Problems(BuildTarget target)
        {
            var errors = new List<string>();
            var s = Resources.Load<QuarryServiceSettings>("QuarryServiceSettings");
            if (!s) { errors.Add("Missing QuarryServiceSettings"); return errors; }
            bool ios = target == BuildTarget.iOS;
            string package = PlayerSettings.GetApplicationIdentifier(ios ? NamedBuildTarget.iOS : NamedBuildTarget.Android);
            string remove = ios ? s.iosRemoveAdsId : s.androidRemoveAdsId, cash = ios ? s.iosCashPackId : s.androidCashPackId;
            if (string.IsNullOrWhiteSpace(remove) || string.IsNullOrWhiteSpace(cash) || remove == cash) errors.Add("Two unique IAP product IDs are required");
            if (!ios) {
                try { if (Convert.FromBase64String(s.googlePlayPublicKey).Length < 128) errors.Add("Google Play public key incomplete"); } catch { errors.Add("Google Play public key invalid"); }
                if (!File.Exists("Assets/google-services.json") || !File.ReadAllText("Assets/google-services.json").Contains("\"" + package + "\"")) errors.Add("Firebase Android configuration does not match the package");
            }
            if (!s.firebaseConfigurationAdded) errors.Add("Firebase startup configuration disabled");
            if (!s.adApplicationIdsConfigured) errors.Add("Ad consent application configuration disabled");
            if (string.IsNullOrWhiteSpace(s.maxSdkKey) || string.IsNullOrWhiteSpace(ios ? s.iosInterstitialId : s.androidInterstitialId) || string.IsNullOrWhiteSpace(ios ? s.iosRewardedId : s.androidRewardedId)) errors.Add("MAX account IDs missing");
            if (string.IsNullOrWhiteSpace(ios ? s.iosTenjinKey : s.androidTenjinKey)) errors.Add("Tenjin platform key missing");
            if (!Uri.TryCreate(s.privacyPolicyUrl, UriKind.Absolute, out var url) || url.Scheme != "https") errors.Add("HTTPS privacy policy missing");
            if (!File.Exists("Assets/MaxSdk/Mediation/Google/Editor/Dependencies.xml")) errors.Add("MAX Google mediation adapter missing");
            if (s.cashPackAmount <= 0 || s.rewardedCashAmount <= 0) errors.Add("Cash rewards must be positive");
            if (AppLovinSettings.Instance.SdkKey != s.maxSdkKey) errors.Add("MAX build key differs from service settings");
            if (string.IsNullOrWhiteSpace(ios ? AppLovinSettings.Instance.AdMobIosAppId : AppLovinSettings.Instance.AdMobAndroidAppId)) errors.Add("AdMob application ID missing");
            return errors;
        }
        [MenuItem("Tools/Bulldozer/Services/Apply account settings")]
        public static void Apply()
        {
            var s = Resources.Load<QuarryServiceSettings>("QuarryServiceSettings");
            if (!s) throw new InvalidOperationException("Missing settings");
            AppLovinSettings.Instance.SdkKey = s.maxSdkKey;
            EditorUtility.SetDirty(AppLovinSettings.Instance);
            var privacy = AppLovinMax.Scripts.IntegrationManager.Editor.AppLovinInternalSettings.Instance;
            // One UMP owner and one bounded native ATT request; avoid a duplicate MAX flow.
            privacy.ConsentFlowEnabled = false; privacy.ConsentFlowPrivacyPolicyUrl = s.privacyPolicyUrl;
            privacy.OverrideDefaultUserTrackingUsageDescriptions = true;
            privacy.UserTrackingUsageDescriptionEn = "Your device information helps us measure ad performance.";
            privacy.Save();
            AssetDatabase.SaveAssets();
        }
        [MenuItem("Tools/Bulldozer/Services/Validate Android configuration")]
        public static void Validate()
        {
            Apply();
            var problems = Problems(BuildTarget.Android);
            Directory.CreateDirectory("Logs");
            File.WriteAllLines("Logs/QuarryServiceValidation.txt", problems.Count == 0 ? new[] { "Android account fields verified. Store catalog, consent dashboard and device delivery still require device testing." } : problems);
            if (problems.Count > 0) throw new InvalidOperationException(string.Join("\n", problems));
            Debug.Log("Quarry Android service configuration verified.");
        }
        public void OnPreprocessBuild(BuildReport report)
        {
            Apply();
            var problems = Problems(report.summary.platform);
            if (problems.Count > 0) throw new BuildFailedException(string.Join("\n", problems));
        }
    }
}
