using Game2Week.Data.Patterns;
using Game2Week.EditorTools.Patterns;
using Game2Week.Battle.Patterns.Trajectories;
using Game2Week.EditorTools.Stages;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game2Week.Tests
{
    public class PatternEditorAssetTests
    {
        GraphPatternDefinition created,copy;
        [TearDown] public void Cleanup(){if(copy)PatternAssetStore.Delete(copy);if(created)PatternAssetStore.Delete(created);}
        [Test] public void CreateCloneSaveDelete_RegistersPrefabAndCatalog()
        {
            created=PatternAssetStore.Create("TestPattern_"+System.Guid.NewGuid().ToString("N"));
            created.trajectory=TrajectoryKind.Bezier;created.seed=123;var attack=PatternAssetStore.Save(created);
            Assert.IsNotNull(attack.PatternPrefab.GetComponent<GraphAttackPattern>());
            Assert.AreSame(attack,ContentCatalogUtility.LoadOrCreate().FindPattern(attack.name));
            copy=PatternAssetStore.Create(created.name+"_Copy",created);Assert.AreEqual(123,copy.seed);Assert.AreEqual(TrajectoryKind.Bezier,copy.trajectory);
            created.speed=-1;Assert.Throws<System.InvalidOperationException>(()=>PatternAssetStore.Save(created));created.speed=2;
            string path=AssetDatabase.GetAssetPath(copy);PatternAssetStore.Delete(copy);copy=null;Assert.IsNull(AssetDatabase.LoadAssetAtPath<GraphPatternDefinition>(path));
        }
    }
}
