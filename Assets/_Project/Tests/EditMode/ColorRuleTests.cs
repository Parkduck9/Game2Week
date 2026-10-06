using Game2Week.Battle;
using Game2Week.Battle.Patterns;
using Game2Week.Data;
using NUnit.Framework;
using UnityEngine;

namespace Game2Week.Tests
{
    /// <summary>4단계: 빨강(정지 자세 + 실제 정지) · 파랑(실제 이동) 판정과 정지 자세 상태.</summary>
    public class ColorRuleTests
    {
        PlayerActionSettings settings;

        [SetUp]
        public void SetUp() => settings = ScriptableObject.CreateInstance<PlayerActionSettings>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(settings);

        [Test]
        public void Red_PassesOnlyWithBraceAndStill()
        {
            Assert.IsTrue(ColorRules.Passes(AttackColor.Red, true, 0f, settings));
            Assert.IsFalse(ColorRules.Passes(AttackColor.Red, false, 0f, settings), "그냥 멈춰 있기만 하면 실패");
            Assert.IsFalse(ColorRules.Passes(AttackColor.Red, true, 1f, settings), "자세여도 움직이고 있으면 실패");
        }

        [Test]
        public void Blue_PassesOnlyWhenActuallyMoving()
        {
            Assert.IsTrue(ColorRules.Passes(AttackColor.Blue, false, settings.blueMinSpeed, settings));
            Assert.IsFalse(ColorRules.Passes(AttackColor.Blue, false, 0.2f, settings), "벽에 막혀 거의 안 움직이면 실패");
            Assert.IsFalse(ColorRules.Passes(AttackColor.Blue, true, 0f, settings), "정지 자세는 파랑을 막지 못함");
        }

        [Test]
        public void Yellow_NeverPassesByColorRule()
        {
            Assert.IsFalse(ColorRules.Passes(AttackColor.Yellow, true, 0f, settings), "정지 자세는 만능 방어가 아님");
            Assert.IsFalse(ColorRules.Passes(AttackColor.Yellow, false, 5f, settings));
        }

        [Test]
        public void Brace_StopsMovement_AndIsReadyAfterSettleTime()
        {
            var motor = new PlayerMotorModel(settings);
            motor.SetBrace(true);
            motor.Tick(Vector2.up, 0.02f);
            Assert.IsTrue(motor.Bracing);
            Assert.IsFalse(motor.BraceReady, "전환 시간 전");
            Assert.AreEqual(Vector2.zero, motor.Displacement, "자세 중에는 이동 입력 무시");

            for (int i = 0; i < 10; i++) motor.Tick(Vector2.up, 0.02f);
            Assert.IsTrue(motor.BraceReady);

            motor.SetBrace(false);
            motor.Tick(Vector2.up, 0.02f);
            Assert.IsFalse(motor.Bracing);
            Assert.Greater(motor.Displacement.y, 0f, "놓으면 바로 다시 이동");
        }

        [Test]
        public void Brace_DuringDodge_StartsAfterDodgeEnds()
        {
            var motor = new PlayerMotorModel(settings);
            motor.RequestDodge();
            motor.Tick(Vector2.right, 0.02f);
            Assert.IsTrue(motor.Dodging);

            motor.SetBrace(true);
            motor.Tick(Vector2.right, 0.02f);
            Assert.IsFalse(motor.Bracing, "회피 중에는 자세로 바뀌지 않음 (회피 이동 유지)");
            Assert.Greater(motor.Displacement.x, 0f);

            for (int i = 0; i < 20 && motor.Dodging; i++) motor.Tick(Vector2.right, 0.02f);
            motor.Tick(Vector2.right, 0.02f);
            Assert.IsTrue(motor.Bracing, "회피가 끝나면 자세로");
        }

        [Test]
        public void Brace_BlocksNewDodgeJumpAndParry()
        {
            var motor = new PlayerMotorModel(settings);
            motor.SetBrace(true);
            motor.Tick(Vector2.zero, 0.02f);
            motor.RequestDodge();
            motor.RequestJump();
            motor.RequestParry();
            motor.Tick(Vector2.zero, 0.02f);
            Assert.IsFalse(motor.Dodging);
            Assert.IsFalse(motor.Airborne);
            Assert.IsFalse(motor.Parrying);
        }

        [Test]
        public void Brace_WhileAirborne_WaitsForLanding()
        {
            var motor = new PlayerMotorModel(settings);
            motor.RequestJump();
            motor.Tick(Vector2.zero, 0.02f);
            motor.SetBrace(true);
            motor.Tick(Vector2.zero, 0.02f);
            Assert.IsTrue(motor.Airborne);
            Assert.IsFalse(motor.Bracing);
            for (int i = 0; i < 60 && motor.Airborne; i++) motor.Tick(Vector2.zero, 0.02f);
            motor.Tick(Vector2.zero, 0.02f);
            Assert.IsTrue(motor.Bracing);
        }
    }
}
