using Game2Week.Battle.Patterns;
using Game2Week.Data;
using Game2Week.Data.Patterns;
using Game2Week.EditorTools.Patterns;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game2Week.Tests
{
    public class SequenceTests
    {
        AttackSequence Sample() => AssetDatabase.LoadAssetAtPath<AttackSequence>(SequenceAssetStore.Root + "/고리_부채저격.asset");
        [Test]
        public void Schedule_StableSeeds_Times_AndDifficulty()
        {
            var a = SequenceSchedule.Build(Sample()); var b = SequenceSchedule.Build(Sample());
            Assert.AreEqual(4, a.Count);
            for (int i = 0; i < a.Count; i++) { Assert.AreEqual(a[i].Seed, b[i].Seed); Assert.AreEqual(i * 1.2f, a[i].Time, .0001f); }
            Assert.AreNotEqual(a[0].Seed, a[1].Seed);
            var scaled = SequenceSchedule.Build(Sample(), .5f);
            Assert.AreEqual(.6f, scaled[1].Time, .0001f);
            Assert.AreEqual(3, scaled[0].Duration);
        }
        [Test]
        public void Rules_RejectInvalidTiming_AndRedBlue_Nesting()
        {
            var copy = Object.Instantiate(Sample());
            try
            {
                copy.entries[0].time = float.NaN;
                Assert.IsNotEmpty(SequenceSchedule.Validate(copy));
                copy.entries[0].time = 0;
                copy.entries[0].pattern = AssetDatabase.LoadAssetAtPath<AttackPatternData>(SequenceAssetStore.Root + "/Attack_고리_부채저격.asset");
                Assert.IsNotEmpty(SequenceSchedule.Validate(copy));
                var all = AssetDatabase.FindAssets("t:AttackPatternData");
                AttackPatternData red = null, blue = null;
                foreach (var id in all)
                {
                    var pattern = AssetDatabase.LoadAssetAtPath<AttackPatternData>(AssetDatabase.GUIDToAssetPath(id));
                    if (!pattern.PatternPrefab || !pattern.PatternPrefab.TryGetComponent<IDirectablePattern>(out var directed)) continue;
                    if (directed.PatternColor == AttackColor.Red) red = pattern;
                    if (directed.PatternColor == AttackColor.Blue) blue = pattern;
                }
                Assert.IsNotNull(red); Assert.IsNotNull(blue);
                copy.entries[0].pattern = red; copy.entries[1].pattern = blue;
                Assert.That(SequenceSchedule.Validate(copy), Has.Some.Contains("빨강"));
            }
            finally { Object.DestroyImmediate(copy); }
        }
        [Test]
        public void Sample_Registered_AndEnemyPhaseMove()
        {
            var attack = AssetDatabase.LoadAssetAtPath<AttackPatternData>(SequenceAssetStore.Root + "/Attack_고리_부채저격.asset");
            var enemy = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/Enemy_Test2.asset");
            CollectionAssert.Contains(enemy.PhaseMoves, attack);
            Assert.AreEqual(AttackColor.Yellow, SequenceSchedule.Color(Sample()));
            Assert.IsEmpty(SequenceSchedule.Validate(Sample()));
        }
    }
}
