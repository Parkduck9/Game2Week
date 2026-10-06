using System;

namespace Game2Week.Battle
{
    public static class BattleFormulas
    {
        /// <summary>
        /// 공격 데미지 = 공격력 × 정확도 배율 × 보석 배율 − 방어력 (최소 1). 놓치면(accuracy null) 0.
        /// </summary>
        public static int FightDamage(int attack, float? accuracy, float boostMultiplier, int defense)
        {
            if (accuracy is not { } a) return 0;
            return Math.Max(1, (int)Math.Round(attack * TimingGauge.Multiplier(a) * boostMultiplier) - defense);
        }

        /// <summary>탄 한 발 데미지 = 적 공격력 − 주인공 방어력 (최소 1).</summary>
        public static int BulletDamage(int enemyAttack, int playerDefense) => Math.Max(1, enemyAttack - playerDefense);
    }
}
