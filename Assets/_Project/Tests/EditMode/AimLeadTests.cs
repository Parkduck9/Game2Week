using Game2Week.Battle.Patterns;
using Game2Week.Battle.Patterns.Trajectories;
using Game2Week.Data.Patterns;
using NUnit.Framework;
using UnityEngine;

namespace Game2Week.Tests
{
    /// <summary>예측 조준 (사용자 요청 2026-10-07): 움직이는 주인공의 앞을 노리고, 서 있으면 지금 위치.</summary>
    public class AimLeadTests
    {
        [Test]
        public void Standing_AimsAtCurrentPosition()
        {
            var target = new Vector3(0, 0, 10);
            Assert.AreEqual(target, AimLead.Predict(Vector3.zero, target, Vector3.zero, 4f, .75f, .7f));
            Assert.AreEqual(target, AimLead.Predict(Vector3.zero, target, Vector3.right * 3f, 4f, .75f, 0f), "lead 0 = 예측 없음");
        }

        [Test]
        public void Moving_FullLead_HitsWhereThePlayerWillBe()
        {
            // 옆에서 쏘는 탄: 주인공이 앞으로 3m/s, 탄속 10, 예고 0.2초 (앞을 보는 시간 상한 2초 안)
            var origin = new Vector3(-8, 0, 0);
            var target = Vector3.zero;
            var velocity = Vector3.forward * 3f;
            var aim = AimLead.Predict(origin, target, velocity, 10f, .2f, 1f);
            float t = .2f + Vector3.Distance(origin, aim) / 10f; // 조준점에 탄이 닿는 시각
            var playerThen = target + velocity * t;
            Assert.Less(Vector3.Distance(aim, playerThen), .05f, "완전 예측이면 그때 위치와 거의 같다");
            Assert.Greater(aim.z, 1f, "이동 방향 앞쪽");

            var partial = AimLead.Predict(origin, target, velocity, 10f, .2f, .7f);
            Assert.Less(partial.z, aim.z);
            Assert.Greater(partial.z, 0f);
        }

        [Test]
        public void LookAhead_IsCapped()
        {
            var aim = AimLead.Predict(Vector3.zero, new Vector3(0, 0, 40), Vector3.right * 3f, 1f, 1f, 1f);
            Assert.AreEqual(3f * AimLead.MaxLookAheadSeconds, aim.x, 1e-3f, "아주 먼 미래까지는 안 본다");
        }

        [Test]
        public void GraphTimeline_UsesVelocity_PreviewWithoutVelocityUnchanged()
        {
            var d = ScriptableObject.CreateInstance<GraphPatternDefinition>();
            try
            {
                d.origin = ShotOrigin.Left; d.aimRadius = 0; d.leadFactor = 1f; d.speed = 5f;
                var still = new PatternTimeline(d); still.Advance(0, new Vector3(0, 0, 8), Vector3.zero, new Vector2(20, 20), new System.Collections.Generic.List<PatternShot>());
                var moving = new PatternTimeline(d); moving.Advance(0, new Vector3(0, 0, 8), Vector3.zero, new Vector2(20, 20), new System.Collections.Generic.List<PatternShot>(), Vector3.forward * 3f);
                Assert.AreEqual(0f, still.WarningTarget.z, 1e-4f, "속도 없음(미리보기) = 지금 위치");
                Assert.Greater(moving.WarningTarget.z, 1f, "움직이면 앞을 노림");
            }
            finally { Object.DestroyImmediate(d); }
        }
    }
}
