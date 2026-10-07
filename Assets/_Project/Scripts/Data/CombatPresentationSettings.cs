using UnityEngine;
namespace Game2Week.Data
{
    [CreateAssetMenu(menuName="Game2Week/전투 연출 설정")]
    public sealed class CombatPresentationSettings:ScriptableObject
    {
        [Header("공격과 행동 시간")]
        [Min(.3f)] public float strikeDuration=.7f;
        [Min(0)] public float strikeImpactTime=.3f;
        [Range(0,.15f)] public float hitHoldSeconds=.055f;
        [Min(.2f)] public float actDuration=1.4f;
        [Header("타격 반응")]
        [Range(0,4)] public float shakeForce=2f;
        [Range(0,.6f)] public float recoilDistance=.3f;
        [Header("비행과 거리 확보")]
        [Min(0)] public float hoverHeight=.22f;
        [Min(0)] public float hoverRange=.15f;
        [Min(0)] public float flightRadius=.8f;
        [Min(0)] public float flightSpeed=.6f;
        [Tooltip("후퇴 뒤 둘 사이 최소 거리 (m)")]
        [Min(1)] public float retreatDistance=4.5f;
        [Tooltip("후퇴 최소 시간 (초)")]
        [Min(.1f)] public float retreatDuration=.9f;
        [Tooltip("후퇴 뒤 둘 사이 거리 = 맵 처음 시작점-적 거리 × 이 비율 (사용자 요청: 행동 뒤 충분히 멀어지게). 경기장 벽에 막히면 가능한 만큼")]
        [Range(0,1.2f)] public float retreatToStartRatio=.8f;
        [Tooltip("후퇴 비행 속도 (m/s) — 거리가 길면 시간이 그만큼 늘어난다")]
        [Min(1)] public float retreatSpeed=12f;
        [Tooltip("후퇴 최대 시간 (초)")]
        [Min(.2f)] public float retreatMaxDuration=2f;
    }
}
