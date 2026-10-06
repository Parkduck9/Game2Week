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
            SceneManager.LoadScene(SceneNames.Battle);
            BattleController battle = null;
            yield return SceneFlowTests.WaitForBattle(c => battle = c);
            var ctx = battle.Context;

            // 등장 대사: Z 한 번 = 전부 표시, 한 번 더 = 닫기
            for (int i = 0; i < 4 && ctx.CurrentState == BattleStateId.Intro; i++) yield return Tap(keyboard.zKey);
            Assert.AreEqual(BattleStateId.EnemyTurn, ctx.CurrentState);

            // ↑를 누르고 있으면 적에게 닿는다 (시작 칸 → 적 칸 약 5.5m, 속도 2.8m/s, 턴 8초)
            Press(keyboard.upArrowKey);
            yield return WaitUntil(() => ctx.CurrentState == BattleStateId.ActionMenu, 6f, "적에게 닿기");
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

        [UnityTest]
        public IEnumerator Esc_PausesAndResumes_DialogueStillWorks()
        {
            SceneManager.LoadScene(SceneNames.Battle);
            BattleController battle = null;
            yield return SceneFlowTests.WaitForBattle(c => battle = c);
            Assert.AreEqual(BattleStateId.Intro, battle.Context.CurrentState);

            yield return Tap(keyboard.escapeKey);
            Assert.IsTrue(battle.Pause.IsPaused);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsTrue(battle.Pause.Menu.gameObject.activeInHierarchy);
            SceneCapture.Save("battle_pause");

            yield return Tap(keyboard.zKey); // 일시정지 중 Z는 "계속"을 고름 (대사는 넘어가지 않음)
            Assert.IsFalse(battle.Pause.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.AreEqual(BattleStateId.Intro, battle.Context.CurrentState);

            yield return Tap(keyboard.escapeKey);
            yield return Tap(keyboard.escapeKey); // ESC 두 번 = 일시정지 후 계속
            Assert.IsFalse(battle.Pause.IsPaused);

            // 풀린 뒤 대사창이 다시 입력을 받는지
            for (int i = 0; i < 4 && battle.Context.CurrentState == BattleStateId.Intro; i++) yield return Tap(keyboard.zKey);
            Assert.AreEqual(BattleStateId.EnemyTurn, battle.Context.CurrentState);
        }
    }
}
