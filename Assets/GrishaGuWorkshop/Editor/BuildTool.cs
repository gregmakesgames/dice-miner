#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GrishaGuWorkshop
{
    public static class BuildTool
    {
        public static void BuildFromCommandLine()
        {
            var outputPath = GetArgumentValue("-buildOutput");
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = Path.Combine("Build", "WebGL");
            }

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("No enabled scenes found in Build Settings.");
            }

            Directory.CreateDirectory(outputPath);

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            Debug.Log($"Starting WebGL build to: {outputPath}");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new Exception(
                    $"WebGL build failed. Result: {report.summary.result}, " +
                    $"Errors: {report.summary.totalErrors}, Warnings: {report.summary.totalWarnings}");
            }

            Debug.Log($"WebGL build completed successfully: {outputPath}");
        }

        private static string GetArgumentValue(string argumentName)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], argumentName, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }

            return null;
        }
    }
}
#endif
