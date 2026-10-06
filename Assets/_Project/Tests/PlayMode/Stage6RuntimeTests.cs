using System.Collections;
using Game2Week.Battle;
using Game2Week.Battle.View;
using Game2Week.Core;
using Game2Week.Flow;
using Game2Week.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    /// <summary>6단계 실제 실행: 전투 이펙트·말풍선·효과음 알림, 씬 전환 페이드·배경음·리스너·메뉴 효과음 연결.</summary>
    public class Stage6RuntimeTests
    {
        [SetUp] public void SetUp() => TestSave.Begin();
        [TearDown] public void TearDown() { Time.timeScale = 1f; TestSave.End(); }

        [UnityTest]
        public IEnumerator Battle_EffectsBubbleAndSoundCues_FollowTurn()
        {
            BattleController battle = null;
            yield return SceneFlowTests.EnterStage(0, c => battle = c);
            var fx = battle.FxRig.GetComponentInChildren<ActionFxModule>(true);
            var bubble = battle.FxRig.GetComponentInChildren<EnemySpeechBubble>(true);
            Assert.IsNotNull(fx, "이펙트 묶음에 동작 효과 모듈");
            Assert.IsNotNull(bubble, "이펙트 묶음에 말풍선 모듈");
            Assert.IsNotNull(battle.Audio, "전투 효과음 연결 지점");

            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            yield return null;
            Assert.IsTrue(bubble.IsShowing, "탄막 턴 시작 때 적 대사 말풍선");
            StringAssert.Contains("간다", bubble.Text);

            yield return new WaitForSeconds(1.2f); // 1-1 예고(0.75초) → 발사
            Assert.Greater(fx.Spawned, 0, "예고 고리·발사 연기");
            Assert.Greater(battle.Audio.CueCount, 0, "예고·발사 효과음 알림");
            int before = fx.Spawned;
            battle.World.Player.Motor.RequestDodge();
            yield return new WaitForSeconds(0.1f);
            Assert.Greater(fx.Spawned, before, "회피 잔광");
            SceneCapture.Save("stage6_battle_fx");

            battle.Context.ChangeState(BattleStateId.ItemMenu);
            yield return null;
            Assert.IsFalse(bubble.IsShowing, "턴이 끝나면 말풍선 숨김");
            Assert.AreEqual(0, fx.ActiveRings, "턴이 끝나면 고리 정리");
        }

        [UnityTest]
        public IEnumerator SceneLoad_FadesOutAndIn_EachSceneHasOneListenerBgmAndMenuSounds()
        {
            SceneManager.LoadScene(SceneNames.MainMenu);
            yield return SceneFlowTests.WaitForScene(SceneNames.MainMenu);
            // 결과·엔딩 씬은 전투 결과가 있어야 열려서 여기선 메인 ↔ 선택만 (씬 파일 연결은 EditMode Stage6Tests가 5개 모두 검사)
            foreach (var next in new[] { SceneNames.StageSelect, SceneNames.MainMenu })
            {
                Assert.AreEqual(1, Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length, $"{SceneManager.GetActiveScene().name}: 리스너 1개");
                Assert.IsNotNull(Object.FindAnyObjectByType<SceneBgm>(), $"{SceneManager.GetActiveScene().name}: 배경음 연결 지점");
                foreach (var menu in Object.FindObjectsByType<MenuNavigator>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    Assert.IsTrue(menu.HasSounds, $"{SceneManager.GetActiveScene().name}/{menu.name}: 메뉴 효과음 연결");

                Assert.IsTrue(SceneLoader.Load(next));
                yield return new WaitForSeconds(SceneLoader.FadeSeconds * 0.8f);
                Assert.Greater(SceneLoader.Fader.Alpha, 0.3f, "떠날 때 어두워짐");
                yield return SceneFlowTests.WaitForScene(next);
                yield return new WaitForSecondsRealtime(SceneLoader.FadeSeconds + 0.1f);
                Assert.AreEqual(0f, SceneLoader.Fader.Alpha, 0.01f, "새 씬에서 다시 밝아짐");
            }
        }
    }
}
