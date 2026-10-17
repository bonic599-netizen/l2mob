using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Linq;

public static class MobileBuildScript
{
    public static void BuildIOS()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            scenes = AssetDatabase.FindAssets("t:Scene")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .ToArray();
        }

        Debug.Log($"[MobileBuildScript] Found {scenes.Length} scenes to build: {string.Join(", ", scenes)}");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "build/iOS",
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[MobileBuildScript] iOS Build succeeded: {summary.totalSize} bytes");
        }
        else
        {
            Debug.LogError($"[MobileBuildScript] iOS Build failed with result: {summary.result}");
            EditorApplication.Exit(1);
        }
    }
}
