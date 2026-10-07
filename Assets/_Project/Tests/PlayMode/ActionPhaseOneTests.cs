using System.Collections;
using Game2Week.Battle;
using Game2Week.Battle.Patterns;
using Game2Week.Battle.View;
using Game2Week.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    public class ActionPhaseOneTests : InputTestFixture
    {
        Keyboard keyboard;
        Mouse mouse;
        public override void Setup()
        {
            base.Setup(); TestSave.Begin();
            keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>();
        }
        public override void TearDown() { Time.timeScale = 1f; TestSave.End(); base.TearDown(); }
        IEnumerator Enter(System.Action<BattleController> ready)
        {
            BattleController battle = null;
            yield return SceneFlowTests.EnterStage(0, c => battle = c);
            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            yield return null; yield return null;
            ready(battle);
        }

        [UnityTest] public IEnumerator KeyboardMouse_JumpDodgeLockOn_PauseAndMenu()
        {
            BattleController battle = null; yield return Enter(c => battle = c);
            var player = battle.World.Player;
            var camera = Object.FindAnyObjectByType<BattleCameraDirector>();
            PressAndRelease(mouse.middleButton); yield return null; yield return null;
            Assert.IsTrue(camera.IsLockedOn);
            PressAndRelease(keyboard.spaceKey); yield return new WaitForSeconds(.16f);
            Assert.IsTrue(player.Motor.Airborne); Assert.Greater(player.transform.position.y, .3f);
            Assert.IsFalse(battle.World.IsPlayerTouchingEnemy());
            SceneCapture.Save("action_phase1_jump");
            yield return new WaitForSeconds(.6f); Assert.IsTrue(player.Motor.IsGrounded);
            Press(keyboard.dKey); PressAndRelease(keyboard.leftShiftKey);
            yield return new WaitForSeconds(.1f); Assert.IsTrue(player.Motor.Dodging);
            Assert.IsTrue(player.Motor.DodgeInvulnerable);
            Release(keyboard.dKey);
            battle.Pause.Pause();
            float cooldown = player.Motor.DodgeCooldown;
            PressAndRelease(keyboard.spaceKey); PressAndRelease(mouse.rightButton);
            yield return new WaitForSecondsRealtime(.12f);
            Assert.AreEqual(cooldown, player.Motor.DodgeCooldown);
            yield return null; Assert.AreEqual(CursorLockMode.None, Cursor.lockState);
            battle.Pause.Resume(); yield return new WaitForSeconds(.18f);
            Assert.IsFalse(player.Motor.Airborne, "일시정지 중 액션을 예약하지 않음");
            Assert.IsFalse(player.Motor.Parrying);
            battle.Context.ChangeState(BattleStateId.ActionMenu); yield return null; yield return null;
            Assert.IsTrue(battle.Ui.IsMainMenuOpen); Assert.AreEqual(CursorLockMode.None, Cursor.lockState);
            Assert.IsNull(battle.World.Patterns.CurrentObject);
        }

        [UnityTest] public IEnumerator Yellow_ParryBothSides_MovesAndDeflectsBehind()
        {
            BattleController battle = null; yield return Enter(c => battle = c);
            var player = battle.World.Player;
            PressAndRelease(mouse.middleButton); yield return null; yield return null;
            yield return new WaitForSeconds(.85f);
            var pattern = BattleTestUtil.FindPattern<YellowTrainingPattern>(battle);
            Assert.Greater(pattern.Bullets.Count, 0);
            var source = pattern.Bullets[0];
            var clone = Object.Instantiate(source, battle.Spawner.Arena.transform);
            try
            {
                foreach (float side in new[] { -1f, 1f })
                {
                    PressAndRelease(mouse.rightButton); yield return new WaitForSeconds(.06f);
                    Assert.IsTrue(player.Motor.CanParry);
                    var start = player.transform.position;
                    var forward = player.transform.forward; var right = player.transform.right;
                    clone.Launch(start + right * side * .65f + forward * .1f + Vector3.up*.4f, -right * side * 4f);
                    var context = new PatternContext(battle.Spawner.Arena,battle.Spawner.Enemy.transform,battle.Spawner.Enemy,player.transform,player.Radius,4,_=>Assert.Fail("쳐낸 탄에 피해"),player);
                    Assert.IsFalse(clone.Tick(.01f,context)); Assert.IsTrue(clone.Deflected);
                    clone.Tick(.13f,context);
                    Assert.AreEqual(-side, Mathf.Sign(Vector3.Dot(clone.transform.position-start,right)));
                    clone.Tick(.12f,context); Assert.IsFalse(clone.Active);
                    Assert.Less(Vector3.Dot(clone.transform.position-start,forward),0f);
                    yield return new WaitForSeconds(.6f);
                }
                Press(keyboard.dKey); PressAndRelease(mouse.rightButton);
                var position = player.transform.position;
                yield return new WaitForSeconds(.08f);
                Assert.IsTrue(player.Motor.Parrying); Assert.Greater(Vector3.Distance(position,player.transform.position),.1f);
                Release(keyboard.dKey); SceneCapture.Save("action_phase1_parry");
            }
            finally { if (clone) Object.Destroy(clone.gameObject); }
        }

        [UnityTest] public IEnumerator MouseOrbit_ChangesCameraRelativeMovement()
        {
            BattleController battle = null; yield return Enter(c => battle = c);
            var camera = Object.FindAnyObjectByType<BattleCameraDirector>();
            float yaw = camera.Yaw;
            Set(mouse.delta, new Vector2(160f,0f)); yield return null; yield return null;
            Assert.Greater(Mathf.Abs(Mathf.DeltaAngle(yaw,camera.Yaw)),5f);
            var expected = camera.ToWorldMove(Vector2.up);
            var before = battle.Spawner.Player.position;
            Press(keyboard.wKey); yield return new WaitForSeconds(.1f); Release(keyboard.wKey);
            var delta = battle.Spawner.Player.position-before;
            Assert.Greater(Vector2.Dot(new Vector2(delta.x,delta.z).normalized,expected.normalized),.95f);
            yield return new WaitForSeconds(.25f); SceneCapture.Save("action_phase1_waist_camera");
        }
    }
}
