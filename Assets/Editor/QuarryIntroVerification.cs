using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bulldozer.Player;

// Explicit, isolated editor smoke test; never runs without its request marker.
[InitializeOnLoad]
internal static class QuarryIntroVerification
{
    const string Key="Bulldozer.IntroVerification";
    const string Request="Library/QuarryIntroVerification.request";
    const string Report="Logs/QuarryIntroVerification.txt";
    static int phase,frame=-1;
    static double deadline,phaseAt;
    static double editReadyAt;
    static bool captured;
    static bool connectivityStubbed;
    static QuarryCameraGuidance cue;
    static Vector3 pausedPosition;
    static int startupProbes;
    static AsyncOperation sceneLoad;
    static float storeHeight;
    static void SetNetwork(Bulldozer.Services.InternetConnectivity net,bool available,Func<bool> probe)
    {
        net.enabled=false;
        var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
        net.GetType().GetField("InterfaceAvailableForTests",flags).SetValue(net,(Func<bool>)(()=>available));
        net.GetType().GetField("ProbeSuccessForTests",flags).SetValue(net,probe);
        net.enabled=true;
    }
    static QuarryIntroVerification(){EditorApplication.update+=Tick;}
    static void Check(bool condition,string message){if(!condition)throw new Exception(message);File.AppendAllText(Report,"PASS: "+message+"\n");}
    static void Tick()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||BuildPipeline.isBuildingPlayer)return;
        if(!EditorApplication.isPlayingOrWillChangePlaymode&&!Application.isPlaying)
        {
            if(editReadyAt==0){editReadyAt=EditorApplication.timeSinceStartup+2;return;}
            if(EditorApplication.timeSinceStartup<editReadyAt)return;
            if(SessionState.GetBool(Key,false))
            {
                SessionState.SetString("Bulldozer.Mining.TestSavePath",SessionState.GetString(Key+".oldSave",""));
                SessionState.SetBool(Key,false);
            }
        }
        else editReadyAt=0;
        if(!SessionState.GetBool(Key,false))
        {
            if(!File.Exists(Request)||EditorApplication.isPlayingOrWillChangePlaymode)return;
            File.Delete(Request);
            if(SceneManager.GetActiveScene().isDirty){File.WriteAllText(Report,"BLOCKED: save the open scene before testing.");return;}
            Directory.CreateDirectory("Logs");File.WriteAllText(Report,"First-time intro verification (isolated editor save)\n");
            SessionState.SetString(Key+".oldSave",SessionState.GetString("Bulldozer.Mining.TestSavePath",""));
            SessionState.SetString(Key+".oldScene",SceneManager.GetActiveScene().path);
            SessionState.SetString("Bulldozer.Mining.TestSavePath",Path.GetFullPath("Library/IntroVerification/"+Guid.NewGuid().ToString("N")+"/progress.json"));
            SessionState.SetBool(Key,true);
            if(SceneManager.GetActiveScene().path!="Assets/0_ProjectData/0_Scenes/0_MainMenu.unity")
                EditorSceneManager.OpenScene("Assets/0_ProjectData/0_Scenes/0_MainMenu.unity");
            EditorApplication.isPlaying=true;
            return;
        }
        if(!EditorApplication.isPlaying)return;
        Application.runInBackground=true;
        if(frame==Time.frameCount)return;frame=Time.frameCount;
        if(deadline==0)deadline=EditorApplication.timeSinceStartup+180;
        try
        {
            var services=Bulldozer.Services.QuarryServicesRoot.Instance;
            if(!connectivityStubbed&&services&&services.connectivity)
            {
                var net=services.connectivity;
                Check(!Bulldozer.Services.QuarryServiceOverlay.ConnectionPanelRequired,"checking connection does not show offline panel");
                SetNetwork(net,true,()=>{
                    startupProbes++;
                    if(startupProbes==3)Check(!net.ConnectionProblem&&!Bulldozer.Services.QuarryServiceOverlay.ConnectionPanelRequired,"first failed probe pair retries without offline flash");
                    return startupProbes>2;
                });
                connectivityStubbed=true;
                // Avoid simulated ad windows interrupting a camera-only test.
                if(services.onlineServices)services.onlineServices.enabled=false;
                File.AppendAllText(Report,"Connectivity stubbed for camera-only checks; native services are outside this test.\n");
            }
            if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Timed out at phase "+phase);
            if(sceneLoad!=null){if(!sceneLoad.isDone)return;sceneLoad=null;cue=null;phaseAt=EditorApplication.timeSinceStartup;}
            if(!cue)cue=UnityEngine.Object.FindFirstObjectByType<QuarryCameraGuidance>();
            if(!cue)return;
            var player=cue.guide.player;
            var camera=player.followCamera;
            if(phase==0&&cue.Playing)
            {
                Check(cue.ActiveCue==QuarryCameraGuidance.PlayerIntro,"fresh save plays quarry intro");
                Check(player.IntroInputBlocked&&player.GuidanceInputBlocked,"controls locked during intro");
                Check(!Bulldozer.UI.CommonUI.Instance.BlocksGameplayInput,"intro starts after loading cover releases");
                Check(Camera.allCamerasCount==1,"one rendering camera");
                Check(cue.introRevealSeconds==3f&&cue.introReturnSeconds==1.3f,"faster intro timing applied");
                ScreenCapture.CaptureScreenshot("Logs/QuarryIntro-Overview.png");
                phase=1;phaseAt=EditorApplication.timeSinceStartup;
            }
            else if(phase==1&&EditorApplication.timeSinceStartup-phaseAt>.8)
            {
                pausedPosition=camera.transform.position;Time.timeScale=0;phase=2;phaseAt=EditorApplication.timeSinceStartup;
            }
            else if(phase==2&&EditorApplication.timeSinceStartup-phaseAt>.6)
            {
                Check(cue.Playing&&Vector3.Distance(pausedPosition,camera.transform.position)<.01f,"temporary pause holds intro camera");
                Time.timeScale=1;phase=3;phaseAt=EditorApplication.timeSinceStartup;
            }
            else if(phase==3)
            {
                if(!captured&&EditorApplication.timeSinceStartup-phaseAt>1.2)
                {ScreenCapture.CaptureScreenshot("Logs/QuarryIntro-Reveal.png");captured=true;}
                if(cue.Playing)return;
                Check(!player.IntroInputBlocked&&!player.GuidanceInputBlocked,"intro releases controls");
                camera.GetFollowPose(out var position,out var rotation);
                Check(Vector3.Distance(position,camera.transform.position)<.02f&&Quaternion.Angle(rotation,camera.transform.rotation)<.1f,"handoff matches follow camera without snap");
                Check((cue.guide.equipment.Snapshot.guidanceSeenMask&QuarryCameraGuidance.PlayerIntro)!=0,"intro seen flag saved");
                ScreenCapture.CaptureScreenshot("Logs/QuarryIntro-Gameplay.png");phase=4;phaseAt=EditorApplication.timeSinceStartup;
            }
            else if(phase==4&&EditorApplication.timeSinceStartup-phaseAt>.5)
            {
                sceneLoad=SceneManager.LoadSceneAsync("Assets/0_ProjectData/0_Scenes/1_Gameplay.unity");cue=null;phase=5;phaseAt=EditorApplication.timeSinceStartup;
            }
            else if(phase==5&&EditorApplication.timeSinceStartup-phaseAt>2)
            {
                Check(cue.ShotsStarted==0&&!cue.Playing&&!player.IntroInputBlocked,"reload does not replay intro");
                var state=cue.guide.equipment.Snapshot;state.guidanceSeenMask&=~QuarryCameraGuidance.PlayerIntro;
                Check(cue.guide.equipment.Commit(state),"prepare isolated save for skip test");
                sceneLoad=SceneManager.LoadSceneAsync("Assets/0_ProjectData/0_Scenes/1_Gameplay.unity");cue=null;phase=6;
            }
            else if(phase==6&&cue.Playing)
            {phase=8;phaseAt=EditorApplication.timeSinceStartup;}
            else if(phase==8&&EditorApplication.timeSinceStartup-phaseAt>.75)
            {
                Check(cue.Playing,"skip requested during active reveal");
                cue.Skip();phase=7;phaseAt=Time.unscaledTime;
            }
            else if(phase==7&&!cue.Playing)
            {
                File.AppendAllText(Report,"Skip blend elapsed (game clock): "+(Time.unscaledTime-phaseAt)+" seconds\n");
                Check(Time.unscaledTime-phaseAt>=cue.introReturnSeconds-.15,"skip uses smooth return blend");
                Check(!player.IntroInputBlocked&&!player.GuidanceInputBlocked,"skip releases controls");
                phase=9;phaseAt=Time.unscaledTime;
            }
            else if(phase==9&&Time.unscaledTime-phaseAt>.5f)
            {
                Check(player.Grounded,"player grounded before opening store");
                Check(camera.fixedAngle,"fixed-angle camera enabled");
                player.touchControls.AddLook(new Vector2(600,300));
                storeHeight=player.transform.position.y;
                Bulldozer.UI.QuarryGameUI.Instance.Open("shop");
                Check(Bulldozer.UI.QuarryGameUI.BlocksInput,"store opens and blocks controls");
                player.Jump();phase=10;phaseAt=Time.unscaledTime;
            }
            else if(phase==10&&Time.unscaledTime-phaseAt>1f)
            {
                Check(player.Grounded,"paused store preserves grounded state");
                Check(Mathf.Abs(camera.Yaw-camera.startYaw)<.01f,"look input cannot rotate fixed camera");
                Bulldozer.UI.QuarryGameUI.Instance.Close();phase=11;phaseAt=Time.unscaledTime;
            }
            else if(phase==11&&Time.unscaledTime-phaseAt>.6f)
            {
                Check(!Bulldozer.UI.QuarryGameUI.BlocksInput&&!player.InputBlocked,"store close releases controls after input guard");
                Check(player.Grounded&&player.VerticalSpeed<=0&&Mathf.Abs(player.transform.position.y-storeHeight)<.03f,"store back does not jump or enter airborne physics");
                SetNetwork(services.connectivity,false,()=>false);phase=12;phaseAt=Time.unscaledTime;
            }
            else if(phase==12&&Time.unscaledTime-phaseAt>3f)
            {
                Check(Bulldozer.Services.QuarryServiceOverlay.ConnectionPanelRequired&&Time.timeScale==0,"confirmed offline state shows panel and pauses gameplay");
                SetNetwork(services.connectivity,true,()=>true);phase=13;phaseAt=Time.unscaledTime;
            }
            else if(phase==13&&Time.unscaledTime-phaseAt>.7f)
            {
                Check(!Bulldozer.Services.QuarryServiceOverlay.ConnectionBlocked&&Time.timeScale>0,"connection recovery releases gate");
                SetNetwork(services.connectivity,true,()=>false);phase=14;phaseAt=Time.unscaledTime;
            }
            else if(phase==14&&Time.unscaledTime-phaseAt>2f)
            {
                Check(services.connectivity.ConnectionProblem&&Bulldozer.Services.QuarryServiceOverlay.ConnectionPanelRequired,"repeated endpoint failures show retry panel with adapter connected");
                SetNetwork(services.connectivity,true,()=>true);phase=15;phaseAt=Time.unscaledTime;
            }
            else if(phase==15&&Time.unscaledTime-phaseAt>.7f)
            {
                Check(!Bulldozer.Services.QuarryServiceOverlay.ConnectionPanelRequired&&Time.timeScale>0,"successful retry hides panel and resumes gameplay");
                File.AppendAllText(Report,"COMPLETE\n");Finish();
            }
        }
        catch(Exception e){File.AppendAllText(Report,"FAILED: "+e+"\n");Finish();}
    }
    static void Finish()
    {
        Time.timeScale=1;
        SessionState.SetString("Bulldozer.Mining.TestSavePath",SessionState.GetString(Key+".oldSave",""));
        SessionState.SetBool(Key,false);deadline=0;phase=0;captured=false;connectivityStubbed=false;startupProbes=0;cue=null;
        EditorApplication.isPlaying=false;
    }
}
