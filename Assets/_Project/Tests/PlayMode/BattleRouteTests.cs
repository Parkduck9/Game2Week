using System.Collections;
using Game2Week.Battle;
using Game2Week.Core;
using Game2Week.Flow;
using Game2Week.Save;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    /// <summary>
    /// 11단계 검증: 키보드 입력만으로 1-1을 처치 · 살려주기 · 패배 세 결말까지 끝까지 플레이하는 "봇".
    /// 화면 상태를 보고 사람처럼 키를 누른다 (대사 Z, 탄막 턴 ↑, 메뉴 → / ↓ / Z, 게이지 가운데에서 Z).
    /// </summary>
    public class BattleRouteTests : InputTestFixture
    {
        enum Route
        {
            Kill,
            Spare,
            Defeat,
        }

        Keyboard keyboard;

        public override void Setup()
        {
            base.Setup();
            TestSave.Begin();
            keyboard = InputSystem.AddDevice<Keyboard>();
        }

        public override void TearDown()
        {
            TestSave.End();
            base.TearDown();
        }

        IEnumerator Tap(ButtonControl key, int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                PressAndRelease(key);
                yield return null;
                yield return null;
            }
        }

        IEnumerator PlayStageOne(Route route, float timeoutSeconds)
        {
            SceneManager.LoadScene(SceneNames.StageSelect);
            yield return SceneFlowTests.WaitForScene(SceneNames.StageSelect);
            Object.FindAnyObjectByType<StageSelectController>().Choose(0);
            yield return SceneFlowTests.WaitForScene(SceneNames.Battle);
            BattleController battle = null;
            yield return SceneFlowTests.WaitForBattle(c => battle = c);

            int acts = 0;
            bool holdingUp = false;
            float start = Time.realtimeSinceStartup;

            while (SceneManager.GetActiveScene().name == SceneNames.Battle && battle)
            {
                if (Time.realtimeSinceStartup - start > timeoutSeconds) Assert.Fail($"{route}: 시간 초과 (상태 {battle.Context.CurrentState})");
                var ui = battle.Ui;
                var state = battle.Context.CurrentState;

                if (state != BattleStateId.EnemyTurn && holdingUp)
                {
                    Release(keyboard.upArrowKey);
                    holdingUp = false;
                    yield return null;
                    continue;
                }

                if (ui.Dialogue.IsWaitingForInput)
                {
                    yield return Tap(keyboard.zKey);
                    continue;
                }

                switch (state)
                {
                    case BattleStateId.EnemyTurn:
                        if (route != Route.Defeat && !holdingUp)
                        {
                            Press(keyboard.upArrowKey);
                            holdingUp = true;
                        }
                        break;

                    case BattleStateId.ActionMenu when ui.IsMainMenuOpen:
                        int choice = route == Route.Kill ? 0 : acts < 2 ? 1 : 2; // 공격 / 행동 / 자비
                        yield return Tap(keyboard.rightArrowKey, choice);
                        yield return Tap(keyboard.zKey);
                        continue;

                    case BattleStateId.Fight when ui.TimingGauge.IsOpen && ui.TimingGauge.Position >= 0.47f:
                        yield return Tap(keyboard.zKey);
                        continue;

                    case BattleStateId.Act when ui.IsListMenuOpen:
                        yield return Tap(keyboard.downArrowKey); // 살펴보기 → 첫 번째 행동
                        yield return Tap(keyboard.zKey);
                        acts++;
                        continue;

                    case BattleStateId.Mercy when ui.IsListMenuOpen:
                        yield return Tap(keyboard.zKey);
                        continue;

                    case BattleStateId.ItemMenu when ui.IsMainMenuOpen:
                        yield return Tap(keyboard.rightArrowKey); // 넘기기
                        yield return Tap(keyboard.zKey);
                        continue;
                }
                yield return null;
            }

            if (holdingUp) Release(keyboard.upArrowKey);
            yield return SceneFlowTests.WaitForScene(SceneNames.Result);
        }

        [UnityTest]
        public IEnumerator Route_Kill()
        {
            yield return PlayStageOne(Route.Kill, 60f);
            Assert.AreEqual("승리", Object.FindAnyObjectByType<ResultController>().Title);
            Assert.AreEqual("EnemyDefeated", new SaveService(TestSave.Directory).Find("stage_001").bestOutcome);
            SceneCapture.Save("route_kill_result");
        }

        [UnityTest]
        public IEnumerator Route_Spare()
        {
            yield return PlayStageOne(Route.Spare, 90f);
            Assert.AreEqual("전투 종료", Object.FindAnyObjectByType<ResultController>().Title);
            Assert.AreEqual("EnemySpared", new SaveService(TestSave.Directory).Find("stage_001").bestOutcome);
        }

        [UnityTest]
        public IEnumerator Route_Defeat()
        {
            Time.timeScale = 3f; // 가만히 맞기만 하는 결말이라 빠르게
            yield return PlayStageOne(Route.Defeat, 90f);
            Time.timeScale = 1f;
            Assert.AreEqual("GAME OVER", Object.FindAnyObjectByType<ResultController>().Title);
            Assert.IsFalse(new SaveService(TestSave.Directory).HasProgress, "패배는 기록하지 않음");
            SceneCapture.Save("route_defeat_result");
        }
    }
}
