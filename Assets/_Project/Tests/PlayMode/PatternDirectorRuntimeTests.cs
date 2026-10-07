using System.Collections;
using Game2Week.Battle;
using Game2Week.Battle.Patterns;
using Game2Week.Battle.Patterns.Director;
using Game2Week.Battle.UI;
using Game2Week.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    /// <summary>5단계 실제 전투: 새 패턴 단독 소개 → 턴 넘어 기록 유지 → 두 패턴 겹침(색 규칙) → 정리, 색 처음 등장 안내.</summary>
    public class PatternDirectorRuntimeTests
    {
        [SetUp] public void SetUp() => TestSave.Begin();
        [TearDown] public void TearDown() { Time.timeScale = 1f; TestSave.End(); }

        static IEnumerator NextEnemyTurn(BattleController battle)
        {
            battle.Context.ChangeState(BattleStateId.ItemMenu);
            yield return null;
            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Stage5_IntroducesAlone_KeepsMemory_ThenOverlapsCompatibleColors()
        {
            BattleController battle = null;
            yield return SceneFlowTests.EnterStage(4, c => battle = c);
            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            yield return null;

            var director = (PatternDirector)battle.World.Patterns.Current;
            Assert.AreEqual(0, director.Plan.Primary, "첫 턴은 새 패턴(포물선)");
            Assert.IsTrue(director.Plan.IsIntroTurn, "소개 턴");
            Assert.AreEqual(1, director.ActiveObjects.Count, "소개 동안은 새 패턴 혼자");
            var state = director.State;
            yield return new WaitForSeconds(PatternDirector.IntroduceSeconds + 0.2f);
            Assert.IsTrue(state.Introduced);
            Assert.AreEqual(1, director.ActiveObjects.Count, "소개 직후에도 아직 다른 겹은 대기");
            if (director.Plan.Secondary >= 0)
            {
                yield return new WaitForSeconds(director.LayerStartTime(1) - director.Elapsed + 0.3f);
                Assert.GreaterOrEqual(director.ActiveObjects.Count, 2, "소개 뒤 같은 턴 안에 다른 겹 합류 (9단계)");
            }

            // 소개 뒤 턴: 셔플 백 + 겹 (동시 상한 2)
            for (int turn = 0; turn < 8; turn++)
            {
                yield return NextEnemyTurn(battle);
                director = (PatternDirector)battle.World.Patterns.Current;
                Assert.AreSame(state, director.State, "턴이 바뀌어도 같은 전투 기록");
                Assert.IsFalse(director.Plan.IsIntroTurn);
                if (director.Plan.Secondary >= 0) break;
            }
            Assert.GreaterOrEqual(director.Plan.Secondary, 0, "8턴 안에 겹치는 턴이 있어야 함");
            var patterns = director.CandidatePatterns;
            Assert.IsTrue(ColorCombinationRules.CanOverlap(
                PatternDirector.ColorOf(patterns[director.Plan.Primary]), PatternDirector.ColorOf(patterns[director.Plan.Secondary])));

            yield return new WaitForSeconds(director.LayerStartTime(1) + 0.3f);
            Assert.GreaterOrEqual(director.ActiveObjects.Count, 2, "두 번째 패턴이 늦게 겹침");
            SceneCapture.Save("director_stage5_overlap");

            var children = new System.Collections.Generic.List<GameObject>(director.ActiveObjects);
            battle.Context.ChangeState(BattleStateId.ItemMenu);
            yield return null;
            Assert.IsNull(battle.World.Patterns.CurrentObject);
            foreach (var go in children) Assert.IsTrue(go == null, "턴이 끝나면 하위 패턴 정리");
        }

        /// <summary>9단계: 적 고유 기술이 후보에 들어가고, 체력이 줄면 단계 기술·알림이 더해진다.</summary>
        [UnityTest]
        public IEnumerator EnemyMoves_AreCandidates_AndPhaseAddsMovesWhenHurt()
        {
            BattleController battle = null;
            yield return SceneFlowTests.EnterStage(0, c => battle = c);
            var enemyData = battle.Context.Enemy.Data;
            Assert.Greater(enemyData.SignatureMoves.Count, 0, "적 고유 기술");
            Assert.Greater(enemyData.PhaseMoves.Count, 0, "체력 단계 기술");
            int phaseEvents = 0;
            battle.World.Feedback.EnemyPhaseChanged += _ => phaseEvents++;

            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            yield return null;
            var director = (PatternDirector)battle.World.Patterns.Current;
            Assert.AreEqual(0, director.Phase);
            CollectionAssert.IsSubsetOf(enemyData.SignatureMoves, director.CandidatePatterns, "1-1도 적 고유 기술을 함께 쓴다");
            CollectionAssert.IsNotSubsetOf(enemyData.PhaseMoves, director.CandidatePatterns);
            Assert.GreaterOrEqual(director.Plan.LayerCount, 2, "한 턴에 두 겹 이상");

            battle.Context.Enemy.TakeDamage(battle.Context.Enemy.MaxHp * 6 / 10);
            yield return NextEnemyTurn(battle);
            director = (PatternDirector)battle.World.Patterns.Current;
            Assert.AreEqual(1, director.Phase, "체력 50% 이하 → 단계 1");
            CollectionAssert.IsSubsetOf(enemyData.PhaseMoves, director.CandidatePatterns, "단계 기술 추가");
            Assert.AreEqual(1, phaseEvents, "단계가 오른 턴에 한 번 알림");
            yield return NextEnemyTurn(battle);
            Assert.AreEqual(1, phaseEvents, "같은 단계에서는 다시 알리지 않음");
            battle.Context.ChangeState(BattleStateId.ItemMenu);
        }

        [UnityTest]
        public IEnumerator Stage4_RedGuide_ShowsOnce()
        {
            BattleController battle = null;
            yield return SceneFlowTests.EnterStage(3, c => battle = c);
            var guide = Object.FindAnyObjectByType<ColorGuideView>(FindObjectsInactive.Include);
            Assert.IsNotNull(guide, "Battle 씬에 색 안내 연결");
            Assert.IsFalse(guide.IsShowing);

            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            yield return new WaitForSeconds(0.3f);
            Assert.IsTrue(guide.IsShowing, "새 패턴(빨강) 예고와 함께 안내");
            Assert.AreEqual(BattleTexts.RedGuide, guide.Text);
            SceneCapture.Save("director_red_guide");

            yield return new WaitForSeconds(3.2f);
            Assert.IsFalse(guide.IsShowing, "잠깐 뒤 사라짐");
            yield return NextEnemyTurn(battle);
            yield return new WaitForSeconds(0.5f);
            Assert.IsFalse(guide.IsShowing, "같은 전투에서 같은 색은 한 번만");
        }
    }
}
