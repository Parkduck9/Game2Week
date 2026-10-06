namespace Game2Week.Battle
{
    /// <summary>
    /// 전투 상태 식별자. 상태끼리는 서로를 직접 참조하지 않고 이 ID로만 전환을 요청한다.
    /// 흐름: Intro → EnemyTurn → (적에게 닿음) ActionMenu → Fight/Act/Mercy
    ///                        → (못 닿음)   ItemMenu   → Item/넘기기 → EnemyTurn …
    /// </summary>
    public enum BattleStateId
    {
        Intro,
        /// <summary>탄막 턴 — 주인공이 경기장을 움직이며 적에게 닿으려 한다</summary>
        EnemyTurn,
        /// <summary>적에게 닿았을 때: 공격 / 행동 / 자비</summary>
        ActionMenu,
        /// <summary>못 닿았을 때: 아이템 / 넘기기</summary>
        ItemMenu,
        Fight,
        Act,
        Item,
        Mercy,
        Victory,
        Defeat,
    }

    public enum BattleOutcome
    {
        EnemyDefeated,
        EnemySpared,
        PlayerDefeated,
    }
}
