using Game2Week.EditorTools;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.InputSystem;

namespace Game2Week.Tests
{
    /// <summary>
    /// 회귀 (2026-10-07 사용자: "Play 누를 때마다 키보드가 됐다 안 됐다"):
    /// 에디터 플레이 중에는 Game 창 포커스와 상관없이 입력을 받고(IgnoreFocus), 플레이가 끝나면 빌드용 값으로 돌아간다.
    /// </summary>
    public class InputFocusSettingsTests
    {
        [Test]
        public void EditorPlay_IgnoresFocus_AndRestoresBuildValue()
        {
            var settings = InputSystem.settings;
            var before = settings.backgroundBehavior;
            try
            {
                Assert.AreEqual(InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView, settings.editorInputBehaviorInPlayMode);
                InputSettingsSetup.OnPlayModeChanged(PlayModeStateChange.EnteredPlayMode);
                Assert.AreEqual(InputSettings.BackgroundBehavior.IgnoreFocus, InputSystem.settings.backgroundBehavior, "플레이 중: Game 창 포커스 무시");
                InputSettingsSetup.OnPlayModeChanged(PlayModeStateChange.EnteredEditMode);
                Assert.AreEqual(InputSettingsSetup.BuildBackground, InputSystem.settings.backgroundBehavior, "끝나면 빌드용 값");
            }
            finally { settings.backgroundBehavior = before; }
        }

        [Test]
        public void SettingsAsset_KeepsBuildValue()
        {
            var asset = AssetDatabase.LoadAssetAtPath<InputSettings>("Assets/_Project/Input/InputSystemSettings.asset");
            Assert.AreEqual(InputSettingsSetup.BuildBackground, asset.backgroundBehavior, "에셋(빌드)에는 IgnoreFocus가 저장되지 않는다");
        }
    }
}
