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
    /// 1-1의 처치·살려주기·패배 결말을 확인하는 봇. 메뉴·대사는 실제 키 입력을 사용한다.
    /// 전체 실행 순서에 영향을 받는 이동은 모터에 직접 전달하며, 회피 실력은 측정 봇에서 확인한다.
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
            float start = Time.realtimeSinceStartup;
            // 9단계: 이 봇은 피하지 않고 직진만 하므로(결말 흐름 검사), 넓은 맵·겹 공격에서 살아남게 처치·살려주기 길은 체력을 채우고
            // 처치 길은 적 체력을 공격 한 번 분량으로 줄인다. 회피 실력·밸런스는 BalanceMeasurementTests가 맡는다.
            if (route == Route.Kill) battle.Context.Enemy.TakeDamage(battle.Context.Enemy.MaxHp - 5);

            while (SceneManager.GetActiveScene().name == SceneNames.Battle && battle)
            {
                if (Time.realtimeSinceStartup - start > timeoutSeconds) Assert.Fail($"{route}: 시간 초과 (상태 {battle.Context.CurrentState})");
                if (route != Route.Defeat && battle.Context.Player.CurrentHp < battle.Context.Player.MaxHp)
                    battle.Context.Player.Heal(battle.Context.Player.MaxHp);
                var ui = battle.Ui;
                var state = battle.Context.CurrentState;

                if (ui.Dialogue.IsWaitingForInput)
                {
                    yield return Tap(keyboard.zKey);
                    continue;
                }
                if (state == BattleStateId.Dialogue && ui.IsListMenuOpen) { yield return Tap(keyboard.zKey); continue; }

                switch (state)
                {
                    case BattleStateId.EnemyTurn:
                        if (route != Route.Defeat)
                        {
                            var direction = battle.Spawner.Enemy.transform.position - battle.World.Player.transform.position;
                            battle.World.Player.Move(new Vector2(direction.x, direction.z).normalized, Time.deltaTime);
                        }
                        break;

                    case BattleStateId.ActionMenu when ui.IsMainMenuOpen:
                        int choice = route == Route.Kill ? 0 : battle.Context.Enemy.CanBeSpared ? 2 : 1; // 공격 / 행동 / 자비
                        yield return Tap(keyboard.rightArrowKey, choice);
                        yield return Tap(keyboard.zKey);
                        continue;

                    case BattleStateId.Fight when ui.TimingGauge.IsOpen && ui.TimingGauge.Position >= 0.47f:
                        yield return Tap(keyboard.zKey);
                        continue;

                    case BattleStateId.Act when ui.IsListMenuOpen:
                        yield return Tap(keyboard.downArrowKey, acts == 0 ? 1 : acts == 1 ? 2 : 0); // 응원하기 → 말 걸기 → 살펴보기
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
