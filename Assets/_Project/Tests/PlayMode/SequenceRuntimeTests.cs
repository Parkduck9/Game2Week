using System.Collections;
using System.Collections.Generic;
using Game2Week.Battle.Patterns;
using Game2Week.Battle.Patterns.Director;
using Game2Week.Data;
using Game2Week.Data.Patterns;
using Game2Week.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game2Week.Tests
{
    public class SequenceRuntimeTests
    {
        [SetUp] public void SetUp() => TestSave.Begin();
        [TearDown] public void TearDown() { Time.timeScale = 1; TestSave.End(); }
        [UnityTest]
        public IEnumerator Director_Sequence_FiresRepeatedly_CollectsThreats_AndCleansUp()
        {
#if UNITY_EDITOR
            BattleController battle = null;
            yield return SceneFlowTests.EnterStage(0, value => battle = value);
            var attack = AssetDatabase.LoadAssetAtPath<AttackPatternData>("Assets/_Project/Data/Patterns/Sequences/Attack_고리_부채저격.asset");
            var encounter = ScriptableObject.CreateInstance<PatternEncounterData>();
            var profile = ScriptableObject.CreateInstance<DifficultyProfile>();
            encounter.newPattern = attack; encounter.profile = profile;
            var root = new GameObject("시퀀스 검증"); var director = root.AddComponent<PatternDirector>(); director.Configure(encounter);
            var context = new PatternContext(battle.Spawner.Arena, battle.Spawner.Enemy.transform, battle.Spawner.Enemy,
                battle.World.Player.transform, .2f, 4, _ => { });
            int shots = 0; context.Feedback.BulletFired += (_, _) => shots++;
            director.Begin(context);
            var sequence = root.GetComponentInChildren<SequencePattern>(); Assert.IsNotNull(sequence);
            Assert.AreEqual(1, sequence.ActiveChildren);
            director.Tick(1.21f); Assert.AreEqual(2, sequence.ActiveChildren);
            director.Tick(2.5f); Assert.AreEqual(4, sequence.ActiveChildren);
            director.Tick(.6f); Assert.AreEqual(21, shots, "고리 12발 + 부채꼴 3발 세 번");
            var threats = new List<ThreatPoint>(); sequence.CollectThreats(threats); Assert.IsNotEmpty(threats);
            SceneCapture.Save("sequence_runtime");
            director.End(); Assert.AreEqual(0, sequence.ActiveChildren);
            yield return null; Assert.IsTrue(sequence == null);
            Object.Destroy(root); Object.Destroy(encounter); Object.Destroy(profile);
#else
            yield return null;
#endif
        }
    }
}

