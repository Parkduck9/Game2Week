using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game2Week.Data.Patterns
{
    /// <summary>
    /// 적 체력 단계 (9단계). 적 체력 비율이 hpBelow 이하로 내려가면 이 단계가 된다 — 겹 수가 늘고 간격·탄속이 바뀐다.
    /// </summary>
    [Serializable]
    public sealed class DifficultyPhase
    {
        [Tooltip("적 체력 비율이 이 값 이하이면 이 단계 (0~1)")]
        [Range(0f, 1f)] public float hpBelow = 0.5f;
        [Tooltip("동시에 돌리는 패턴 수 추가 (최대 " + nameof(DifficultyProfile.MaxLayers) + "까지)")]
        [Min(0)] public int extraLayers = 1;
        [Tooltip("발사 간격 배율 (1보다 작으면 더 자주)")]
        [Min(0.1f)] public float intervalMultiplier = 0.85f;
        [Tooltip("탄속 배율")]
        [Min(0.1f)] public float speedMultiplier = 1.05f;
    }

    /// <summary>
    /// 맵 단계별 난이도 (5단계 → 9단계 겹·체력 단계). 하위 패턴의 기본 발사 간격 × (기준 간격 / 1.40), 탄속 × 배율을 한 번만 적용한다.
    /// 동시 실행 상한(겹 수)과 살아 있는 위험 상한(공유 예산)도 여기서 정한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game2Week/Difficulty Profile", fileName = "Profile_")]
    public sealed class DifficultyProfile : ScriptableObject
    {
        /// <summary>간격 배율의 기준 — 1단계 원형 기준 간격</summary>
        public const float BaseReferenceInterval = 1.40f;
        /// <summary>한 턴에 동시에 돌릴 수 있는 패턴 수의 절대 상한 (주 공격 + 방해 + 저격)</summary>
        public const int MaxLayers = 3;

        [Tooltip("이 단계의 기준 발사 간격 (초). 1.40이면 패턴 기본값 그대로")]
        [Min(0.1f)] public float referenceInterval = BaseReferenceInterval;
        [Tooltip("탄속 배율")]
        [Min(0.1f)] public float speedScale = 1f;
        [Tooltip("한 턴에 동시에 돌릴 수 있는 패턴 수 (1~3)")]
        [Range(1, MaxLayers)] public int maxSimultaneous = 1;
        [Tooltip("살아 있는 위험(예고 + 탄) 수가 이 이상이면 다음 겹 시작을 미룬다")]
        [Min(1)] public int liveThreatCap = 8;
        [Tooltip("두 번째 패턴을 시작하기까지 최소 시간 (초)")]
        [Min(0f)] public float secondPatternDelay = 1.6f;
        [Tooltip("세 번째 패턴을 시작하기까지 최소 시간 (초)")]
        [Min(0f)] public float thirdPatternDelay = 3.2f;
        [Tooltip("적 체력 단계 — hpBelow가 큰 것부터 (예: 0.6, 0.3)")]
        public List<DifficultyPhase> phases = new();

        public float IntervalScale => referenceInterval / BaseReferenceInterval;

        /// <summary>적 체력 비율에 맞는 단계 번호 (0 = 기본, 1 = phases[0] …). 조건을 만족하는 가장 깊은 단계.</summary>
        public int PhaseFor(float enemyHealthRatio)
        {
            int phase = 0;
            for (int i = 0; i < phases.Count; i++)
                if (phases[i] != null && enemyHealthRatio <= phases[i].hpBelow) phase = i + 1;
            return phase;
        }

        /// <summary>단계를 반영한 겹 수 (1~MaxLayers)</summary>
        public int LayersFor(int phase)
        {
            int layers = maxSimultaneous;
            for (int i = 0; i < phase && i < phases.Count; i++) layers += phases[i].extraLayers;
            return Mathf.Clamp(layers, 1, MaxLayers);
        }

        /// <summary>단계를 반영한 (간격 배율, 탄속 배율)</summary>
        public (float interval, float speed) ScalesFor(int phase)
        {
            float interval = IntervalScale, speed = speedScale;
            for (int i = 0; i < phase && i < phases.Count; i++) { interval *= phases[i].intervalMultiplier; speed *= phases[i].speedMultiplier; }
            return (interval, speed);
        }

        /// <summary>n번째 겹(0 = 주 공격)의 시작 지연</summary>
        public float LayerDelay(int layer) => layer <= 0 ? 0f : layer == 1 ? secondPatternDelay : thirdPatternDelay;
    }
}
