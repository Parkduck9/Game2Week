using System.Collections;
using Game2Week.Battle;
using Game2Week.Battle.View.Animation;
using Game2Week.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    /// <summary>5~8단계 통합: 실제 전투에서 주인공 애니메이션이 피격 알림을 받고, 패배 때 쓰러진 뒤 사라진다.</summary>
    public class IntegrationTests
    {
        [SetUp] public void SetUp() => TestSave.Begin();
        [TearDown] public void TearDown() { Time.timeScale = 1f; TestSave.End(); }

        [UnityTest]
        public IEnumerator HeroineAnimation_ReceivesHit_AndFallsOnDefeat()
        {
            BattleController battle = null;
            yield return SceneFlowTests.EnterStage(0, c => battle = c);
            var driver = battle.World.Player.GetComponentInChildren<PlayerAnimationDriver>();
            Assert.IsNotNull(driver, "주인공 v2 애니메이션 구동기");
            Assert.IsTrue(driver.IsReady);

            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            yield return null;
            battle.World.Feedback.RaisePlayerHit(battle.World.Player.transform.position);
            yield return null; yield return null;
            Assert.AreEqual(PlayerMotion.Hit, driver.Current, "BattleWorld가 피격 알림을 애니메이션에 연결");

            battle.Context.ChangeState(BattleStateId.Defeat);
            yield return null; yield return null;
            Assert.AreEqual(PlayerMotion.Fall, driver.Current, "패배 때 쓰러짐 동작");
            Assert.IsTrue(battle.World.Player.GetComponentInChildren<Renderer>().enabled, "쓰러지는 동안은 보임");
            yield return new WaitForSeconds(0.6f);
            Assert.IsFalse(battle.World.Player.GetComponentInChildren<Renderer>().enabled, "쓰러진 뒤 사라짐");
            SceneCapture.Save("integration_defeat");
        }
    }
}
