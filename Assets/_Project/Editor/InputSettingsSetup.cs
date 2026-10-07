using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game2Week.EditorTools
{
    /// <summary>
    /// 에디터가 켜질 때 Input System 설정을 맞춘다: 플레이 중 키보드 입력을 Game 창 포커스와 상관없이 게임으로 보낸다.
    /// (기본값은 Game 창을 한 번 클릭하기 전까지 키보드를 무시해서 "처음 Play 누르면 키가 안 먹는" 문제가 생김)
    ///
    /// 2026-10-07 수정 — "Play 누를 때마다 키보드가 됐다 안 됐다": Input System 1.20부터
    /// AllDeviceInputAlwaysGoesToGameView만으로는 부족하다. Game 창에 포커스가 없고 runInBackground가 꺼져 있으면
    /// 입력을 버리고(InputManager: !gameHasFocus → 이벤트 버림), 포커스를 잃으면 키보드를 끈다(ResetAndDisableNonBackgroundDevices).
    /// Play 직전에 어느 창을 눌렀는지에 따라 결과가 달라졌다. → 에디터 플레이 중에만 backgroundBehavior = IgnoreFocus.
    /// 에셋 값(빌드용)은 ResetAndDisableNonBackgroundDevices 그대로 — 빌드된 게임에는 영향 없음.
    /// </summary>
    [InitializeOnLoad]
    public static class InputSettingsSetup
    {
        /// <summary>배치모드용: Unity.exe -batchmode -executeMethod Game2Week.EditorTools.InputSettingsSetup.ApplyAndExit</summary>
        public static void ApplyAndExit()
        {
            Ensure();
            Debug.Log($"[Game2Week] editorInputBehaviorInPlayMode = {InputSystem.settings.editorInputBehaviorInPlayMode}, backgroundBehavior = {InputSystem.settings.backgroundBehavior}");
            EditorApplication.Exit(InputSystem.settings.editorInputBehaviorInPlayMode == Wanted && InputSystem.settings.backgroundBehavior == BuildBackground ? 0 : 1);
        }

        const string SettingsPath = "Assets/_Project/Input/InputSystemSettings.asset";
        const InputSettings.EditorInputBehaviorInPlayMode Wanted = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        /// <summary>빌드에서 쓰는 값 (에셋에 저장되는 값)</summary>
        public const InputSettings.BackgroundBehavior BuildBackground = InputSettings.BackgroundBehavior.ResetAndDisableNonBackgroundDevices;
        /// <summary>에디터 플레이 중에만 쓰는 값 — Game 창 포커스와 상관없이 입력 처리</summary>
        public const InputSettings.BackgroundBehavior EditorPlayBackground = InputSettings.BackgroundBehavior.IgnoreFocus;

        static InputSettingsSetup()
        {
            EditorApplication.delayCall += Ensure;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            // 도메인 리로드 직후 이미 플레이 중이면(Play 진입 중 리로드) 바로 적용
            if (EditorApplication.isPlaying) ApplyEditorPlay();
        }

        /// <summary>Play 상태 변화 처리 (테스트에서도 호출)</summary>
        public static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode) ApplyEditorPlay();
            else if (change == PlayModeStateChange.ExitingPlayMode || change == PlayModeStateChange.EnteredEditMode) RestoreBuildValue();
        }

        static void ApplyEditorPlay()
        {
            var settings = InputSystem.settings;
            if (settings && settings.backgroundBehavior != EditorPlayBackground) settings.backgroundBehavior = EditorPlayBackground;
        }

        /// <summary>플레이가 끝나면 에셋 값을 빌드용으로 되돌린다 (에셋에 IgnoreFocus가 저장되지 않게)</summary>
        static void RestoreBuildValue()
        {
            var settings = InputSystem.settings;
            if (settings && settings.backgroundBehavior != BuildBackground) settings.backgroundBehavior = BuildBackground;
        }

        static void Ensure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            var settings = AssetDatabase.LoadAssetAtPath<InputSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<InputSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }

            if (settings.editorInputBehaviorInPlayMode != Wanted || settings.backgroundBehavior != BuildBackground)
            {
                settings.editorInputBehaviorInPlayMode = Wanted;
                settings.backgroundBehavior = BuildBackground;
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
