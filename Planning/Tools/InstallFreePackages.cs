using System;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

public static class InstallFreePackages
{
    public static string Main()
    {
        if (!Application.dataPath.Replace('\\', '/').EndsWith("/Bulldozer_Simulator/Assets", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("This operation is restricted to Bulldozer_Simulator.");
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            throw new InvalidOperationException("Wait for edit mode and compilation to finish.");

        string[] packages = {
            "com.unity.cinemachine",
            "com.unity.animation.rigging",
            "com.unity.formats.fbx",
            "com.unity.mobile.android-logcat",
            "com.unity.memoryprofiler"
        };
        var request = Client.AddAndRemove(packages, new string[0]);
        EditorApplication.CallbackFunction poll = null;
        poll = () => {
            if (!request.IsCompleted) return;
            EditorApplication.update -= poll;
            string result = request.Status == StatusCode.Success
                ? "Success: requested free packages resolved through Unity Package Manager."
                : "Failed: " + request.Error?.message;
            Directory.CreateDirectory("Maintenance/2026-10-23-plugin-audit");
            File.WriteAllText("Maintenance/2026-10-23-plugin-audit/package-install-result.txt", result);
            Debug.Log(result);
        };
        EditorApplication.update += poll;
        return "Started one asynchronous UPM AddAndRemove request for five packages. Verify resolved packages and compilation after reload.";
    }
}
