using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bulldozer.EditorTools
{
    public static class EditorToolValidation
    {
        [Serializable] private sealed class Results { public bool passed; public List<string> checks = new List<string>(); }

        [MenuItem(EditorToolUtility.MenuRoot + "Validation/Run Editor Tool Checks", false, 190)]
        private static void MenuRun()
        {
            if (!EditorToolUtility.RequireEditMode()) return;
            Debug.Log(Run());
        }

        // Isolated fixtures only: never calls DeleteAll, deletes a real prefs key, or opens a user scene.
        public static string Run()
        {
            if (!EditorToolUtility.CanEdit) throw new InvalidOperationException("Editor must be idle and outside Play Mode.");
            var results = new Results();
            string tempRoot = Path.Combine(Path.GetTempPath(), "BulldozerEditorTools-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempRoot);
            try
            {
                foreach (string invalid in new[] { "", ".", "..", "../other", "data/../../other", "/tmp/other", "C:\\other", "data//other", "EditorToolBackups/old" })
                    Check(!SaveFolderOperations.TryResolve(tempRoot, invalid, out _, out _), "Rejected unsafe save path: " + invalid, results);
                Check(SaveFolderOperations.TryResolve(tempRoot, "Saves/Slot1", out var path, out _), "Allowed nested save subfolder", results);
                Directory.CreateDirectory(path);
                File.WriteAllText(Path.Combine(path, "save.json"), "fixture-data");
                string archive = SaveFolderOperations.Archive(tempRoot, "Saves/Slot1");
                Check(!Directory.Exists(path) && File.ReadAllText(Path.Combine(archive, "save.json")) == "fixture-data", "Archive retains fixture bytes and resets source folder", results);
                Directory.CreateDirectory(path);
                bool refused = false;
                try { SaveFolderOperations.Restore(tempRoot, "Saves/Slot1", archive); } catch (IOException) { refused = true; }
                Check(refused && Directory.Exists(archive), "Restore refuses to overwrite existing saves", results);
                Directory.Delete(path);
                SaveFolderOperations.Restore(tempRoot, "Saves/Slot1", archive);
                Check(File.ReadAllText(Path.Combine(path, "save.json")) == "fixture-data" && !Directory.Exists(archive), "Archive restore round-trip preserves fixture bytes", results);
                refused = false;
                try { SaveFolderOperations.Restore(tempRoot, "Saves/Other", tempRoot); } catch (IOException) { refused = true; }
                Check(refused, "Restore rejects a path outside the dedicated backup folder", results);
            }
            finally { Directory.Delete(tempRoot, true); }

            var entries = SceneSwitcherWindow.BuildEntries(new[] {
                new EditorBuildSettingsScene("Assets/A/Main.unity", false),
                new EditorBuildSettingsScene("Assets/B/Main.unity", true),
                new EditorBuildSettingsScene("Assets/C/Main.unity", true)
            }, new[] { "Assets/A/Main.unity", "Assets/Extra.unity" });
            Check(entries.Count == 4 && entries[0].BuildIndex == -1 && entries[1].BuildIndex == 0 && entries[2].BuildIndex == 1,
                "Scene list deduplicates full paths and indexes only enabled scenes", results);

            var previous = SceneManager.GetActiveScene();
            var fixture = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            Mesh mesh = null;
            try
            {
                SceneManager.SetActiveScene(fixture);
                var root = new GameObject("EditorAuditFixture");
                var child = new GameObject("InactiveMesh");
                child.transform.SetParent(root.transform);
                mesh = new Mesh { vertices = new[] { Vector3.zero, Vector3.right, Vector3.up }, triangles = new[] { 0, 1, 2 } };
                child.AddComponent<MeshFilter>().sharedMesh = mesh;
                child.AddComponent<MeshRenderer>().sharedMaterials = new Material[] { null };
                child.SetActive(false);
                var report = MobileAudit.Scenes(new[] { root, child }, "fixture");
                Check(report.objects == 2 && report.renderers == 1 && report.activeRenderers == 0 && report.potentialTriangles == 1,
                    "Scene audit includes inactive objects and deduplicates overlapping selection roots", results);
                Check(report.findings.Exists(f => f.message.Contains("Missing material")), "Audit reports missing material slots", results);
                Check(HierarchyComponentIcons.IsHidden(root.transform), "Hierarchy omits Transform components", results);
                var cancelled = MobileAudit.Scenes(new[] { root }, "cancel fixture", _ => true);
                Check(cancelled.partial && cancelled.objects == 0, "Cancelled scene audit is explicitly partial", results);
                mesh.SetIndices(new[] { 0, 1 }, MeshTopology.Lines, 0);
                Check(MobileAudit.TriangleCount(mesh) == 0, "Line topology is not counted as triangles", results);
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(fixture, true);
                if (mesh) UnityEngine.Object.DestroyImmediate(mesh);
            }

            string fixtureFolder = "Assets/__BulldozerEditorToolTests_" + Guid.NewGuid().ToString("N");
            Texture2D texture = null;
            try
            {
                AssetDatabase.CreateFolder("Assets", Path.GetFileName(fixtureFolder));
                string assetPath = fixtureFolder + "/tiny.png";
                texture = new Texture2D(32, 32);
                File.WriteAllBytes(assetPath, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
                var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
                importer.maxTextureSize = 2048;
                importer.isReadable = false;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.SaveAndReimport();
                var tiny = MobileAudit.Textures(new[] { assetPath });
                Check(tiny.textures == 1 && tiny.findings.Count == 0, "Small source texture is not misreported as 2048px because of its cap", results);
                importer.isReadable = true;
                importer.SaveAndReimport();
                string before = EditorJsonUtility.ToJson(importer);
                var readable = MobileAudit.Textures(new[] { assetPath });
                Check(readable.findings.Exists(f => f.message.Contains("Read/Write")), "Texture audit detects Read/Write", results);
                Check(before == EditorJsonUtility.ToJson(importer), "Texture audit leaves importer settings unchanged", results);
            }
            finally
            {
                if (texture) UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.DeleteAsset(fixtureFolder);
            }
            results.passed = true;
            return JsonUtility.ToJson(results, true);
        }

        private static void Check(bool condition, string label, Results results)
        {
            if (!condition) throw new InvalidOperationException("Editor tool check failed: " + label);
            results.checks.Add(label);
        }
    }
}
