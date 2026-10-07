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
    public class CombatFeelRuntimeTests:InputTestFixture
    {
        public override void Setup(){base.Setup();TestSave.Begin();InputSystem.AddDevice<Keyboard>();InputSystem.AddDevice<Mouse>();}
        public override void TearDown(){Time.timeScale=1;TestSave.End();base.TearDown();}
        static void Capture(string name){var canvas=Object.FindAnyObjectByType<Canvas>();bool before=canvas&&canvas.enabled;if(canvas)canvas.enabled=false;try{SceneCapture.Save(name);}finally{if(canvas)canvas.enabled=before;}}
        [UnityTest] public IEnumerator 공격동작중영향시점과후퇴연결을확인한다()
        {
            BattleController battle=null;yield return SceneFlowTests.EnterStage(0,value=>battle=value);battle.Context.ChangeState(BattleStateId.EnemyTurn);yield return null;
            BattleTestUtil.MoveNearEnemy(battle,.6f);battle.Context.ChangeState(BattleStateId.ActionMenu);yield return null;
            var player=battle.World.Player;var before=player.transform.position;var enemy=battle.Spawner.Enemy.transform.position;
            var driver=player.GetComponent<PlayerAnimationDriver>();int impacts=0,hp=battle.Context.Enemy.CurrentHp,shakes=battle.CameraDirector.ShakeCount;
            battle.Context.PlayPresentation(ActionCue.Strike,()=>{impacts++;battle.Context.Events.RaiseEnemyDamaged(battle.Context.Enemy.TakeDamage(1));},()=>{});
            yield return new WaitForSeconds(.2f);Assert.AreEqual(0,impacts);Assert.AreEqual(hp,battle.Context.Enemy.CurrentHp);Assert.AreEqual(PlayerMotion.Strike,driver.Current);Capture("combat_strike_ready");
            Time.timeScale=0;yield return new WaitForSecondsRealtime(.1f);Assert.AreEqual(0,impacts);Time.timeScale=1;
            yield return new WaitForSeconds(.13f);Assert.AreEqual(1,impacts);Assert.AreEqual(hp-1,battle.Context.Enemy.CurrentHp);Assert.Greater(battle.CameraDirector.ShakeCount,shakes);Capture("combat_strike_hit");
            yield return new WaitForSeconds(.5f);Assert.AreEqual(1,impacts);Assert.AreEqual(1,driver.Animator.speed);
            battle.Context.NextTurn();yield return new WaitForSeconds(.3f);Assert.AreEqual(BattleStateId.ActionPresentation,battle.Context.CurrentState);Assert.IsNull(battle.World.Patterns.CurrentObject);Capture("combat_retreat");
            yield return new WaitForSeconds(.75f);Assert.AreEqual(BattleStateId.EnemyTurn,battle.Context.CurrentState);Assert.Less(Vector3.Distance(before,player.transform.position),.01f);Assert.Greater(Vector3.Distance(player.transform.position,battle.Spawner.Enemy.transform.position),3.5f);
        }
        [UnityTest] public IEnumerator 공동행동은둘이함께보여주고적비행은메뉴에서멈춘다()
        {
            BattleController battle=null;yield return SceneFlowTests.EnterStage(1,value=>battle=value);battle.Context.ChangeState(BattleStateId.EnemyTurn);yield return null;
            var enemy=battle.Spawner.Enemy;var start=enemy.transform.position;yield return new WaitForSeconds(.4f);Assert.Greater(Vector3.Distance(start,enemy.transform.position),.05f);
            battle.Context.ChangeState(BattleStateId.ActionMenu);BattleTestUtil.MoveNearEnemy(battle,.7f);start=enemy.transform.position;yield return new WaitForSeconds(.2f);Assert.Less(Vector3.Distance(start,enemy.transform.position),.001f);
            var driver=battle.World.Player.GetComponent<PlayerAnimationDriver>();bool finished=false;
            battle.Context.PlayPresentation(ActionCue.Play,null,()=>finished=true);yield return new WaitForSeconds(.4f);Assert.IsFalse(finished);Assert.AreEqual(PlayerMotion.Play,driver.Current);Capture("combat_play_together");yield return new WaitForSeconds(1.1f);Assert.IsTrue(finished);
            start=enemy.transform.position;var player=battle.World.Player.transform.position;finished=false;
            battle.Context.PlayPresentation(ActionCue.RunTogether,null,()=>finished=true);yield return new WaitForSeconds(.5f);Assert.AreEqual(PlayerMotion.RunTogether,driver.Current);Assert.Greater(Vector3.Distance(start,enemy.transform.position),.1f);Assert.Greater(Vector3.Distance(player,battle.World.Player.transform.position),.1f);Capture("combat_run_together");yield return new WaitForSeconds(1);Assert.IsTrue(finished);
        }
        [UnityTest] public IEnumerator 이미멀리있으면후퇴를생략해순간이동하지않는다()
        {
            BattleController battle=null;yield return SceneFlowTests.EnterStage(0,value=>battle=value);
            var at=battle.Spawner.Enemy.transform.position;battle.Context.NextTurn();yield return null;yield return null;
            Assert.AreEqual(BattleStateId.EnemyTurn,battle.Context.CurrentState);
            Assert.Less(Vector3.Distance(at,battle.Spawner.Enemy.transform.position),.1f);
            battle.Context.ChangeState(BattleStateId.ItemMenu);
        }
        [UnityTest] public IEnumerator 빗나감은공격동작만보여주고충격과피해를남기지않는다()
        {
            BattleController battle=null;yield return SceneFlowTests.EnterStage(0,value=>battle=value);var driver=battle.World.Player.GetComponent<PlayerAnimationDriver>();
            int hp=battle.Context.Enemy.CurrentHp,shakes=battle.CameraDirector.ShakeCount;
            battle.Context.PlayPresentation(ActionCue.Strike,()=>battle.Context.Events.RaiseEnemyDamaged(0),()=>{},false);yield return new WaitForSeconds(.2f);Assert.AreEqual(PlayerMotion.Strike,driver.Current);yield return new WaitForSeconds(.6f);
            Assert.AreEqual(hp,battle.Context.Enemy.CurrentHp);Assert.AreEqual(shakes,battle.CameraDirector.ShakeCount);
        }
    }
}
