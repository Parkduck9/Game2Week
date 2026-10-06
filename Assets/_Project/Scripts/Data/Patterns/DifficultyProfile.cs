using UnityEngine;

namespace Game2Week.Data.Patterns
{
    /// <summary>
    /// 맵 단계별 난이도 (5단계). 하위 패턴의 기본 발사 간격 × (기준 간격 / 1.40), 탄속 × 배율을 한 번만 적용한다.
    /// 동시 실행 상한과 살아 있는 위험 상한(공유 예산)도 여기서 정한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game2Week/Difficulty Profile", fileName = "Profile_")]
    public sealed class DifficultyProfile : ScriptableObject
    {
        /// <summary>간격 배율의 기준 — 1단계 원형 기준 간격</summary>
        public const float BaseReferenceInterval = 1.40f;

        [Tooltip("이 단계의 기준 발사 간격 (초). 1.40이면 패턴 기본값 그대로")]
        [Min(0.1f)] public float referenceInterval = BaseReferenceInterval;
        [Tooltip("탄속 배율")]
        [Min(0.1f)] public float speedScale = 1f;
        [Tooltip("한 턴에 동시에 돌릴 수 있는 패턴 수 (1 또는 2)")]
        [Range(1, 2)] public int maxSimultaneous = 1;
        [Tooltip("살아 있는 위험(예고 + 탄) 수가 이 이상이면 두 번째 패턴 시작을 미룬다")]
        [Min(1)] public int liveThreatCap = 8;
        [Tooltip("두 번째 패턴을 시작하기까지 최소 시간 (초)")]
        [Min(0f)] public float secondPatternDelay = 1.6f;

        public float IntervalScale => referenceInterval / BaseReferenceInterval;
    }
}
