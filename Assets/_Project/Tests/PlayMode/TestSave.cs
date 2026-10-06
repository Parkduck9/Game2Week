using System.IO;
using Game2Week.Core;
using UnityEngine;

namespace Game2Week.Tests
{
    /// <summary>PlayMode 테스트가 실제 세이브(persistentDataPath)를 건드리지 않게 임시 폴더로 바꾼다.</summary>
    public static class TestSave
    {
        public static string Directory { get; private set; }

        public static void Begin()
        {
            Directory = Path.Combine(Path.GetTempPath(), "Game2WeekPlayMode_" + System.Guid.NewGuid().ToString("N"));
            GameSession.SaveDirectoryOverrideForTests = Directory;
            Time.timeScale = 1f;
        }

        public static void End()
        {
            GameSession.SaveDirectoryOverrideForTests = null;
            Time.timeScale = 1f;
            if (Directory != null && System.IO.Directory.Exists(Directory)) System.IO.Directory.Delete(Directory, true);
            Directory = null;
        }
    }
}
