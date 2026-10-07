using System.Collections;
using Game2Week.Battle;
using Game2Week.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    public class DialogueRuntimeTests
    {
        [SetUp] public void Setup() => TestSave.Begin();
        [TearDown] public void TearDown() => TestSave.End();
        static IEnumerator Close(BattleController battle)
        {
            battle.Ui.Dialogue.Advance(); battle.Ui.Dialogue.Advance(); yield return null;
        }
        static IEnumerator WaitForResult(BattleController battle)
        {
            float start=Time.realtimeSinceStartup;
            while(!battle.Ui.Dialogue.IsWaitingForInput){Assert.Less(Time.realtimeSinceStartup-start,5,"행동 결과 대기 시간 초과");yield return null;}
        }
        static IEnumerator WaitForTransition(BattleController battle)
        {
            float start=Time.realtimeSinceStartup;
            while(battle.Context.CurrentState==BattleStateId.ActionPresentation){Assert.Less(Time.realtimeSinceStartup-start,5,"후퇴 대기 시간 초과");yield return null;}
        }
        static IEnumerator Intro(BattleController battle)
        {
            yield return Close(battle); yield return Close(battle);
            Assert.IsTrue(battle.Ui.IsListMenuOpen); battle.Ui.ChooseList(0); yield return null;
        }
        [UnityTest] public IEnumerator 도입선택이자비상태에반영되고탄막턴으로복귀()
        {
            BattleController battle = null; yield return SceneFlowTests.EnterStage(0, found => battle = found);
            Assert.AreEqual(BattleStateId.Dialogue, battle.Context.CurrentState); battle.Ui.Dialogue.Advance(); SceneCapture.Save("phase10_dialogue_intro");
            yield return Intro(battle);
            Assert.IsTrue(battle.Context.Enemy.Spare.HasFlag("listened")); Assert.AreEqual(BattleStateId.EnemyTurn, battle.Context.CurrentState);
            Assert.IsFalse(battle.Context.Enemy.CanBeSpared); Assert.AreEqual(0, battle.Context.Enemy.Spare.NoFightTurns);
            battle.Context.ChangeState(BattleStateId.ItemMenu);
        }
        [UnityTest] public IEnumerator 삼턴전에는자비불가이며조건충족뒤대화와자비메뉴연결()
        {
            BattleController battle = null; yield return SceneFlowTests.EnterStage(0, found => battle = found); yield return Intro(battle);
            for (int turn = 0; turn < 3; turn++)
            {
                battle.Context.ChangeState(BattleStateId.ActionMenu); battle.Ui.ChooseMain(1);
                battle.Ui.ChooseList(turn < 2 ? turn + 1 : 0);
                yield return WaitForResult(battle);
                if (turn < 2) Assert.IsFalse(battle.Context.Enemy.CanBeSpared);
                yield return Close(battle);yield return WaitForTransition(battle);
            }
            Assert.IsTrue(battle.Context.Enemy.CanBeSpared); Assert.AreEqual(3, battle.Context.Enemy.Spare.NoFightTurns);
            Assert.AreEqual(BattleStateId.Dialogue, battle.Context.CurrentState); battle.Ui.Dialogue.Advance(); SceneCapture.Save("phase10_spare_ready");
            yield return Close(battle); yield return Close(battle);
            Assert.AreEqual(BattleStateId.EnemyTurn, battle.Context.CurrentState);
            battle.Context.ChangeState(BattleStateId.ActionMenu); battle.Ui.ChooseMain(2); Assert.IsTrue(battle.Ui.IsListMenuOpen);
        }
    }
}
