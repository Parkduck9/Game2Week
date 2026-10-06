namespace Game2Week.Battle
{
    /// <summary>
    /// 전투 화면에 나오는 공통 문구. 메뉴 이름은 임시 — IP 기획 때 여기만 바꾼다.
    /// (적마다 다른 문구는 EnemyData에 있다)
    /// </summary>
    public static class BattleTexts
    {
        public static readonly string[] ActionMenu = { "공격", "행동", "자비" };
        public static readonly string[] ItemMenu = { "아이템", "넘기기" };

        public const string Check = "살펴보기";
        public const string Spare = "살려주기";
        public const string SpareReadyColor = "#FFD84A";

        public const string TurnHint = "적에게 닿으면 공격 기회! · 보석을 주우면 좋은 일이";
        public const string MissedEnemy = "* 적에게 닿지 못했다. 숨을 고르자.";
        public const string SkipTurn = "* 가만히 숨을 골랐다.";
        public const string NoItems = "* 가진 아이템이 없다.";
        public const string NotReadyToSpare = "* ...아직은 마음을 열지 않은 것 같다.";
        public const string NowSpareable = "* 이제 살려 줄 수 있을 것 같다.";
        public const string PlayerDefeated = "* 눈앞이 캄캄해졌다...";
        public const string DefaultFlavor = "* 적이 이쪽을 노려본다.";
        public const string Miss = "MISS";
        public const string GaugeHint = "가운데에서 Z!";

        public static string FightResult(string enemyName, int damage) =>
            damage > 0 ? $"* {enemyName}에게 {damage}의 피해를 주었다!" : "* 빗나갔다!";
    }
}
