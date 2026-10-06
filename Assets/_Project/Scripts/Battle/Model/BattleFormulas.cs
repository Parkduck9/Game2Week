using System;

namespace Game2Week.Battle
{
    public static class BattleFormulas
    {
        /// <summary>
        /// 임시 공격 데미지 (07단계 타이밍 공격 + 데미지 공식 확정 전까지).
        /// 공격력 × 배율 − 방어력, 최소 1.
        /// </summary>
        public static int TempFightDamage(int attack, float multiplier, int defense) =>
            Math.Max(1, (int)Math.Round(attack * multiplier) - defense);
    }
}
