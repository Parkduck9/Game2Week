using Game2Week.Battle.Patterns;
using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>IHitRule 구현 — 주인공의 실제 자세·속도로 ColorRules를 판정한다.</summary>
    public sealed class PlayerHitRule : IHitRule
    {
        /// <summary>같은 색 통과 알림 최소 간격 (초) — 겹친 동안 매 구간 판정하므로 연출이 쏟아지지 않게</summary>
        const float PassNotifyInterval = 0.25f;

        readonly PlayerMover player;
        readonly BattleFeedback feedback;
        float lastRedNotify = float.NegativeInfinity, lastBlueNotify = float.NegativeInfinity;

        public PlayerHitRule(PlayerMover player, BattleFeedback feedback = null)
        {
            this.player = player;
            this.feedback = feedback;
        }

        /// <summary>색 규칙으로 통과시킨 판정 횟수 (겹친 동안 매 구간 셈 — 표시·테스트용)</summary>
        public int RedPasses { get; private set; }
        public int BluePasses { get; private set; }

        public bool ShouldHit(AttackColor color)
        {
            if (!ColorRules.Passes(color, player.Motor.BraceReady, player.GroundSpeed, player.Settings)) return true;
            if (color == AttackColor.Red) { RedPasses++; Notify(color, ref lastRedNotify); }
            else if (color == AttackColor.Blue) { BluePasses++; Notify(color, ref lastBlueNotify); }
            return false;
        }

        void Notify(AttackColor color, ref float last)
        {
            if (feedback == null || Time.time - last < PassNotifyInterval) return;
            last = Time.time;
            feedback.RaiseColorPassed(color, player.transform.position);
        }

        public void ResetCounts() => RedPasses = BluePasses = 0;
    }
}
