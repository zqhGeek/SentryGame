using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace SentryGame.Editor
{
    public static class SentryGameAndroidBuilder
    {
        private const string OutputDirectory = @"F:\Caches\SentryGame\Builds\Android";
        private const string ApkName = "SentryGame.apk";

        public static void BuildApk()
        {
            // 重建正式场景列表，切换 Android 目标，并把 APK 输出到统一缓存目录供本机验证。
            SentryGameSceneBuilder.BuildAllScenes();
            Directory.CreateDirectory(OutputDirectory);
            var outputPath = Path.Combine(OutputDirectory, ApkName);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.buildAppBundle = false;

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"Android APK 构建失败：{report.summary.result}");
            }

            UnityEngine.Debug.Log($"Android APK 构建完成：{outputPath}，大小：{report.summary.totalSize} 字节");
        }
    }
}
