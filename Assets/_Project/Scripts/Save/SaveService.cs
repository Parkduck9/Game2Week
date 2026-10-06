using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game2Week.Save
{
    /// <summary>
    /// 진행·설정 저장소. 파일 위치는 생성자에서 받는다 (게임: persistentDataPath, 테스트: 임시 폴더).
    /// MSIX 스토어 앱에서는 이 폴더가 패키지 전용으로 옮겨져 앱 삭제 시 함께 지워진다.
    /// </summary>
    public sealed class SaveService
    {
        public const string SaveFileName = "save.json";
        public const string SettingsFileName = "settings.json";

        readonly string directory;
        readonly Action<string> warn;

        public SaveService(string directory, Action<string> warn = null)
        {
            this.directory = directory ?? throw new ArgumentNullException(nameof(directory));
            this.warn = warn ?? (m => Debug.LogWarning("[Save] " + m));
            Progress = JsonFileStore.Read<SaveData>(SavePath, d => d.version == SaveData.CurrentVersion, this.warn) ?? new SaveData();
            Settings = JsonFileStore.Read<SettingsData>(SettingsPath, d => d.version == SettingsData.CurrentVersion, this.warn) ?? new SettingsData();
        }

        public static string DefaultDirectory => Application.persistentDataPath;

        public string SavePath => Path.Combine(directory, SaveFileName);
        public string SettingsPath => Path.Combine(directory, SettingsFileName);

        public SaveData Progress { get; private set; }
        public SettingsData Settings { get; }

        /// <summary>이어하기 가능 여부 — 한 번이라도 클리어했으면 true</summary>
        public bool HasProgress => Progress.stages.Exists(s => s.cleared);

        public StageRecord Find(string stageId) => Progress.stages.Find(s => s.id == stageId);

        /// <summary>
        /// 해금된 스테이지 수 (진행 순서 기준). 1번은 항상 열려 있고, 클리어한 스테이지 다음 하나까지 열린다.
        /// 해금 상태를 따로 저장하지 않으므로 스테이지가 추가돼도 자연스럽게 이어진다.
        /// </summary>
        public int UnlockedCount(IReadOnlyList<string> stageOrder)
        {
            if (stageOrder.Count == 0) return 0;
            int unlocked = 1;
            for (int i = 0; i < stageOrder.Count - 1; i++)
            {
                if (Find(stageOrder[i])?.cleared != true) break;
                unlocked = i + 2;
            }
            return unlocked;
        }

        /// <summary>클리어 기록. 더 빠르면 최고기록 갱신 → true. 즉시 저장한다.</summary>
        public bool RecordClear(string stageId, float timeSeconds, string outcome)
        {
            var record = Find(stageId);
            if (record == null)
            {
                record = new StageRecord { id = stageId };
                Progress.stages.Add(record);
            }

            bool newRecord = record.bestTimeSeconds <= 0f || timeSeconds < record.bestTimeSeconds;
            record.cleared = true;
            if (newRecord)
            {
                record.bestTimeSeconds = timeSeconds;
                record.bestOutcome = outcome;
            }
            SaveProgress();
            return newRecord;
        }

        public void MarkEndingSeen()
        {
            Progress.endingSeen = true;
            SaveProgress();
        }

        /// <summary>새로 시작: 진행만 지우고 설정은 남긴다.</summary>
        public void ResetProgress()
        {
            Progress = new SaveData();
            SaveProgress();
        }

        public void SaveProgress() => JsonFileStore.Write(SavePath, Progress);

        public void SaveSettings() => JsonFileStore.Write(SettingsPath, Settings);

        public static string FormatTime(float seconds)
        {
            if (seconds <= 0f) return "-";
            int minutes = (int)(seconds / 60f);
            return $"{minutes}:{seconds - minutes * 60f:00.0}";
        }
    }
}
