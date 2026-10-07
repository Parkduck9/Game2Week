using Game2Week.Data;
using Game2Week.Stages;

namespace Game2Week.Battle
{
    public static class GemRewards
    {
        /// <summary>보석 보상을 적용하고, 화면에 띄울 짧은 문구를 돌려준다.</summary>
        public static string Apply(string gemType, GemRewardSettings settings, PlayerCombatant player, EnemyCombatant enemy)
        {
            switch (gemType)
            {
                case GemTypes.Heal:
                    int healed = player.Heal(settings.HealAmount);
                    return healed > 0 ? $"HP {healed} 회복" : "HP가 가득 차 있다";
                case GemTypes.Attack:
                    player.GrantAttackBoost(settings.AttackMultiplier);
                    return $"다음 공격 ×{settings.AttackMultiplier:0.#}";
                case GemTypes.Spare:
                    if (enemy.Spare.UsesRule) return enemy.Spare.RevealGemHint();
                    enemy.AddSpareProgress(settings.SpareProgress);
                    return enemy.CanBeSpared ? "마음이 통했다" : "조금 가까워진 것 같다";
                default:
                    return string.Empty;
            }
        }
    }
}
