if(EditorApplication.isPlaying)throw new System.Exception("Edit mode required");
var cart=UnityEngine.Object.FindFirstObjectByType<Bulldozer.Player.QuarryWheelbarrow>();
var paint=AssetDatabase.LoadAssetAtPath<Material>("Assets/0_ProjectData/Player/Production/Caddy Teal.mat");
paint.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/0_ProjectData/Player/Art/Wheelbarrow/BaseColor.png"));
paint.SetColor("_BaseColor",new Color(.88f,.88f,.88f));paint.SetFloat("_Metallic",.28f);paint.SetFloat("_Smoothness",.22f);paint.enableInstancing=true;EditorUtility.SetDirty(paint);
var rubber=AssetDatabase.LoadAssetAtPath<Material>("Assets/0_ProjectData/Player/Production/Caddy Rubber.mat");rubber.SetColor("_BaseColor",new Color(.055f,.052f,.048f));rubber.SetFloat("_Metallic",0);rubber.SetFloat("_Smoothness",.12f);EditorUtility.SetDirty(rubber);
var safety=AssetDatabase.LoadAssetAtPath<Material>("Assets/0_ProjectData/Player/Production/Caddy Safety.mat");safety.SetColor("_BaseColor",new Color(.42f,.30f,.10f));safety.SetFloat("_Smoothness",.14f);EditorUtility.SetDirty(safety);
var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/0_ProjectData/Player/Art/Wheelbarrow/BaseColor.png");importer.maxTextureSize=1024;importer.mipmapEnabled=true;importer.textureCompression=TextureImporterCompression.Compressed;importer.anisoLevel=2;importer.SaveAndReimport();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(cart.gameObject.scene);UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
return "Weathered atlas assigned; paint, rubber and grip finish saved. Texture capped at 1024 with mipmaps.";

