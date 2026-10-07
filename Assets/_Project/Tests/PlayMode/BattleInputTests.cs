using System;
using System.Collections;
using Game2Week.Battle;
using Game2Week.Core;
using Game2Week.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    /// <summary>실제 키보드 입력 경로 확인: Z로 대사 넘기기 → ↑로 적에게 닿기 → 메뉴 → X로 취소.</summary>
    public class BattleInputTests : InputTestFixture
    {
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

        IEnumerator WaitUntil(Func<bool> condition, float timeout, string what)
        {
            float start = Time.realtimeSinceStartup;
            while (!condition())
            {
                if (Time.realtimeSinceStartup - start > timeout) Assert.Fail($"시간 초과: {what}");
                yield return null;
            }
        }

        IEnumerator Tap(ButtonControl key)
        {
            PressAndRelease(key);
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator Keyboard_ReachEnemy_ChooseAct_CancelBack()
        {
            BattleController battle = null;
            yield return SceneFlowTests.EnterStage(0, c => battle = c);
            var ctx = battle.Context;

            // 등장 대사: Z 한 번 = 전부 표시, 한 번 더 = 닫기
            for (int i = 0; i < 12 && (ctx.CurrentState == BattleStateId.Intro || ctx.CurrentState == BattleStateId.Dialogue); i++) yield return Tap(keyboard.zKey);
            Assert.AreEqual(BattleStateId.EnemyTurn, ctx.CurrentState);

            // ↑를 누르고 있으면 적에게 닿는다 (시작 칸 → 적 칸 약 5.5m, 속도 2.8m/s, 턴 8초)
            Press(keyboard.upArrowKey);
            yield return WaitUntil(() => ctx.CurrentState == BattleStateId.ActionMenu, 10f, "적에게 닿기"); // 9단계 넓은 맵 18m
            Release(keyboard.upArrowKey);
            yield return null;
            Assert.IsTrue(battle.Ui.IsMainMenuOpen);

            // → 로 "행동", Z 로 선택 → 목록, X 로 취소 → 다시 행동 메뉴
            yield return Tap(keyboard.rightArrowKey);
            yield return Tap(keyboard.zKey);
            Assert.AreEqual(BattleStateId.Act, ctx.CurrentState);
            Assert.IsTrue(battle.Ui.IsListMenuOpen);

            yield return Tap(keyboard.xKey);
            Assert.AreEqual(BattleStateId.ActionMenu, ctx.CurrentState);
        }

        /// <summary>사용자 요청: 행동 메뉴에서도 A·D로 좌우 이동 (방향키와 같게), W·S는 목록 위아래.</summary>
        [UnityTest]
        public IEnumerator Menu_WASD_MovesLikeArrows()
        {
            BattleController battle = null;
            yield return SceneFlowTests.EnterStage(0, c => battle = c);
            var ctx = battle.Context;
            ctx.ChangeState(BattleStateId.ActionMenu);
            yield return null; yield return null;
            Assert.IsTrue(battle.Ui.IsMainMenuOpen);

            yield return Tap(keyboard.dKey); // 공격 → 행동
            yield return Tap(keyboard.zKey);
            Assert.AreEqual(BattleStateId.Act, ctx.CurrentState, "D = 오른쪽");
            yield return Tap(keyboard.xKey);
            Assert.AreEqual(BattleStateId.ActionMenu, ctx.CurrentState);

            ctx.ChangeState(BattleStateId.ItemMenu); yield return null;
            ctx.ChangeState(BattleStateId.ActionMenu); yield return null; yield return null; // 새로 연 메뉴 (공격부터)
            yield return Tap(keyboard.dKey); // 공격 → 행동
            yield return Tap(keyboard.dKey); // 행동 → 자비
            yield return Tap(keyboard.aKey); // 자비 → 행동
            yield return Tap(keyboard.zKey);
            Assert.AreEqual(BattleStateId.Act, ctx.CurrentState, "A = 왼쪽");
            Assert.IsTrue(battle.Ui.IsListMenuOpen);
        }

        [UnityTest]
        public IEnumerator Esc_PausesAndResumes_DialogueStillWorks()
        {
            BattleController battle = null;
            yield return SceneFlowTests.EnterStage(0, c => battle = c);
            Assert.AreEqual(BattleStateId.Dialogue, battle.Context.CurrentState);

            yield return Tap(keyboard.escapeKey);
            Assert.IsTrue(battle.Pause.IsPaused);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsTrue(battle.Pause.Menu.gameObject.activeInHierarchy);
            SceneCapture.Save("battle_pause");

            yield return Tap(keyboard.zKey); // 일시정지 중 Z는 "계속"을 고름 (대사는 넘어가지 않음)
            Assert.IsFalse(battle.Pause.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.AreEqual(BattleStateId.Dialogue, battle.Context.CurrentState);

            yield return Tap(keyboard.escapeKey);
            yield return Tap(keyboard.escapeKey); // ESC 두 번 = 일시정지 후 계속
            Assert.IsFalse(battle.Pause.IsPaused);

            // 풀린 뒤 대사창이 다시 입력을 받는지
            for (int i = 0; i < 12 && (battle.Context.CurrentState == BattleStateId.Intro || battle.Context.CurrentState == BattleStateId.Dialogue); i++) yield return Tap(keyboard.zKey);
            Assert.AreEqual(BattleStateId.EnemyTurn, battle.Context.CurrentState);
        }
    }
}
