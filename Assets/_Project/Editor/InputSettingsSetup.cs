using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game2Week.EditorTools
{
    /// <summary>
    /// 에디터가 켜질 때 Input System 설정을 맞춘다:
    /// 플레이 중 키보드 입력을 Game 창 포커스와 상관없이 게임으로 보낸다.
    /// (기본값은 Game 창을 한 번 클릭하기 전까지 키보드를 무시해서 "처음 Play 누르면 키가 안 먹는" 문제가 생김)
    /// 빌드된 게임에는 영향 없음 — 에디터 플레이 전용 설정.
    /// </summary>
    [InitializeOnLoad]
    public static class InputSettingsSetup
    {
        /// <summary>배치모드용: Unity.exe -batchmode -executeMethod Game2Week.EditorTools.InputSettingsSetup.ApplyAndExit</summary>
        public static void ApplyAndExit()
        {
            Ensure();
            Debug.Log($"[Game2Week] editorInputBehaviorInPlayMode = {InputSystem.settings.editorInputBehaviorInPlayMode}");
            EditorApplication.Exit(InputSystem.settings.editorInputBehaviorInPlayMode == Wanted ? 0 : 1);
        }

        const string SettingsPath = "Assets/_Project/Input/InputSystemSettings.asset";
        const InputSettings.EditorInputBehaviorInPlayMode Wanted = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;

        static InputSettingsSetup() => EditorApplication.delayCall += Ensure;

        static void Ensure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            var settings = AssetDatabase.LoadAssetAtPath<InputSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<InputSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }

            if (settings.editorInputBehaviorInPlayMode != Wanted)
            {
                settings.editorInputBehaviorInPlayMode = Wanted;
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }

            if (InputSystem.settings != settings)
            {
                EditorBuildSettings.AddConfigObject("com.unity.input.settings", settings, true);
                InputSystem.settings = settings;
                Debug.Log("[Game2Week] Input System 설정: 플레이 중 키보드 입력이 항상 Game 창으로 가도록 설정함");
            }
        }
    }
}
