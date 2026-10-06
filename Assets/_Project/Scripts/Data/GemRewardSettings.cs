using UnityEngine;

namespace Game2Week.Data
{
    /// <summary>보석 보상 수치. (임시 값 — 데미지 공식 정할 때 같이 확정)</summary>
    [CreateAssetMenu(menuName = "Game2Week/Gem Reward Settings", fileName = "GemRewardSettings")]
    public sealed class GemRewardSettings : ScriptableObject
    {
        [SerializeField, Min(0)] int healAmount = 5;
        [Tooltip("다음 공격 배율")]
        [SerializeField, Min(1f)] float attackMultiplier = 1.5f;
        [SerializeField, Min(0)] int spareProgress = 1;

        public int HealAmount => healAmount;
        public float AttackMultiplier => attackMultiplier;
        public int SpareProgress => spareProgress;
    }
}
