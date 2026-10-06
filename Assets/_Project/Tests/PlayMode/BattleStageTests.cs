using System.Collections;
using System.Linq;
using Game2Week.Battle;
using Game2Week.Battle.View;
using Game2Week.Core;
using Game2Week.Flow;
using Game2Week.Stages;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    /// <summary>Battle 씬이 stage_001대로 구성되는지 + 상태별 화면 캡처 (Logs/scene_battle_*.png).</summary>
    public class BattleStageTests
    {
        [SetUp]
        public void SetUp() => TestSave.Begin();

        [TearDown]
        public void TearDown() => TestSave.End();

        [UnityTest]
        public IEnumerator Battle_BuildsStage_AndShowsEachPhase()
        {
            SceneManager.LoadScene(SceneNames.Battle);
            BattleController battle = null;
            yield return SceneFlowTests.WaitForBattle(c => battle = c);

            var spawner = battle.Spawner;
            var stage = new StageRepository(StageRepository.DefaultDirectory).LoadStage("stage_001").Stage;
            Assert.AreEqual(StageGeometry.ArenaSize(stage.grid), spawner.Arena.Size);
            Assert.AreEqual(stage.gems.Count, spawner.Gems.Count);
            Assert.IsNotNull(spawner.Enemy, "적에 EnemyView가 있어야 함");
            Assert.AreEqual(BattleStateId.Intro, battle.Context.CurrentState);

            yield return new WaitForSeconds(1.2f);
            SceneCapture.Save("battle_1_Intro");

            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            battle.World.ShowGems(spawner.Gems.Keys.ToList()); // 확인용: 모든 보석 표시
            yield return new WaitForSeconds(1f);
            Assert.That(Vector3.Distance(spawner.Arena.CellToWorld(stage.playerStart), spawner.Player.position), Is.LessThan(0.01f));
            var pattern = battle.World.Patterns.CurrentObject ? battle.World.Patterns.CurrentObject.GetComponent<Game2Week.Battle.Patterns.RadialBurstPattern>() : null;
            Assert.IsNotNull(pattern, "stage_001 → Pattern_Test(방사형 결정탄)");
            Assert.Greater(pattern.ActiveBullets, 0, "첫 물결 발사됨");
            yield return new WaitForSeconds(0.5f);
            SceneCapture.Save("battle_2_EnemyTurn");

            // 가만히 있으면 탄에 맞는다 (적 바로 아래 = 탄 방향 하나와 일치)
            yield return new WaitForSeconds(2.5f);
            Assert.Less(battle.Context.Player.CurrentHp, battle.Context.Player.MaxHp, "탄에 맞아 HP 감소");

            battle.Context.ChangeState(BattleStateId.ActionMenu);
            yield return new WaitForSeconds(1f);
            Assert.IsNull(battle.World.Patterns.CurrentObject, "턴이 끝나면 탄막 정리");
            Assert.IsTrue(battle.Ui.IsMainMenuOpen);
            SceneCapture.Save("battle_3_ActionMenu");

            battle.Ui.ChooseMain(0); // 공격 → 타이밍 게이지
            yield return new WaitForSeconds(0.55f);
            Assert.IsTrue(battle.Ui.TimingGauge.IsOpen);
            SceneCapture.Save("battle_4_Gauge");
            battle.Ui.TimingGauge.Press();
            yield return new WaitForSeconds(0.5f);
            SceneCapture.Save("battle_4_Fight");
            Assert.Less(battle.Context.Enemy.CurrentHp, battle.Context.Enemy.MaxHp);

            yield return new WaitForSeconds(0.9f);
            battle.Context.ChangeState(BattleStateId.ItemMenu);
            yield return new WaitForSeconds(0.8f);
            SceneCapture.Save("battle_5_ItemMenu");
        }
    }
}
