using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Bulldozer.EditorTools
{
    [Serializable]
    internal sealed class AuditFinding
    {
        public string severity, message, location;
        [NonSerialized] public UnityEngine.Object target;
    }

    [Serializable]
    internal sealed class AuditReport
    {
        public string scope, capturedUtc, summary;
        public bool partial;
        public int objects, renderers, activeRenderers, missingScripts, textures;
        public long potentialTriangles;
        public List<AuditFinding> findings = new List<AuditFinding>();

        internal void Add(string severity, string message, UnityEngine.Object target)
        {
            var go = target as GameObject;
            if (target is Component component) go = component.gameObject;
            findings.Add(new AuditFinding { severity = severity, message = message, target = target,
                location = go ? EditorToolUtility.HierarchyPath(go) : target ? AssetDatabase.GetAssetPath(target) : "" });
        }
    }

    internal static class MobileAudit
    {
        internal const int MaxObjects = 50000;
        internal const int MaxTextures = 5000;

        internal static long TriangleCount(Mesh mesh)
        {
            if (!mesh) return 0;
            long count = 0;
            for (int i = 0; i < mesh.subMeshCount; i++)
                if (mesh.GetTopology(i) == MeshTopology.Triangles) count += (long)mesh.GetIndexCount(i) / 3;
            return count;
        }

        internal static AuditReport Scenes(IEnumerable<GameObject> roots, string scope, Func<int, bool> cancel = null)
        {
            var report = NewReport(scope);
            var visited = new HashSet<int>();
            var materials = new HashSet<Material>();
            var stack = new Stack<Transform>();
            var components = new List<Component>(16);
            var materialBuffer = new List<Material>(8);
            int listeners = 0, cameras = 0, eventSystems = 0, shadowLights = 0, dynamicBodies = 0;
            long particleCapacity = 0;
            foreach (var root in roots) if (root && root.scene.IsValid()) stack.Push(root.transform);
            while (stack.Count > 0)
            {
                var transform = stack.Pop();
                if (!transform || !visited.Add(transform.GetInstanceID())) continue;
                if (report.objects >= MaxObjects || (report.objects % 128 == 0 && cancel != null && cancel(report.objects))) { report.partial = true; break; }
                var go = transform.gameObject;
                report.objects++;
                for (int i = 0; i < transform.childCount; i++) stack.Push(transform.GetChild(i));
                components.Clear();
                go.GetComponents(components);
                int missing = 0;
                foreach (var component in components)
                {
                    if (!component) { missing++; continue; }
                    if (component is AudioListener listener && listener.enabled && go.activeInHierarchy) listeners++;
                    if (component is Camera camera && camera.enabled && go.activeInHierarchy) cameras++;
                    if (component.GetType().FullName == "UnityEngine.EventSystems.EventSystem" && component is Behaviour es && es.isActiveAndEnabled) eventSystems++;
                    if (component is Light light && light.isActiveAndEnabled && light.shadows != LightShadows.None && light.lightmapBakeType != LightmapBakeType.Baked) shadowLights++;
                    if (component is Rigidbody body && !body.isKinematic && go.activeInHierarchy) dynamicBodies++;
                    if (component is MeshCollider collider && collider.enabled && collider.sharedMesh)
                    {
                        if (TriangleCount(collider.sharedMesh) > 5000) report.Add("Review", "Detailed MeshCollider (>5k triangles). Consider simpler collision geometry.", collider);
                        if (!collider.convex && collider.attachedRigidbody && !collider.attachedRigidbody.isKinematic)
                            report.Add("Error", "Non-convex MeshCollider attached to a dynamic Rigidbody.", collider);
                    }
                    if (component is ParticleSystem particles && go.activeInHierarchy)
                    {
                        particleCapacity += particles.main.maxParticles;
                        if (particles.main.maxParticles > 150) report.Add("Review", "Particle maxParticles exceeds the initial 150-particle review threshold. This is capacity, not observed live count.", particles);
                    }
                    if (!(component is Renderer renderer)) continue;
                    report.renderers++;
                    if (renderer.enabled && go.activeInHierarchy) report.activeRenderers++;
                    materialBuffer.Clear();
                    renderer.GetSharedMaterials(materialBuffer);
                    if (materialBuffer.Count > 2) report.Add("Review", materialBuffer.Count + " material slots; review batching and submeshes.", renderer);
                    foreach (var material in materialBuffer)
                    {
                        if (!material) { report.Add("Warning", "Missing material slot.", renderer); continue; }
                        if (!materials.Add(material)) continue;
                        if (!material.shader || material.shader.name == "Hidden/InternalErrorShader") report.Add("Error", "Missing/error shader.", material);
                        else if (!material.shader.isSupported) report.Add("Warning", "Shader unsupported in the current Editor graphics environment.", material);
                        if (material.renderQueue >= (int)RenderQueue.Transparent) report.Add("Review", "Transparent material; review screen coverage and overdraw on the phone.", material);
                    }
                    Mesh mesh = null;
                    if (renderer is SkinnedMeshRenderer skin)
                    {
                        mesh = skin.sharedMesh;
                        if (skin.bones.Length > 64) report.Add("Review", "Skinned renderer uses more than 64 bones.", skin);
                        if (skin.updateWhenOffscreen) report.Add("Review", "Skinned renderer updates offscreen; verify this is necessary.", skin);
                    }
                    else if (renderer is MeshRenderer && go.TryGetComponent<MeshFilter>(out var filter)) mesh = filter.sharedMesh;
                    long triangles = TriangleCount(mesh);
                    report.potentialTriangles += triangles;
                    if (triangles > 6000) report.Add("Review", triangles + " mesh triangles; compare to the object's visual role and LOD needs.", renderer);
                }
                if (missing > 0) { report.missingScripts += missing; report.Add("Error", missing + " missing script component(s).", go); }
            }
            if (listeners > 1) report.Add("Warning", listeners + " active AudioListeners in scanned scope.", null);
            if (eventSystems > 1) report.Add("Warning", eventSystems + " active EventSystems in scanned scope; check for duplicate input routing.", null);
            if (cameras > 1) report.Add("Review", cameras + " active cameras. Intentional URP camera stacks are valid, but have a cost.", null);
            if (shadowLights > 1) report.Add("Review", shadowLights + " realtime/mixed shadow-casting lights; review low-tier shadow cost.", null);
            if (dynamicBodies > 24) report.Add("Review", dynamicBodies + " active non-kinematic rigidbodies; compare against the intended debris/physics budget.", null);
            if (report.potentialTriangles > 150000) report.Add("Review", "Potential mesh triangles exceed 150k. This includes inactive objects, every LOD and repeated mesh instances; it is NOT a visible-frame measurement.", null);
            report.summary = (report.partial ? "PARTIAL scan. " : "") + report.objects + " objects; " + report.renderers + " renderers (" + report.activeRenderers + " enabled/active); " +
                report.potentialTriangles + " potential mesh triangles; " + materials.Count + " unique shared materials; " + report.missingScripts + " missing scripts.\n" +
                "Active-object particle capacity: " + particleCapacity + ". Counts include all inspected LODs and inactive meshes. Renderer count is not draw calls. No phone FPS or memory claim.";
            return report;
        }

        internal static int PlatformCap(TextureImporter importer, string platform)
        {
            var settings = importer.GetPlatformTextureSettings(platform);
            return settings.overridden ? settings.maxTextureSize : importer.maxTextureSize;
        }

        internal static AuditReport Textures(IEnumerable<string> inputPaths, Func<int, bool> cancel = null)
        {
            var report = NewReport("Selected Project assets/folders and their dependencies");
            var paths = new HashSet<string>(StringComparer.Ordinal);
            foreach (string path in inputPaths)
            {
                if (AssetDatabase.IsValidFolder(path))
                {
                    foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { path })) paths.Add(AssetDatabase.GUIDToAssetPath(guid));
                }
                else if (!string.IsNullOrEmpty(path))
                {
                    foreach (var dependency in AssetDatabase.GetDependencies(path, true)) paths.Add(dependency);
                }
            }
            var sorted = new List<string>(paths);
            sorted.Sort(StringComparer.Ordinal);
            foreach (string path in sorted)
            {
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;
                if (report.textures >= MaxTextures || (cancel != null && cancel(report.textures))) { report.partial = true; break; }
                report.textures++;
                importer.GetSourceTextureWidthAndHeight(out int width, out int height);
                int sourceEdge = Mathf.Max(width, height);
                int android = PlatformCap(importer, "Android"), ios = PlatformCap(importer, "iPhone");
                var reasons = new List<string>();
                if (Mathf.Min(sourceEdge, android) > 1024 || Mathf.Min(sourceEdge, ios) > 1024) reasons.Add("source/platform cap permits >1024px");
                if (importer.isReadable) reasons.Add("Read/Write enabled");
                if (importer.textureCompression == TextureImporterCompression.Uncompressed &&
                    (!importer.GetPlatformTextureSettings("Android").overridden || !importer.GetPlatformTextureSettings("iPhone").overridden))
                    reasons.Add("uncompressed default inherited by at least one mobile target");
                if (reasons.Count == 0) continue;
                report.findings.Add(new AuditFinding { severity = "Review", location = path,
                    message = string.Join("; ", reasons) + ". Source " + width + "×" + height + "; Android cap " + android + "; iOS cap " + ios + ".",
                    target = AssetDatabase.LoadMainAssetAtPath(path) });
            }
            report.summary = (report.partial ? "PARTIAL scan. " : "") + report.textures + " texture importers checked. " + report.findings.Count + " need review.\n" +
                "1024px is a starting review threshold, not an automatic fix. Source size/caps are not exact build resolution or VRAM. Atlas packing, format overrides, mipmaps and target support need separate review. Nothing was reimported or changed.";
            return report;
        }

        private static AuditReport NewReport(string scope) => new AuditReport { scope = scope, capturedUtc = DateTime.UtcNow.ToString("O") };
    }
}
