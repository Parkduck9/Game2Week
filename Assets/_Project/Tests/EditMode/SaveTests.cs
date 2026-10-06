using System.IO;
using Game2Week.Save;
using Game2Week.UI;
using NUnit.Framework;
using UnityEngine;

namespace Game2Week.Tests
{
    public class SaveTests
    {
        string dir;
        static readonly string[] Order = { "stage_001", "stage_002", "stage_003" };

        [SetUp]
        public void SetUp() => dir = Path.Combine(Path.GetTempPath(), "Game2WeekSave_" + System.Guid.NewGuid().ToString("N"));

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        SaveService NewService() => new(dir, _ => { });

        [Test]
        public void Fresh_HasNoProgress_AndOnlyFirstStageUnlocked()
        {
            var save = NewService();
            Assert.IsFalse(save.HasProgress);
            Assert.AreEqual(1, save.UnlockedCount(Order));
        }

        [Test]
        public void ClearingUnlocksNext_UpToLast()
        {
            var save = NewService();
            save.RecordClear("stage_001", 30f, "EnemySpared");
            Assert.AreEqual(2, save.UnlockedCount(Order));
            save.RecordClear("stage_002", 30f, "EnemySpared");
            save.RecordClear("stage_003", 30f, "EnemySpared");
            Assert.AreEqual(3, save.UnlockedCount(Order));
            Assert.IsTrue(save.HasProgress);
        }

        [Test]
        public void BestTime_OnlyImproves_AndKeepsItsOutcome()
        {
            var save = NewService();
            Assert.IsTrue(save.RecordClear("stage_001", 40f, "EnemyDefeated"));
            Assert.IsFalse(save.RecordClear("stage_001", 50f, "EnemySpared"));
            Assert.IsTrue(save.RecordClear("stage_001", 35f, "EnemySpared"));

            var record = save.Find("stage_001");
            Assert.AreEqual(35f, record.bestTimeSeconds);
            Assert.AreEqual("EnemySpared", record.bestOutcome);
        }

        [Test]
        public void ProgressAndSettings_PersistAcrossRestart()
        {
            var save = NewService();
            save.RecordClear("stage_001", 12.5f, "EnemySpared");
            save.MarkEndingSeen();
            save.Settings.textSpeed = TextSpeeds.Fast;
            save.Settings.fullscreen = false;
            save.SaveSettings();

            var reopened = NewService();
            Assert.AreEqual(12.5f, reopened.Find("stage_001").bestTimeSeconds);
            Assert.IsTrue(reopened.Progress.endingSeen);
            Assert.AreEqual(TextSpeeds.Fast, reopened.Settings.textSpeed);
            Assert.IsFalse(reopened.Settings.fullscreen);
        }

        [Test]
        public void ResetProgress_KeepsSettings()
        {
            var save = NewService();
            save.RecordClear("stage_001", 10f, "EnemySpared");
            save.Settings.bgmVolume = 0.3f;
            save.SaveSettings();

            save.ResetProgress();

            var reopened = NewService();
            Assert.IsFalse(reopened.HasProgress);
            Assert.AreEqual(0.3f, reopened.Settings.bgmVolume, 0.001f);
        }

        [Test]
        public void CorruptedSave_RecoversFromBackup()
        {
            var save = NewService();
            save.RecordClear("stage_001", 10f, "EnemySpared");
            save.RecordClear("stage_002", 20f, "EnemySpared"); // 두 번째 저장 → 첫 번째가 .bak
            File.WriteAllText(save.SavePath, "{ broken");

            var reopened = NewService();
            Assert.IsTrue(reopened.Find("stage_001").cleared, "백업에서 복구");
        }

        [Test]
        public void CorruptedWithoutBackup_StartsFresh()
        {
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, SaveService.SaveFileName), "nope");
            Assert.IsFalse(NewService().HasProgress);
        }

        [Test]
        public void FormatTime()
        {
            Assert.AreEqual("-", SaveService.FormatTime(0f));
            Assert.AreEqual("0:41.3", SaveService.FormatTime(41.3f));
            Assert.AreEqual("1:05.0", SaveService.FormatTime(65f));
        }

        // ---------- 설정 ----------

        static readonly Vector2Int[] Resolutions = { new(1280, 720), new(1920, 1080), new(2560, 1440) };

        [Test]
        public void Settings_VolumeStepsAndClamps()
        {
            var data = new SettingsData { bgmVolume = 0.9f };
            var model = new SettingsModel(data, Resolutions);

            Assert.IsTrue(model.Change(SettingRow.BgmVolume, 1, out _));
            Assert.AreEqual(1f, data.bgmVolume, 0.001f);
            Assert.IsFalse(model.Change(SettingRow.BgmVolume, 1, out _), "100%에서 더 안 올라감");
            StringAssert.Contains("100%", model.Label(SettingRow.BgmVolume));
        }

        [Test]
        public void Settings_DisplayAndResolution_ReportDisplayChange()
        {
            var data = new SettingsData { fullscreen = true, resolutionWidth = 1920, resolutionHeight = 1080 };
            var model = new SettingsModel(data, Resolutions);

            model.Change(SettingRow.DisplayMode, 1, out bool displayChanged);
            Assert.IsTrue(displayChanged);
            Assert.IsFalse(data.fullscreen);

            model.Change(SettingRow.Resolution, 1, out displayChanged);
            Assert.IsTrue(displayChanged);
            Assert.AreEqual(2560, data.resolutionWidth);
            Assert.IsFalse(model.Change(SettingRow.Resolution, 1, out _), "가장 큰 해상도에서 멈춤");
        }

        [Test]
        public void Settings_TextSpeedCycle()
        {
            var data = new SettingsData();
            var model = new SettingsModel(data, Resolutions);

            model.Change(SettingRow.TextSpeed, -1, out _);
            Assert.AreEqual(TextSpeeds.Slow, data.textSpeed);
            Assert.Less(TextSpeeds.Multiplier(TextSpeeds.Slow), TextSpeeds.Multiplier(TextSpeeds.Fast));
        }

        // ---------- 비활성 메뉴 항목 ----------

        [Test]
        public void MenuList_SkipsDisabledItems()
        {
            var list = new MenuList();
            list.SetCount(4, new[] { 1 });

            Assert.IsTrue(list.Move(1));
            Assert.AreEqual(2, list.Index, "1번(이어하기) 건너뜀");
            list.Move(-1);
            Assert.AreEqual(0, list.Index);

            list.Select(1);
            Assert.AreEqual(0, list.Index, "비활성 항목은 선택 안 됨");
        }

        [Test]
        public void MenuList_ResetPicksFirstEnabled()
        {
            var list = new MenuList();
            list.SetCount(3, new[] { 0 });
            Assert.AreEqual(1, list.Index);
        }
    }
}
