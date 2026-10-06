using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game2Week.Data
{
    public enum PatternOrder
    {
        Sequential,
        Random,
    }

    /// <summary>ACT 메뉴의 행동 하나. Check는 모든 적에게 자동으로 붙으므로 여기에 넣지 않는다.</summary>
    [Serializable]
    public sealed class ActOption
    {
        [SerializeField] string displayName = "행동";
        [SerializeField, TextArea] string resultText = "* ...";
        [Tooltip("살려주기 진행도 증가량. 적의 Spare Threshold 이상이 되면 살려줄 수 있다.")]
        [SerializeField, Min(0)] int spareProgress = 1;

        public ActOption() { }

        public ActOption(string displayName, string resultText, int spareProgress)
        {
            this.displayName = displayName;
            this.resultText = resultText;
            this.spareProgress = spareProgress;
        }

        public string DisplayName => displayName;
        public string ResultText => resultText;
        public int SpareProgress => spareProgress;
    }

    [CreateAssetMenu(menuName = "Game2Week/Enemy", fileName = "Enemy_")]
    public sealed class EnemyData : ScriptableObject
    {
        [Header("기본")]
        [SerializeField] string displayName = "적";
        [Tooltip("3D 외형 프리팹 (05단계 EnemyView)")]
        [SerializeField] GameObject viewPrefab;
        [SerializeField, Min(1)] int maxHp = 30;
        [SerializeField, Min(0)] int attack = 4;
        [SerializeField, Min(0)] int defense;

        [Header("대사")]
        [SerializeField, TextArea] string checkText = "* 설명";
        [SerializeField, TextArea] string encounterText = "* 적이 나타났다!";
        [Tooltip("플레이어 턴마다 박스에 나오는 문구 (순서대로 반복)")]
        [SerializeField, TextArea] List<string> flavorTexts = new();
        [Tooltip("적 턴 시작 시 말풍선 (순서대로 반복)")]
        [SerializeField, TextArea] List<string> enemyTurnLines = new();
        [SerializeField, TextArea] string spareText = "* 적을 살려 주었다.";
        [SerializeField, TextArea] string defeatText = "* 적을 쓰러뜨렸다.";

        [Header("행동 / 살려주기")]
        [SerializeField] List<ActOption> acts = new();
        [Tooltip("살려주기 진행도가 이 값 이상이면 살려줄 수 있다. 0이면 처음부터 가능.")]
        [SerializeField, Min(0)] int spareThreshold = 2;

        [Header("공격")]
        [SerializeField] List<AttackPatternData> attackPatterns = new();
        [SerializeField] PatternOrder patternOrder = PatternOrder.Sequential;

        public string DisplayName => displayName;
        public GameObject ViewPrefab => viewPrefab;
        public int MaxHp => maxHp;
        public int Attack => attack;
        public int Defense => defense;
        public string CheckText => checkText;
        public string EncounterText => encounterText;
        public IReadOnlyList<string> FlavorTexts => flavorTexts;
        public IReadOnlyList<string> EnemyTurnLines => enemyTurnLines;
        public string SpareText => spareText;
        public string DefeatText => defeatText;
        public IReadOnlyList<ActOption> Acts => acts;
        public int SpareThreshold => spareThreshold;
        public IReadOnlyList<AttackPatternData> AttackPatterns => attackPatterns;
        public PatternOrder PatternOrder => patternOrder;
    }
}
