using System.Collections.Generic;
using System.Linq;
using Game2Week.Battle;
using Game2Week.Data;
using Game2Week.Rendering;
using Game2Week.Stages;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game2Week.Tests
{
    public class GemTests
    {
        static List<StageGem> Gems(int count, float chance) =>
            Enumerable.Range(1, count).Select(i => new StageGem { id = $"gem_{i:00}", spawnChance = chance, position = new GridPoint(i, 5) }).ToList();

        [Test]
        public void Roll_NeverExceedsMaxPerTurn()
        {
            var field = new GemField(Gems(6, 1f), maxPerTurn: 2, new System.Random(1));
            for (int i = 0; i < 20; i++) Assert.AreEqual(2, field.RollForTurn().Count);
        }

        [Test]
        public void Roll_ZeroChanceOrZeroMax_ShowsNothing()
        {
            Assert.IsEmpty(new GemField(Gems(3, 0f), 1, new System.Random(1)).RollForTurn());
            Assert.IsEmpty(new GemField(Gems(3, 1f), 0, new System.Random(1)).RollForTurn());
        }

        [Test]
        public void Roll_LowChance_IsRare()
        {
            var field = new GemField(Gems(1, 0.25f), 1, new System.Random(7));
            int shown = 0;
            for (int i = 0; i < 2000; i++) shown += field.RollForTurn().Count;
            Assert.That(shown / 2000f, Is.InRange(0.2f, 0.3f));
        }

        [Test]
        public void Collect_OnlyActiveGems_AndCollectedNeverReturns()
        {
            var field = new GemField(Gems(1, 1f), 1, new System.Random(1));
            Assert.IsNull(field.Collect("gem_01"), "아직 안 나온 보석은 못 먹음");

            field.RollForTurn();
            Assert.AreEqual("gem_01", field.Collect("gem_01").id);
            Assert.AreEqual(1, field.CollectedCount);

            for (int i = 0; i < 10; i++) Assert.IsEmpty(field.RollForTurn());
        }

        [Test]
        public void EndTurn_HidesUncollectedGems_ButTheyCanReturn()
        {
            var field = new GemField(Gems(1, 1f), 1, new System.Random(1));
            field.RollForTurn();
            field.EndTurn();

            Assert.IsEmpty(field.Active);
            Assert.IsNull(field.Collect("gem_01"));
            Assert.AreEqual(1, field.RollForTurn().Count);
        }

        // ---------- 보상 ----------

        readonly List<Object> created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in created) Object.DestroyImmediate(o);
            created.Clear();
        }

        T Create<T>(System.Action<SerializedObject> setup = null) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            created.Add(asset);
            if (setup != null)
            {
                var so = new SerializedObject(asset);
                setup(so);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            return asset;
        }

        [Test]
        public void Rewards_ApplyByType()
        {
            var settings = Create<GemRewardSettings>();
            var player = new PlayerCombatant(Create<PlayerData>(so => so.FindProperty("maxHp").intValue = 20));
            var enemy = new EnemyCombatant(Create<EnemyData>(so => so.FindProperty("spareThreshold").intValue = 1));
            player.TakeDamage(10);

            StringAssert.Contains("5", GemRewards.Apply(GemTypes.Heal, settings, player, enemy));
            Assert.AreEqual(15, player.CurrentHp);

            GemRewards.Apply(GemTypes.Attack, settings, player, enemy);
            Assert.AreEqual(1.5f, player.NextAttackMultiplier);

            GemRewards.Apply(GemTypes.Spare, settings, player, enemy);
            Assert.IsTrue(enemy.CanBeSpared);
        }

        [Test]
        public void AttackBoost_DoesNotStack_AndIsConsumedOnce()
        {
            var player = new PlayerCombatant(Create<PlayerData>());
            player.GrantAttackBoost(1.5f);
            player.GrantAttackBoost(1.2f);

            Assert.AreEqual(1.5f, player.ConsumeAttackMultiplier());
            Assert.AreEqual(1f, player.ConsumeAttackMultiplier());
        }

        // ---------- 로우폴리 메시 ----------

        [Test]
        public void LowPolyMeshes_HaveExpectedTriangleCounts()
        {
            var meshes = new[]
            {
                (LowPolyMesh.Icosphere(1f, 0), 20),
                (LowPolyMesh.Icosphere(1f, 1), 80),
                (LowPolyMesh.Octahedron(1f, 1f), 8),
                (LowPolyMesh.Cone(1f, 1f, 6), 6),
            };
            foreach (var (mesh, tris) in meshes)
            {
                Assert.AreEqual(tris, mesh.triangles.Length / 3, mesh.name);
                Assert.AreEqual(mesh.vertexCount, mesh.triangles.Length, "면마다 정점이 따로 있어야 각진 음영");
                Object.DestroyImmediate(mesh);
            }
        }
    }
}
