using Game2Week.Battle.View;
using Game2Week.Core;
using NUnit.Framework;
using UnityEngine;

namespace Game2Week.Tests
{
    /// <summary>6단계 순수 로직: 음량 → dB, 믹서 없을 때 소스 음량, 이펙트 재사용 목록.</summary>
    public class Stage6Tests
    {
        [Test]
        public void Volume_ToDecibels()
        {
            Assert.AreEqual(0f, AudioRouting.ToDecibels(1f), 1e-4f);
            Assert.AreEqual(-6.0206f, AudioRouting.ToDecibels(0.5f), 1e-3f);
            Assert.AreEqual(AudioRouting.SilentDecibels, AudioRouting.ToDecibels(0f));
            Assert.Less(AudioRouting.ToDecibels(0.2f), AudioRouting.ToDecibels(0.8f), "작을수록 낮은 dB");
        }

        [Test]
        public void Routing_WithoutMixer_UsesSettingVolumeOnSource()
        {
            var routing = ScriptableObject.CreateInstance<AudioRouting>();
            try
            {
                Assert.IsFalse(routing.HasMixer);
                Assert.AreEqual(0.3f, routing.Route(null, true, 0.3f));
                Assert.DoesNotThrow(() => routing.ApplyVolumes(0.5f, 0.5f));
            }
            finally { Object.DestroyImmediate(routing); }
        }

        [Test]
        public void AllScenes_OneListener_Bgm_MenuSounds()
        {
            foreach (var name in new[] { "MainMenu", "StageSelect", "Battle", "Result", "Ending" })
            {
                var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene($"Assets/_Project/Scenes/{name}.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
                Assert.AreEqual(1, Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length, $"{name}: 리스너 1개");
                var bgm = Object.FindAnyObjectByType<SceneBgm>(FindObjectsInactive.Include);
                Assert.IsNotNull(bgm, $"{name}: 배경음 연결 지점");
                var bgmProps = new UnityEditor.SerializedObject(bgm);
                Assert.IsNotNull(bgmProps.FindProperty("session").objectReferenceValue, $"{name}: 배경음 → 설정(세션)");
                Assert.IsNotNull(bgmProps.FindProperty("routing").objectReferenceValue, $"{name}: 배경음 → 출력 경로");
                foreach (var menu in Object.FindObjectsByType<Game2Week.UI.MenuNavigator>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    Assert.IsTrue(menu.HasSounds, $"{name}/{menu.name}: 메뉴 효과음");
                Assert.IsTrue(scene.IsValid());
            }
        }

        [Test]
        public void FxPool_ReusesFreeItems_CreatesOnlyWhenBusy()
        {
            int created = 0;
            var pool = new FxPool<bool[]>(() => { created++; return new[] { false }; }, item => !item[0]);
            var first = pool.Get();
            first[0] = true; // 사용 중
            var second = pool.Get();
            Assert.AreNotSame(first, second);
            Assert.AreEqual(2, created);
            first[0] = false; // 끝남
            Assert.AreSame(first, pool.Get(), "끝난 것을 다시 씀");
            Assert.AreEqual(2, pool.Count);
        }
    }
}
