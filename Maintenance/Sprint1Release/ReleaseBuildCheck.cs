using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class ReleaseBuildCheck {
public static string Main(){
if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new Exception("Stable edit mode required");
string output=Path.GetFullPath("Builds/Sprint1/Quarry-Sprint1-Validation-v2.apk");
bool oldBundle=EditorUserBuildSettings.buildAppBundle;bool oldKey=PlayerSettings.Android.useCustomKeystore;
int oldCode=PlayerSettings.Android.bundleVersionCode;string oldVersion=PlayerSettings.bundleVersion;
try{
EditorUserBuildSettings.buildAppBundle=false;PlayerSettings.Android.useCustomKeystore=false;
PlayerSettings.Android.bundleVersionCode=2;PlayerSettings.bundleVersion="0.1.1";
var options=new BuildPlayerOptions{scenes=new[]{"Assets/0_ProjectData/0_Scenes/0_MainMenu.unity","Assets/0_ProjectData/0_Scenes/1_Gameplay.unity"},locationPathName=output,target=BuildTarget.Android,options=BuildOptions.Development|BuildOptions.CompressWithLz4};
var report=BuildPipeline.BuildPlayer(options);var s=report.summary;
File.WriteAllText("Maintenance/Sprint1Release/build-check.txt","Result="+s.result+"\nErrors="+s.totalErrors+"\nWarnings="+s.totalWarnings+"\nBytes="+s.totalSize+"\nDuration="+s.totalTime+"\nOutput="+s.outputPath+"\nINTERNAL DEVELOPMENT APK, debug signing, version code 2, version 0.1.1. NOT STORE READY. Project release identity/version settings restored afterward.");
if(s.result!=BuildResult.Succeeded)throw new Exception("Android build "+s.result);
return "Built "+output;
} catch(Exception ex){File.AppendAllText("Maintenance/Sprint1Release/build-check.txt","\n"+ex);throw;}
finally{EditorUserBuildSettings.buildAppBundle=oldBundle;PlayerSettings.Android.useCustomKeystore=oldKey;PlayerSettings.Android.bundleVersionCode=oldCode;PlayerSettings.bundleVersion=oldVersion;}
}}
