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
        public const string UnhurtHint = "탄막 턴을 한 번 맞지 않고 넘겨 보자.";
        public const string ListenHint = "대화에서 이야기를 들어 보자.";
        public const string HealthHint = "적이 지친 뒤 이야기를 해 보자.";
        public static string NoFightHint(int count, int required) => $"공격하지 않은 턴 {count}/{required}";
        public static string ActOrderHint(string order) => "행동 순서: " + order;
        public static string GemHint(int count, int required) => $"자비 보석 {count}/{required}";
        public static string ActionCountHint(SpareActionKind kind, int count, int required) => $"{(kind == SpareActionKind.Parry ? "쳐내기" : kind == SpareActionKind.RedPass ? "빨강 정지 통과" : "파랑 이동 통과")} {count}/{required}";
        public static string SpareHearts(SpareTracker tracker) => new string('●', tracker.Fulfilled) + new string('○', tracker.Total - tracker.Fulfilled);

        public const string TurnHint = "WASD 이동 · Shift 회피 · Space 점프 · 우클릭 쳐내기 · Ctrl 정지 자세 · 휠 클릭 록온";
        public const string BraceSettling = "자세 잡는 중";
        public const string BraceReady = "정지 자세";
        public const string BraceIdle = "Ctrl 정지 자세";
        /// <summary>빨강·파랑이 전투에서 처음 나올 때 잠깐 보여 주는 안내</summary>
        public const string RedGuide = "빨강 공격! Ctrl을 누르고 멈추면 통과한다";
        public const string BlueGuide = "파랑 공격! 계속 움직이면 통과한다";
        public const string MissedEnemy = "* 적에게 닿지 못했다. 숨을 고르자.";
        public const string SkipTurn = "* 가만히 숨을 골랐다.";
        public const string NoItems = "* 가진 아이템이 없다.";
        public const string NotReadyToSpare = "* ...아직은 마음을 열지 않은 것 같다.";
        public const string NowSpareable = "* 이제 살려 줄 수 있을 것 같다.";
        public const string PlayerDefeated = "* 눈앞이 캄캄해졌다...";
        public const string DefaultFlavor = "* 적이 이쪽을 노려본다.";
        public const string Miss = "MISS";
        public const string GaugeHint = "가운데에서 Z!";

        public static string AttackColorName(Patterns.AttackColor color) => color switch
        {
            Patterns.AttackColor.Red => "빨강",
            Patterns.AttackColor.Blue => "파랑",
            _ => "노랑",
        };

        /// <summary>화면 표시용 색 (TMP 리치 텍스트·HUD)</summary>
        public static UnityEngine.Color AttackColorTint(Patterns.AttackColor color) => color switch
        {
            Patterns.AttackColor.Red => new UnityEngine.Color(1f, 0.35f, 0.35f),
            Patterns.AttackColor.Blue => new UnityEngine.Color(0.4f, 0.68f, 1f),
            _ => new UnityEngine.Color(1f, 0.85f, 0.3f),
        };

        public static string FightResult(string enemyName, int damage) =>
            damage > 0 ? $"* {enemyName}에게 {damage}의 피해를 주었다!" : "* 빗나갔다!";
    }
}
