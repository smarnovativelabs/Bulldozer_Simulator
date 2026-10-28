if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new System.Exception("Build requires stable edit mode");
System.IO.Directory.CreateDirectory("Builds/Sprint1");
 bool bundle=EditorUserBuildSettings.buildAppBundle;bool customKey=PlayerSettings.Android.useCustomKeystore;
 try {
  EditorUserBuildSettings.buildAppBundle=false;PlayerSettings.Android.useCustomKeystore=false;
  var options=new BuildPlayerOptions {scenes=new[]{"Assets/0_ProjectData/0_Scenes/1_Gameplay.unity"},locationPathName="Builds/Sprint1/Quarry-Sprint1-Test.apk",target=BuildTarget.Android,options=BuildOptions.Development};
  var report=BuildPipeline.BuildPlayer(options);var summary=report.summary;
  System.IO.File.WriteAllText("Maintenance/Miner/sprint1-build-result.txt","Result="+summary.result+"\nErrors="+summary.totalErrors+"\nWarnings="+summary.totalWarnings+"\nBytes="+summary.totalSize+"\nDuration="+summary.totalTime+"\nOutput="+summary.outputPath+"\nDevelopment APK, debug signing, gameplay scene only. Not a store release.");
 }catch(System.Exception ex){System.IO.File.WriteAllText("Maintenance/Miner/sprint1-build-result.txt","BUILD FAILED\n"+ex);throw;}
 finally{EditorUserBuildSettings.buildAppBundle=bundle;PlayerSettings.Android.useCustomKeystore=customKey;}
return "Build finished; see sprint1-build-result.txt";
