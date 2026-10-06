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
            SceneCapture.Save("battle_2_EnemyTurn");

            battle.Context.ChangeState(BattleStateId.ActionMenu);
            yield return new WaitForSeconds(1f);
            Assert.IsTrue(battle.Ui.IsMainMenuOpen);
            SceneCapture.Save("battle_3_ActionMenu");

            battle.Ui.ChooseMain(0); // 공격 (임시: 바로 데미지)
            yield return new WaitForSeconds(0.1f);
            SceneCapture.Save("battle_4_Fight");
            Assert.Less(battle.Context.Enemy.CurrentHp, battle.Context.Enemy.MaxHp);

            yield return new WaitForSeconds(0.9f);
            battle.Context.ChangeState(BattleStateId.ItemMenu);
            yield return new WaitForSeconds(0.8f);
            SceneCapture.Save("battle_5_ItemMenu");
        }
    }
}
