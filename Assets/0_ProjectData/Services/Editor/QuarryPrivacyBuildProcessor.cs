#if UNITY_EDITOR && UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Bulldozer.Services.Editor
{
    public static class QuarryPrivacyBuildProcessor
    {
        [PostProcessBuild(1000)]
        public static void ApplyFirstInstallPrivacyDefaults(BuildTarget target, string buildPath)
        {
            if (target != BuildTarget.iOS) return;
            string path = Path.Combine(buildPath, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(path);
            plist.root.SetBoolean("FIREBASE_ANALYTICS_COLLECTION_ENABLED", false);
            plist.root.SetBoolean("FirebaseCrashlyticsCollectionEnabled", false);
            plist.root.SetBoolean("GOOGLE_ANALYTICS_IDFA_COLLECTION_ENABLED", false);
            plist.root.SetBoolean("GOOGLE_ANALYTICS_DEFAULT_ALLOW_ANALYTICS_STORAGE", false);
            plist.root.SetBoolean("GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_STORAGE", false);
            plist.root.SetBoolean("GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_PERSONALIZATION_SIGNALS", false);
            plist.root.SetBoolean("GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_USER_DATA", false);
            plist.root.SetString("NSUserTrackingUsageDescription", "Allow attribution to help us understand which ads introduce players to this quarry game.");
            plist.WriteToFile(path);
            string projectPath = PBXProject.GetPBXProjectPath(buildPath);
            var project = new PBXProject(); project.ReadFromFile(projectPath);
            string framework = project.GetUnityFrameworkTargetGuid();
            project.AddFrameworkToProject(framework, "AppTrackingTransparency.framework", true);
            project.SetBuildProperty(framework, "SWIFT_VERSION", "5.0");
            project.WriteToFile(projectPath);
        }
    }
}
#endif

