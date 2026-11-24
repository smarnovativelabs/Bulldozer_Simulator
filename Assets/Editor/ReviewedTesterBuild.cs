using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
internal static class ReviewedTesterBuild
{
    const string Root="Builds/Android/Reviewed-20261124/";
    static ReviewedTesterBuild(){EditorApplication.update+=Tick;}
    static void Tick()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||BuildPipeline.isBuildingPlayer)return;
        if(File.Exists("Library/ReviewedSaveChecks.request"))
        {
            File.Delete("Library/ReviewedSaveChecks.request");
            try{SaveCompatibilityChecks.Main(); File.Copy("Maintenance/TesterReadiness/save-checks.txt",Root+"SaveChecks.txt",true);}
            catch(Exception e){File.WriteAllText(Root+"SaveChecks.txt",e.ToString());}
        }
        if(!File.Exists("Library/ReviewedTesterBuild.request"))return;
        File.Delete("Library/ReviewedTesterBuild.request");
        bool oldBundle=EditorUserBuildSettings.buildAppBundle,oldExport=EditorUserBuildSettings.exportAsGoogleAndroidProject;
        try
        {
            if(!File.ReadAllText(Root+"FullGameplay.txt").Contains("COMPLETE")||!File.ReadAllText(Root+"SaveChecks.txt").Contains("COMPLETE"))throw new Exception("Review checks must pass before build");
            EditorUserBuildSettings.buildAppBundle=false;EditorUserBuildSettings.exportAsGoogleAndroidProject=false;
            File.WriteAllText(Root+"BuildStatus.txt","Building");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=Root+"BulldozerCrush-DigAndMine3D-Tester.apk",target=BuildTarget.Android,options=BuildOptions.None});
            var s=report.summary;
            File.WriteAllText(Root+"BuildReport.txt",s.result+"\nErrors="+s.totalErrors+" Warnings="+s.totalWarnings+" Bytes="+s.totalSize+" Duration="+s.totalTime+"\n"+string.Join("\n",report.steps.SelectMany(step=>step.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content)));
            File.WriteAllText(Root+"BuildStatus.txt",s.result.ToString());
        }
        catch(Exception e){File.WriteAllText(Root+"BuildStatus.txt","Failed\n"+e);}
        finally{EditorUserBuildSettings.buildAppBundle=oldBundle;EditorUserBuildSettings.exportAsGoogleAndroidProject=oldExport;}
    }
}
