using System.Collections.Generic;
using UnityEngine;

namespace Game2Week.Data.Patterns
{
    /// <summary>
    /// 맵 하나의 패턴 구성 (5단계): 이 맵에서 새로 배우는 패턴 1개 + 이미 배운 패턴들 + 난이도.
    /// PatternDirector가 읽는다. 맵(스테이지 JSON)에는 이 구성을 쓰는 Director의 AttackPatternData 이름을 넣는다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game2Week/Pattern Encounter", fileName = "Encounter_")]
    public sealed class PatternEncounterData : ScriptableObject
    {
        [Tooltip("이 맵에서 처음 보여 주는 패턴 — 첫 턴에 단독으로")]
        public AttackPatternData newPattern;
        [Tooltip("앞 맵에서 배운 패턴들")]
        public List<AttackPatternData> knownPatterns = new();
        public DifficultyProfile profile;
        [Tooltip("셔플 순서 재현용")]
        public int seed = 1201;

        /// <summary>0번 = 새 패턴, 1번부터 배운 패턴</summary>
        public List<AttackPatternData> All()
        {
            var all = new List<AttackPatternData>();
            if (newPattern) all.Add(newPattern);
            foreach (var p in knownPatterns) if (p && !all.Contains(p)) all.Add(p);
            return all;
        }
    }
}
