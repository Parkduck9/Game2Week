namespace Game2Week.Battle
{
    /// <summary>
    /// 전투 상태 식별자. 상태끼리는 서로를 직접 참조하지 않고 이 ID로만 전환을 요청한다.
    /// </summary>
    public enum BattleStateId
    {
        Intro,
        PlayerMenu,
        Fight,
        Act,
        Item,
        Mercy,
        Dialogue,
        EnemyTurn,
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
