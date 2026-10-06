using System.Collections;
using Game2Week.Battle;
using Game2Week.Battle.View.Animation;
using Game2Week.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    public class ActionPhaseSevenPlayerAnimationTests : InputTestFixture
    {
        Keyboard keyboard;
        public override void Setup(){base.Setup();TestSave.Begin();keyboard=InputSystem.AddDevice<Keyboard>();InputSystem.AddDevice<Mouse>();}
        public override void TearDown(){TestSave.End();base.TearDown();}
        [UnityTest] public IEnumerator 실제전투스킨과동작전환()
        {
            BattleController battle=null;yield return SceneFlowTests.EnterStage(0,c=>battle=c);
            var driver=battle.World.Player.GetComponent<PlayerAnimationDriver>();Assert.IsTrue(driver.IsReady);
            driver.Bind(battle.World.Player,battle.World.Feedback);
            Assert.Greater(driver.GetComponentsInChildren<SkinnedMeshRenderer>().Length,0);
            Assert.IsNull(driver.transform.Find("RightHand_ParryCue"));
            battle.Context.ChangeState(BattleStateId.EnemyTurn);yield return null;
            PressAndRelease(keyboard.spaceKey);yield return new WaitForSeconds(.13f);
            Assert.AreEqual(PlayerMotion.Jump,driver.Current);SceneCapture.Save("heroine_v2_jump");
            yield return new WaitForSeconds(.65f);Press(keyboard.dKey);PressAndRelease(keyboard.leftShiftKey);yield return new WaitForSeconds(.1f);
            Assert.AreEqual(PlayerMotion.Dodge,driver.Current);Release(keyboard.dKey);SceneCapture.Save("heroine_v2_dodge");
            yield return new WaitForSeconds(.3f);Press(keyboard.leftCtrlKey);yield return new WaitForSeconds(.14f);
            Assert.AreEqual(PlayerMotion.Brace,driver.Current);
            var arm=System.Array.Find(driver.GetComponentsInChildren<Transform>(),t=>t.name=="RightUpperArm");
            Assert.IsNotNull(arm);Assert.Greater(Quaternion.Angle(Quaternion.identity,arm.localRotation),20f,"정지 자세 클립이 실제 팔 골격을 움직여야 한다.");
            SceneCapture.Save("heroine_v2_brace");Release(keyboard.leftCtrlKey);
            battle.World.Feedback.RaisePlayerHit(driver.transform.position);yield return new WaitForSeconds(.05f);
            Assert.AreEqual(PlayerMotion.Hit,driver.Current);yield return new WaitForSeconds(.3f);
            driver.PlayFall();yield return new WaitForSeconds(.2f);Assert.AreEqual(PlayerMotion.Fall,driver.Current);
            battle.Context.ChangeState(BattleStateId.ItemMenu);
        }
    }
}
