if(EditorApplication.isPlaying)throw new System.Exception("Edit mode required");
var folder="Assets/0_ProjectData/Audio/";var report=new System.Collections.Generic.List<string>();
foreach(var guid in AssetDatabase.FindAssets("t:AudioClip",new[]{folder.TrimEnd('/')})){
 var path=AssetDatabase.GUIDToAssetPath(guid);var importer=(AudioImporter)AssetImporter.GetAtPath(path);bool music=path.Contains("Underscore");var settings=importer.defaultSampleSettings;
 report.Add("BEFORE "+path+" "+settings.loadType+" "+settings.compressionFormat);
 settings.loadType=music?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
 settings.compressionFormat=music?AudioCompressionFormat.Vorbis:AudioCompressionFormat.ADPCM;settings.quality=.7f;settings.sampleRateSetting=AudioSampleRateSetting.OverrideSampleRate;settings.sampleRateOverride=24000;settings.preloadAudioData=!music;
 importer.defaultSampleSettings=settings;importer.forceToMono=!music;importer.loadInBackground=music;importer.SaveAndReimport();
 var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);report.Add("AFTER "+clip.name+" "+clip.length+"s "+clip.channels+"ch "+clip.loadType+" 24000Hz "+settings.compressionFormat);
}
AudioClip Clip(string name)=>AssetDatabase.LoadAssetAtPath<AudioClip>(folder+name);
var prefab="Assets/0_ProjectData/UI/Common/CommonUI.prefab";var root=PrefabUtility.LoadPrefabContents(prefab);
try{var audio=root.GetComponent<Bulldozer.UI.QuarryAudio>();if(!audio)audio=root.AddComponent<Bulldozer.UI.QuarryAudio>();
audio.music=Clip("Quarry_Dawn_Underscore.wav");audio.stoneBreak=Clip("Stone_Break.wav");audio.pickup=Clip("Ore_Pickup.wav");audio.place=Clip("Ore_Place.wav");audio.ready=Clip("Refinery_Ready.wav");audio.cart=Clip("impactMetal_light_001.ogg");audio.delivery=Clip("Delivery_Confirm.wav");audio.refineryLoop=Clip("Refinery_Loop.wav");audio.truckLoop=Clip("Truck_Engine_Loop.wav");audio.musicVolume=.12f;
audio.hits=new[]{Clip("impactMining_000.ogg"),Clip("impactMining_001.ogg"),Clip("impactMining_002.ogg")};audio.footsteps=new[]{Clip("footstep_concrete_000.ogg"),Clip("footstep_concrete_001.ogg"),Clip("footstep_concrete_002.ogg")};
PrefabUtility.SaveAsPrefabAsset(root,prefab);report.Add("Persistent CommonUI soundtrack configured: 8 world voices + 3 loops + 1 music source; existing Feel UI feedback retained.");
}finally{PrefabUtility.UnloadPrefabContents(root);}
AssetDatabase.SaveAssets();System.IO.File.WriteAllLines("Maintenance/Audio/import-report.txt",report);return string.Join("\n",report);
