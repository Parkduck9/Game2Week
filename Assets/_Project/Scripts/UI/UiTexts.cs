namespace Game2Week.UI
{
    /// <summary>메뉴·안내 화면의 공통 문구 (전투 문구는 BattleTexts). IP·현지화 때 여기만 바꾼다.</summary>
    public static class UiTexts
    {
        public static string GameTitle => Core.ProductInfo.Title;
        public const string ControlsHint = "방향키 이동 · Z / Enter 확인 · X / Shift 취소 · ESC 일시정지";

        public static readonly string[] MainMenu = { "새로 시작", "이어하기", "설정", "종료" };
        public const string ConfirmNewGame = "지금까지의 진행이 지워져요.\n새로 시작할까요? (설정은 그대로)";

        public static readonly string[] YesNo = { "예", "아니요" };

        public const string StageSelectTitle = "스테이지 선택";
        public const string NoRecord = "기록 없음";
        public const string StageSelectHint = "Z 시작 · X 메인으로";

        public static readonly string[] PauseMenu = { "계속", "설정", "메인으로" };
        public const string PauseTitle = "일시정지";
        public const string ConfirmQuitBattle = "메인으로 나갈까요?\n이번 전투 진행은 저장되지 않아요.";

        public const string SettingsTitle = "설정";

        public const string NextStage = "다음 스테이지";
        public const string ToEnding = "엔딩으로";
        public const string Retry = "다시 도전";
        public const string StageSelect = "스테이지 선택";
        public const string NewRecord = "신기록!";

        public static string Outcome(string outcome) => outcome switch
        {
            "EnemyDefeated" => "처치",
            "EnemySpared" => "살려줌",
            _ => string.Empty,
        };

        public static readonly string[] EndingLines =
        {
            "모든 스테이지를 마쳤어요.",
            "",
            "소심해 보여도, 할 땐 하는 아이였다.",
            "",
            "― 끝 ―",
            "",
            "플레이해 주셔서 감사합니다.",
        };
        public const string EndingHint = "Z 메인으로";
    }
}
