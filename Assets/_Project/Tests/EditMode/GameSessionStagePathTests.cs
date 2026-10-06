using System;
using System.IO;
using Game2Week.Core;
using Game2Week.Stages;
using NUnit.Framework;
using UnityEngine;

namespace Game2Week.Tests
{
    public class GameSessionStagePathTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void UnsetOverride_LoadsProjectStages(string directory)
        {
            var session = ScriptableObject.CreateInstance<GameSession>();
            try
            {
                session.StageDirectoryOverride = directory;
                var expected = new StageRepository(StageRepository.DefaultDirectory).LoadIndex().stages;
                Assert.Greater(expected.Count, 0);
                CollectionAssert.AreEqual(expected, session.StageIds);
                Assert.IsTrue(session.BeginStage(0));
            }
            finally { UnityEngine.Object.DestroyImmediate(session); }
        }

        [Test]
        public void ExplicitOverride_StillUsesSelectedDirectory()
        {
            var session = ScriptableObject.CreateInstance<GameSession>();
            try
            {
                session.StageDirectoryOverride = Path.Combine(Path.GetTempPath(), "UnusedStages_" + Guid.NewGuid().ToString("N"));
                Assert.AreEqual(0, session.StageCount);
            }
            finally { UnityEngine.Object.DestroyImmediate(session); }
        }
    }
}
