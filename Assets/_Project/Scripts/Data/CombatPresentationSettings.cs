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
        [Min(1)] public float retreatDistance=4.5f;
        [Min(.1f)] public float retreatDuration=.9f;
    }
}
