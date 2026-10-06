using System;
using Game2Week.Data;

namespace Game2Week.Battle
{
    /// <summary>전투 중 플레이어의 상태. 데이터 에셋은 읽기만 하고 변하는 값은 여기에 둔다.</summary>
    public sealed class PlayerCombatant
    {
        public PlayerCombatant(PlayerData data)
        {
            Data = data ? data : throw new ArgumentNullException(nameof(data));
            CurrentHp = data.MaxHp;
            Inventory = new Inventory(data.StartingItems);
        }

        public PlayerData Data { get; }
        public int MaxHp => Data.MaxHp;
        public int CurrentHp { get; private set; }
        public bool IsDefeated => CurrentHp <= 0;
        public Inventory Inventory { get; }

        /// <summary>실제로 깎인 HP를 돌려준다.</summary>
        public int TakeDamage(int amount)
        {
            int applied = Math.Clamp(amount, 0, CurrentHp);
            CurrentHp -= applied;
            return applied;
        }

        /// <summary>다음 공격에 곱해질 배율 (공격 보석 등). 공격하면 1로 돌아간다.</summary>
        public float NextAttackMultiplier { get; private set; } = 1f;

        /// <summary>더 큰 배율만 남긴다 (중첩되지 않음).</summary>
        public void GrantAttackBoost(float multiplier) => NextAttackMultiplier = Math.Max(NextAttackMultiplier, multiplier);

        public float ConsumeAttackMultiplier()
        {
            var value = NextAttackMultiplier;
            NextAttackMultiplier = 1f;
            return value;
        }

        /// <summary>실제로 회복된 HP를 돌려준다.</summary>
        public int Heal(int amount)
        {
            int applied = Math.Clamp(amount, 0, MaxHp - CurrentHp);
            CurrentHp += applied;
            return applied;
        }
    }
}
