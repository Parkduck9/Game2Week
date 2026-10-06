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
            var product=AssetDatabase.LoadAssetAtPath<Game2Week.Core.ProductInfo>("Assets/_Project/Data/ProductInfo.asset");
            if(product)
            {
                // 회사 이름을 바꾸면 기존 세이브 경로가 달라지므로 이름 확정 전에는 유지한다.
                if(product.companyName!="DefaultCompany"||product.productName!="Game2Week")throw new InvalidOperationException("회사/제품 이름 변경은 세이브 이전 계획을 확인한 뒤 적용하세요.");
                PlayerSettings.companyName=product.companyName;PlayerSettings.productName=product.productName;PlayerSettings.bundleVersion=product.version;
            }
            var version = ArgumentValue("-buildVersion");
            if (!string.IsNullOrEmpty(version)) PlayerSettings.bundleVersion = version;

            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0) throw new InvalidOperationException("빌드 목록에 씬이 없음");

            var outputPath=Path.GetFullPath(Path.Combine(Application.dataPath,"..",OutputDirectory));
            var projectPath=Path.GetFullPath(Path.Combine(Application.dataPath,".."))+Path.DirectorySeparatorChar;
            if(!outputPath.StartsWith(projectPath,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("빌드 출력은 프로젝트 폴더 안이어야 합니다.");
            if (Directory.Exists(outputPath)) Directory.Delete(outputPath, true);
            var exe = Path.Combine(outputPath, PlayerSettings.productName + ".exe");

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = exe,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            });

            var summary = report.summary;
            if(product&&summary.result==BuildResult.Succeeded)
            {
                var exported=UnityEngine.Object.Instantiate(product);exported.version=PlayerSettings.bundleVersion;
                File.WriteAllText(Path.Combine(projectPath,"Builds/product_info.json"),JsonUtility.ToJson(exported,true),new System.Text.UTF8Encoding(false));
                UnityEngine.Object.DestroyImmediate(exported);
            }
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
