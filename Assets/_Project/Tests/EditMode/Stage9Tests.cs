using Game2Week.Battle.Patterns.Trajectories;
using Game2Week.Data;
using Game2Week.Data.Patterns;
using Game2Week.Stages;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game2Week.Tests
{
    /// <summary>9단계: 넓어진 맵, 스테이지 JSON v2, 넓은 맵용 탄 수명, 적 고유 기술·체력.</summary>
    public class Stage9Tests
    {
        [Test]
        public void AllStages_AreWide_TurnLongEnough_SchemaV2()
        {
            var repo = new StageRepository(StageRepository.DefaultDirectory);
            foreach (var id in repo.LoadIndex().stages)
            {
                var result = repo.LoadStage(id);
                var s = result.Stage;
                Assert.IsTrue(result.ChecksumValid, $"{id}: 맵툴로 저장");
                Assert.AreEqual(StageDefinition.CurrentSchemaVersion, s.schemaVersion, id);
                Assert.IsFalse(string.IsNullOrEmpty(s.theme), id); // 11단계: 실제 테마 연결은 GraphicsTests에서 검사한다.
                Assert.IsNotNull(s.dialogues, id);
                float meters = StageGeometry.CellDistance(s.playerStart, s.enemy.position) * s.grid.cellSize;
                Assert.GreaterOrEqual(meters, StageLimits.RecommendedPlayerEnemyMeters, $"{id}: 시작점-적 {meters:0.0}m");
                Assert.GreaterOrEqual(s.enemyTurn.duration, meters / 3.4f + 4f, $"{id}: 피하면서 닿을 시간");
                Assert.IsFalse(StageValidator.HasErrors(StageValidator.Validate(s)), id);
            }
        }

        [Test]
        public void SchemaV1_StillLoads_WithDefaults_AndShortDistanceWarns()
        {
            const string v1 = "{\"schemaVersion\":1,\"id\":\"stage_old\",\"name\":\"old\",\"grid\":{\"width\":12,\"depth\":14,\"cellSize\":0.5}," +
                              "\"playerStart\":{\"x\":6,\"z\":1},\"enemy\":{\"enemyId\":\"Enemy_Test\",\"position\":{\"x\":6,\"z\":12}}," +
                              "\"gems\":[],\"gemRules\":{\"maxPerTurn\":1},\"enemyTurn\":{\"duration\":8.0,\"patterns\":[]}}";
            var s = StageJson.ReadStage(v1);
            Assert.AreEqual(1, s.schemaVersion);
            Assert.AreEqual(StageThemes.Default, s.theme);
            Assert.AreEqual(string.Empty, s.dialogues.intro);
            var issues = StageValidator.Validate(s);
            Assert.IsFalse(StageValidator.HasErrors(issues), "v1도 읽을 수 있음");
            Assert.IsTrue(issues.Exists(i => i.Severity == IssueSeverity.Warning && i.Message.Contains("권장")), "5.5m는 짧다는 경고");
            StageJson.Write(s);
            Assert.AreEqual(StageDefinition.CurrentSchemaVersion, s.schemaVersion, "저장하면 v2");
        }

        [Test]
        public void GraphLifetime_ReachesPlayer_OnWideMaps_Capped()
        {
            var d = ScriptableObject.CreateInstance<GraphPatternDefinition>();
            try
            {
                d.lifetime = 3f; d.speed = 2.4f;
                Assert.AreEqual(3f, GraphAttackPattern.ReachLifetime(d, 3f, 4f), 1e-4f, "좁은 맵은 에셋 수명 그대로");
                Assert.AreEqual(22f / 2.4f, GraphAttackPattern.ReachLifetime(d, 20f, 12f), 1e-3f, "적-주인공 20m + 2m까지");
                Assert.AreEqual(GraphAttackPattern.MaxReachLifetime, GraphAttackPattern.ReachLifetime(d, 60f, 12f), 1e-4f, "최대 10초");
                d.trajectory = TrajectoryKind.Parabola;
                Assert.AreEqual(3f, GraphAttackPattern.ReachLifetime(d, 20f, 12f), 1e-4f, "진행률로 모양이 정해지는 궤적은 그대로");
            }
            finally { Object.DestroyImmediate(d); }
        }

        [Test]
        public void Enemies_HaveOwnMoves_AndMoreHealth()
        {
            foreach (var name in new[] { "Enemy_Test", "Enemy_Test2" })
            {
                var e = AssetDatabase.LoadAssetAtPath<EnemyData>($"Assets/_Project/Data/Enemies/{name}.asset");
                Assert.Greater(e.SignatureMoves.Count, 0, name);
                Assert.Greater(e.PhaseMoves.Count, 0, name);
                Assert.GreaterOrEqual(e.MaxHp, 70, $"{name}: 공격 4~6번 (평균 피해 약 14)");
                foreach (var m in e.SignatureMoves) Assert.IsNotNull(m.PatternPrefab, m.name);
            }
            var a = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/Enemy_Test.asset");
            var b = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/Enemy_Test2.asset");
            CollectionAssert.AreNotEquivalent(a.SignatureMoves, b.SignatureMoves, "적마다 다른 기술");
        }
    }
}
