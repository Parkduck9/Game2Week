using System;
using System.Collections.Generic;

namespace Game2Week.Save
{
    // JsonUtility 직렬화용. 필드 이름 = JSON 키 — 바꾸면 version을 올리고 변환 코드를 둔다.

    /// <summary>진행 저장 (save.json). "새로 시작"하면 이것만 초기화된다.</summary>
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public List<StageRecord> stages = new();
        public bool endingSeen;
    }

    [Serializable]
    public sealed class StageRecord
    {
        public string id;
        public bool cleared;
        /// <summary>가장 빠른 클리어 시간 (초). 0이면 기록 없음</summary>
        public float bestTimeSeconds;
        /// <summary>최고기록을 세운 판의 결과 (BattleOutcome 이름: EnemyDefeated / EnemySpared)</summary>
        public string bestOutcome = string.Empty;
    }

    /// <summary>설정 저장 (settings.json). 진행과 따로 저장해서 "새로 시작"해도 남는다.</summary>
    [Serializable]
    public sealed class SettingsData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public float bgmVolume = 0.8f;
        public float sfxVolume = 0.8f;
        public bool fullscreen = true;
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        /// <summary><see cref="TextSpeeds"/> 중 하나</summary>
        public string textSpeed = TextSpeeds.Normal;
    }

    public static class TextSpeeds
    {
        public const string Slow = "slow";
        public const string Normal = "normal";
        public const string Fast = "fast";

        public static readonly IReadOnlyList<string> All = new[] { Slow, Normal, Fast };

        /// <summary>대사 출력 속도 배율</summary>
        public static float Multiplier(string speed) => speed switch
        {
            Slow => 0.6f,
            Fast => 1.8f,
            _ => 1f,
        };
    }
}
