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
            BattleController battle = null;
            yield return SceneFlowTests.EnterStage(0, c => battle = c);

            var spawner = battle.Spawner;
            var stage = new StageRepository(StageRepository.DefaultDirectory).LoadStage("stage_001").Stage;
            Assert.AreEqual(StageGeometry.ArenaSize(stage.grid), spawner.Arena.Size);
            Assert.AreEqual(stage.gems.Count, spawner.Gems.Count);
            Assert.IsNotNull(spawner.Enemy, "적에 EnemyView가 있어야 함");
            Assert.AreEqual(BattleStateId.Dialogue, battle.Context.CurrentState);

            yield return new WaitForSeconds(1.2f);
            SceneCapture.Save("battle_1_Intro");

            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            battle.World.ShowGems(spawner.Gems.Keys.ToList()); // 확인용: 모든 보석 표시
            yield return new WaitForSeconds(1f);
            Assert.That(Vector3.Distance(spawner.Arena.CellToWorld(stage.playerStart), spawner.Player.position), Is.LessThan(0.01f));
            // 9단계부터 1-1도 PatternDirector — 첫 턴의 주 공격은 노랑 직선 조작 시험
            var pattern = BattleTestUtil.FindPattern<Game2Week.Battle.Patterns.YellowTrainingPattern>(battle);
            Assert.IsNotNull(pattern, "stage_001 → 노랑 직선 조작 시험");
            Assert.Greater(pattern.ActiveBullets, 0, "첫 물결 발사됨");
            yield return new WaitForSeconds(0.5f);
            SceneCapture.Save("battle_2_EnemyTurn");

            // 가만히 있으면 탄에 맞는다 — 넓은 맵이라 적 4m 앞으로 옮겨 다음 조준을 받는다
            BattleTestUtil.MoveNearEnemy(battle, 4f);
            yield return new WaitForSeconds(4f);
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
            battle.Context.ChangeState(BattleStateId.Defeat);
            yield return new WaitForSeconds(0.6f);
            Assert.IsFalse(spawner.Player.GetComponentInChildren<Renderer>().enabled, "패배 연출: 주인공 사라짐");
            SceneCapture.Save("battle_6_Defeat");
        }

        /// <summary>에셋 연결 검증: 1-2 주황 적·노랑 사인파·스테이지 데이터대로의 경기장 (9단계부터 21×24m).</summary>
        [UnityTest]
        public IEnumerator Stage2_FromAssetsOnly_Works()
        {
            SceneManager.LoadScene(SceneNames.StageSelect);
            yield return SceneFlowTests.WaitForScene(SceneNames.StageSelect);
            Object.FindAnyObjectByType<StageSelectController>().Choose(1);
            yield return SceneFlowTests.WaitForScene(SceneNames.Battle);
            BattleController battle = null;
            yield return SceneFlowTests.WaitForBattle(c => battle = c);

            Assert.AreEqual("stage_002", battle.Context.Stage.id);
            Assert.AreEqual(new Vector2(21f, 24f), battle.Spawner.Arena.Size);
            Assert.AreEqual("주황 테스트 적", battle.Context.Enemy.Data.DisplayName);
            Assert.AreEqual(3, battle.Context.Enemy.Data.Acts.Count);

            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            yield return new WaitForSeconds(1.2f);
            // 1-2부터 PatternDirector 아래에서 첫 턴은 새 패턴(노랑 사인파) 단독
            var pattern = battle.World.Patterns.CurrentObject.GetComponentInChildren<Game2Week.Battle.Patterns.YellowTrainingPattern>();
            Assert.Greater(pattern.ActiveBullets, 0);
            SceneCapture.Save("stage2_EnemyTurn");

            battle.Context.ChangeState(BattleStateId.ActionMenu);
            yield return new WaitForSeconds(1f);
            SceneCapture.Save("stage2_ActionMenu");
        }

        /// <summary>모든 스테이지가 데이터대로 만들어지고 탄막이 도는지 + 스테이지별 캡처 (Logs/scene_stage_*.png)</summary>
        [UnityTest]
        public IEnumerator AllStages_BuildFromData()
        {
            var repo = new StageRepository(StageRepository.DefaultDirectory);
            var ids = repo.LoadIndex().stages;
            Assert.GreaterOrEqual(ids.Count, 5);

            for (int i = 0; i < ids.Count; i++)
            {
                SceneManager.LoadScene(SceneNames.StageSelect);
                yield return SceneFlowTests.WaitForScene(SceneNames.StageSelect);
                Object.FindAnyObjectByType<StageSelectController>().Choose(i);
                yield return SceneFlowTests.WaitForScene(SceneNames.Battle);
                BattleController battle = null;
                yield return SceneFlowTests.WaitForBattle(c => battle = c);

                var stage = repo.LoadStage(ids[i]).Stage;
                Assert.AreEqual(ids[i], battle.Context.Stage.id);
                Assert.AreEqual(StageGeometry.ArenaSize(stage.grid), battle.Spawner.Arena.Size, ids[i]);
                Assert.AreEqual(stage.gems.Count, battle.Spawner.Gems.Count, ids[i]);
                Assert.That(Vector3.Distance(battle.Spawner.Arena.CellToWorld(stage.enemy.position), battle.Spawner.Enemy.transform.position), Is.LessThan(0.01f), ids[i]);

                battle.Context.ChangeState(BattleStateId.EnemyTurn);
                battle.World.ShowGems(battle.Spawner.Gems.Keys.ToList());
                yield return new WaitForSeconds(1.3f);
                Assert.IsNotNull(battle.World.Patterns.CurrentObject, $"{ids[i]}: 탄막 패턴 실행");
                SceneCapture.Save($"stage_{i + 1}_{ids[i]}");
            }
        }

        [UnityTest]
        public IEnumerator ItemMenu_Capture()
        {
            BattleController battle = null;
            yield return SceneFlowTests.EnterStage(0, c => battle = c);
            battle.Context.ChangeState(BattleStateId.ItemMenu);
            yield return new WaitForSeconds(0.8f);
            SceneCapture.Save("battle_5_ItemMenu");
        }
    }
}
