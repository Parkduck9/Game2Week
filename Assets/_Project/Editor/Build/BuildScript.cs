using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Game2Week.EditorTools.Build
{
    /// <summary>
    /// Windows 64비트 빌드. 메뉴 Tools ▸ Build ▸ Windows 또는 배치모드:
    /// Unity.exe -batchmode -quit -projectPath . -executeMethod Game2Week.EditorTools.Build.BuildScript.BuildWindows [-buildVersion 1.0.0]
    /// 결과: Builds/Windows/&lt;제품 이름&gt;.exe — 이 폴더를 Tools/package_msix.ps1이 MSIX로 포장한다.
    /// </summary>
    public static class BuildScript
    {
        public const string OutputDirectory = "Builds/Windows";

        [MenuItem("Tools/Build/Windows (x64)")]
        public static void BuildWindowsMenu() => Build(exitWhenDone: false);

        public static void BuildWindows() => Build(exitWhenDone: true);

        static void Build(bool exitWhenDone)
        {
            var version = ArgumentValue("-buildVersion");
            if (!string.IsNullOrEmpty(version)) PlayerSettings.bundleVersion = version;

            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0) throw new InvalidOperationException("빌드 목록에 씬이 없음");

            if (Directory.Exists(OutputDirectory)) Directory.Delete(OutputDirectory, true);
            var exe = Path.Combine(OutputDirectory, PlayerSettings.productName + ".exe");

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = exe,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            });

            var summary = report.summary;
            Debug.Log($"[Build] {summary.result} · {summary.totalSize / (1024 * 1024)}MB · {summary.totalTime.TotalSeconds:0}s · {exe} · v{PlayerSettings.bundleVersion}");
            if (exitWhenDone) EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        static string ArgumentValue(string name)
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, name);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
        }
    }
}
