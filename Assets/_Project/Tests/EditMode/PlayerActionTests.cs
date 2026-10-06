using Game2Week.Battle;
using Game2Week.Battle.Patterns;
using Game2Week.Data;
using NUnit.Framework;
using UnityEngine;

namespace Game2Week.Tests
{
    public class PlayerActionTests
    {
        PlayerActionSettings settings;
        PlayerMotorModel motor;
        [SetUp] public void Setup() { settings = ScriptableObject.CreateInstance<PlayerActionSettings>(); motor = new PlayerMotorModel(settings); }
        [TearDown] public void TearDown() => Object.DestroyImmediate(settings);

        [Test] public void Parry_AllowsMovement_AndHasTimedWindow()
        {
            motor.RequestParry(); motor.Tick(Vector2.right, 0.06f);
            Assert.IsTrue(motor.CanParry); Assert.Greater(motor.Displacement.x, 0f);
            motor.Tick(Vector2.right, 0.20f); Assert.IsFalse(motor.CanParry);
        }
        [Test] public void Dodge_HasLimitedInvulnerability_AndCooldown()
        {
            motor.RequestDodge(); motor.Tick(Vector2.right, 0.02f); Assert.IsFalse(motor.DodgeInvulnerable);
            motor.Tick(Vector2.right, 0.08f); Assert.IsTrue(motor.DodgeInvulnerable);
            motor.Tick(Vector2.right, 0.10f); Assert.IsFalse(motor.DodgeInvulnerable);
            motor.Tick(Vector2.right, 0.04f); Assert.IsFalse(motor.Dodging);
            motor.RequestDodge(); motor.Tick(Vector2.zero, 0.02f); Assert.IsFalse(motor.Dodging);
        }
        [Test] public void Dodge_DistanceDoesNotOvershootOnLongFrame()
        { motor.RequestDodge(); motor.Tick(Vector2.up, 0.5f); Assert.AreEqual(settings.dodgeDistance, motor.Displacement.y, 0.001f); }
        [Test] public void Jump_ReachesApex_ThenLands_AndCannotAirDodge()
        {
            motor.RequestJump(); motor.Tick(Vector2.zero, settings.jumpDuration / 2f);
            Assert.AreEqual(settings.jumpHeight, motor.Height, 0.001f);
            motor.RequestDodge(); motor.Tick(Vector2.zero, 0.02f); Assert.IsFalse(motor.Dodging);
            motor.Tick(Vector2.zero, settings.jumpDuration); Assert.IsTrue(motor.IsGrounded); Assert.AreEqual(0f, motor.Height);
        }
        [Test] public void Pause_DoesNotAdvanceTime_AndClearBufferDropsActions()
        {
            motor.RequestJump(); motor.Tick(Vector2.zero, 0f); Assert.IsFalse(motor.Airborne);
            motor.ClearBuffer(); motor.Tick(Vector2.zero, 0.02f); Assert.IsFalse(motor.Airborne);
        }
        [Test] public void SweptCollision_HitsFastCrossing_ButNotAboveJumpingBody()
        {
            Assert.IsTrue(AttackGeometry.SweptBody(new(-2f,.32f,0f), new(2f,.32f,0f), Vector3.zero, Vector3.zero,.36f,-.14f,.99f));
            Assert.IsFalse(AttackGeometry.SweptBody(new(-2f,.32f,0f), new(2f,.32f,0f), Vector3.up*.75f,Vector3.up*.75f,.36f,-.14f,.99f));
        }
        [Test] public void SweptCollision_AccountsForPlayerCrossingStationaryBullet()
        { Assert.IsTrue(AttackGeometry.SweptBody(new(0,.32f,0),new(0,.32f,0),Vector3.left*2f,Vector3.right*2f,.36f,-.14f,.99f)); }
        [TestCase(-1f, 1f)] [TestCase(1f, -1f)]
        public void Deflection_CrossesOppositeShoulder_AndGoesBehind(float incoming, float outgoing)
        {
            var direction = AttackGeometry.DeflectionDirection(Vector3.right * incoming, Vector3.zero, Vector3.forward, Vector3.right);
            Assert.AreEqual(outgoing, Mathf.Sign(direction.x)); Assert.Less(direction.z,0f);
        }
    }
}
