using System.Collections.Generic;
using Game2Week.Battle;
using Game2Week.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game2Week.Tests
{
    public class CombatantTests
    {
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

        static void SetList<T>(SerializedObject so, string field, IList<T> values) where T : Object
        {
            var prop = so.FindProperty(field);
            prop.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        PlayerData Player(int maxHp, params ItemData[] items) => Create<PlayerData>(so =>
        {
            so.FindProperty("maxHp").intValue = maxHp;
            SetList(so, "startingItems", items);
        });

        EnemyData Enemy(int maxHp, int spareThreshold, PatternOrder order = PatternOrder.Sequential, params AttackPatternData[] patterns) => Create<EnemyData>(so =>
        {
            so.FindProperty("maxHp").intValue = maxHp;
            so.FindProperty("spareThreshold").intValue = spareThreshold;
            so.FindProperty("patternOrder").enumValueIndex = (int)order;
            SetList(so, "attackPatterns", patterns);
        });

        [Test]
        public void Player_DamageAndHeal_AreClampedToHpRange()
        {
            var player = new PlayerCombatant(Player(20));

            Assert.AreEqual(15, player.TakeDamage(15));
            Assert.AreEqual(5, player.TakeDamage(15));
            Assert.IsTrue(player.IsDefeated);
            Assert.AreEqual(0, player.TakeDamage(-3));

            Assert.AreEqual(20, player.Heal(50));
            Assert.AreEqual(20, player.CurrentHp);
            Assert.AreEqual(0, player.Heal(5));
        }

        [Test]
        public void Player_StartsWithStartingItems_AndItemsAreConsumed()
        {
            var a = Create<ItemData>();
            var b = Create<ItemData>();
            var player = new PlayerCombatant(Player(20, a, b));

            Assert.AreEqual(2, player.Inventory.Items.Count);
            Assert.AreSame(b, player.Inventory.Take(1));
            Assert.AreSame(a, player.Inventory.Take(0));
            Assert.IsTrue(player.Inventory.IsEmpty);
            Assert.Throws<System.ArgumentOutOfRangeException>(() => player.Inventory.Take(0));
        }

        [Test]
        public void Enemy_CanBeSpared_AfterReachingThreshold()
        {
            var enemy = new EnemyCombatant(Enemy(30, spareThreshold: 2));

            Assert.IsFalse(enemy.CanBeSpared);
            enemy.AddSpareProgress(1);
            Assert.IsFalse(enemy.CanBeSpared);
            enemy.AddSpareProgress(1);
            Assert.IsTrue(enemy.CanBeSpared);
        }

        [Test]
        public void Enemy_WithZeroThreshold_CanBeSparedImmediately()
        {
            Assert.IsTrue(new EnemyCombatant(Enemy(30, spareThreshold: 0)).CanBeSpared);
        }

        [Test]
        public void Enemy_TakeDamage_StopsAtZero()
        {
            var enemy = new EnemyCombatant(Enemy(30, 2));

            Assert.AreEqual(30, enemy.TakeDamage(99));
            Assert.IsTrue(enemy.IsDefeated);
        }

        [Test]
        public void Enemy_SequentialPatterns_Cycle()
        {
            var p1 = Create<AttackPatternData>();
            var p2 = Create<AttackPatternData>();
            var enemy = new EnemyCombatant(Enemy(30, 2, PatternOrder.Sequential, p1, p2));

            Assert.AreSame(p1, enemy.NextPattern());
            Assert.AreSame(p2, enemy.NextPattern());
            Assert.AreSame(p1, enemy.NextPattern());
        }

        [Test]
        public void Enemy_RandomPatterns_StayInList()
        {
            var p1 = Create<AttackPatternData>();
            var p2 = Create<AttackPatternData>();
            var enemy = new EnemyCombatant(Enemy(30, 2, PatternOrder.Random, p1, p2), new System.Random(42));

            for (int i = 0; i < 20; i++)
            {
                var p = enemy.NextPattern();
                Assert.IsTrue(p == p1 || p == p2);
            }
        }

        [Test]
        public void Enemy_WithoutPatterns_ReturnsNull()
        {
            Assert.IsNull(new EnemyCombatant(Enemy(30, 2)).NextPattern());
        }
    }
}
