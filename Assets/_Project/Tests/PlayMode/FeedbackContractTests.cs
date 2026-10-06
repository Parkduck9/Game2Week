using System.Collections;
using Game2Week.Battle;
using Game2Week.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    /// <summary>6-0 연결 지점: BattleFxRig가 만들어져 알림 창구를 받고, 주인공 동작·적 대사가 알림으로 나온다.</summary>
    public class FeedbackContractTests
    {
        [SetUp] public void SetUp() => TestSave.Begin();
        [TearDown] public void TearDown() => TestSave.End();

        [UnityTest]
        public IEnumerator FxRig_IsBound_AndPlayerActionsAndEnemyLinesAreAnnounced()
        {
            BattleController battle = null;
            yield return SceneFlowTests.EnterStage(0, c => battle = c);
            Assert.IsNotNull(battle.FxRig, "Battle 씬에 이펙트 묶음 프리팹 연결");
            Assert.IsTrue(battle.FxRig.IsBound);
            Assert.AreSame(battle.World.Feedback, battle.FxRig.Feedback);

            int dodged = 0, jumped = 0, landed = 0, spoke = 0;
            var feedback = battle.World.Feedback;
            feedback.PlayerDodged += _ => dodged++;
            feedback.PlayerJumped += _ => jumped++;
            feedback.PlayerLanded += _ => landed++;
            battle.Context.Events.EnemySpoke += _ => spoke++;

            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            yield return null;
            Assert.AreEqual(1, spoke, "탄막 턴 시작 때 적 대사 알림");

            var player = battle.World.Player;
            player.Motor.RequestDodge();
            yield return new WaitForSeconds(.3f);
            Assert.AreEqual(1, dodged);

            yield return new WaitForSeconds(1f); // 회피 쿨다운 뒤 점프
            player.Motor.RequestJump();
            yield return new WaitForSeconds(.9f);
            Assert.AreEqual(1, jumped);
            Assert.AreEqual(1, landed);
        }
    }
}
