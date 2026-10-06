using Game2Week.Battle.Patterns;

namespace Game2Week.Battle.View
{
    /// <summary>IHitRule 구현 — 주인공의 실제 자세·속도로 ColorRules를 판정한다.</summary>
    public sealed class PlayerHitRule : IHitRule
    {
        readonly PlayerMover player;

        public PlayerHitRule(PlayerMover player) => this.player = player;

        /// <summary>색 규칙으로 통과시킨 판정 횟수 (겹친 동안 매 구간 셈 — 표시·테스트용)</summary>
        public int RedPasses { get; private set; }
        public int BluePasses { get; private set; }

        public bool ShouldHit(AttackColor color)
        {
            if (!ColorRules.Passes(color, player.Motor.BraceReady, player.GroundSpeed, player.Settings)) return true;
            if (color == AttackColor.Red) RedPasses++;
            else if (color == AttackColor.Blue) BluePasses++;
            return false;
        }

        public void ResetCounts() => RedPasses = BluePasses = 0;
    }
}
