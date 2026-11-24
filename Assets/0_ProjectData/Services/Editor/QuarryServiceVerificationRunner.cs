using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bulldozer.Services.Editor
{
    // A one-shot marker is used when the editor's tool connection is rebuilding.
    // No work is performed unless explicitly requested by the marker/menu.
    [InitializeOnLoad]
    public static class QuarryServiceVerificationRunner
    {
        const string Request = "Library/QuarryServiceVerification.request";
        const string Smoke = "Quarry.Services.Smoke";
        static double nextSmoke;
        static QuarryServiceVerificationRunner() { EditorApplication.update += Tick; }
        [MenuItem("Tools/Bulldozer/Services/Run recovery checks")]
        public static void RequestChecks() => File.WriteAllText(Request, "checks");
        static void Tick()
        {
            if (SessionState.GetBool(Smoke, false)) { SmokeTick(); return; }
            if (!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            bool smoke = File.ReadAllText(Request).Trim() == "smoke";
            File.Delete(Request);
            Directory.CreateDirectory("Logs");
            try {
                QuarryServiceBuildValidation.Validate();
                string report = QuarryServiceChecks.Run();
                var config = Resources.Load<QuarryServiceSettings>("QuarryServiceSettings");
                report += "\nReward cash: " + config.rewardedCashAmount + "; interstitial seconds: " + config.interstitialCooldown + "; warning: " + config.interstitialWarningSeconds;
                report += "\nActive scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
                File.WriteAllText("Logs/QuarryServiceChecks.txt", report);
                Debug.Log(report);
                // EDM owns the generated Gradle entries; do not copy another game's Gradle files.
                bool resolver = EditorApplication.ExecuteMenuItem("Assets/External Dependency Manager/Android Resolver/Force Resolve");
                File.AppendAllText("Logs/QuarryServiceChecks.txt", "\nAndroid resolver command dispatched: " + resolver);
                if (smoke)
                {
                    SessionState.SetString(Smoke+".oldPath",SessionState.GetString("Bulldozer.Mining.TestSavePath",""));
                    Directory.CreateDirectory("Library/QuarryServiceSmoke");
                    SessionState.SetString("Bulldozer.Mining.TestSavePath",Path.GetFullPath("Library/QuarryServiceSmoke/mining.json"));
                    SessionState.SetInt(Smoke+".phase",0);SessionState.SetBool(Smoke,true);
                    EditorApplication.isPlaying=true;
                }
            } catch (Exception error) {
                File.WriteAllText("Logs/QuarryServiceChecks.txt", "FAILED\n" + error);
                Debug.LogException(error);
            }
        }
        static void SmokeTick()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||!EditorApplication.isPlaying)return;
            if(nextSmoke==0){nextSmoke=EditorApplication.timeSinceStartup+15;return;}
            if(EditorApplication.timeSinceStartup<nextSmoke)return;
            try
            {
                var root=QuarryServicesRoot.Instance;
                if(!root||!root.InitializationRequested||!root.onlineServices.InitializationRequested||!root.purchasing.InitializationRequested)throw new Exception("Automatic service startup failed");
                var net=root.connectivity;
                int phase=SessionState.GetInt(Smoke+".phase",0);
                var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
                if(phase==0)
                {
                    File.WriteAllText("Logs/QuarryServiceSmoke.txt","PASS: services start automatically\n");
                    net.GetType().GetField("InterfaceAvailableForTests",flags).SetValue(net,(Func<bool>)(()=>false));net.CheckNow();
                    nextSmoke=EditorApplication.timeSinceStartup+4;
                }
                else if(phase==1)
                {
                    if(net.Status!=ConnectivityStatus.Offline||!QuarryServiceOverlay.ConnectionBlocked||Time.timeScale!=0)throw new Exception("Offline gate did not block and pause gameplay");
                    File.AppendAllText("Logs/QuarryServiceSmoke.txt","PASS: adapter loss pauses gameplay and shows gate\n");
                    ScreenCapture.CaptureScreenshot("Logs/QuarryOfflineSmoke.png");
                    net.GetType().GetField("InterfaceAvailableForTests",flags).SetValue(net,(Func<bool>)(()=>true));
                    net.GetType().GetField("ProbeSuccessForTests",flags).SetValue(net,(Func<bool>)(()=>true));net.CheckNow();
                    nextSmoke=EditorApplication.timeSinceStartup+15;
                }
                else if(phase==2)
                {
                    if(net.Status!=ConnectivityStatus.Online||QuarryServiceOverlay.ConnectionBlocked)throw new Exception("Internet restoration did not release gate");
                    File.AppendAllText("Logs/QuarryServiceSmoke.txt","PASS: verified internet automatically releases gate\n");
                    var ui=Bulldozer.UI.QuarryGameUI.Instance;
                    if(!ui)throw new Exception("Gameplay did not load after internet recovery");
                    ui.player.IntroInputBlocked=false;ui.player.GuidanceInputBlocked=false;ui.Open("shop");
                    nextSmoke=EditorApplication.timeSinceStartup+4;
                }
                else if(phase==3)
                {
                    ScreenCapture.CaptureScreenshot("Logs/QuarryStoreSmoke.png");
                    File.AppendAllText("Logs/QuarryServiceSmoke.txt","Store ready: "+root.purchasing.Ready+"; "+root.purchasing.Status+"\n"+root.onlineServices.AdDiagnostics+"\nIsolated Editor save used; no production transactions.\n");
                    nextSmoke=EditorApplication.timeSinceStartup+3;
                }
                else { FinishSmoke(); return; }
                SessionState.SetInt(Smoke+".phase",phase+1);
            }
            catch(Exception error){File.AppendAllText("Logs/QuarryServiceSmoke.txt","FAILED: "+error+"\n");FinishSmoke();}
        }
        static void FinishSmoke()
        {
            SessionState.SetString("Bulldozer.Mining.TestSavePath",SessionState.GetString(Smoke+".oldPath",""));
            SessionState.SetBool(Smoke,false);nextSmoke=0;EditorApplication.isPlaying=false;
        }
    }
}
