using System.Collections;
using Game2Week.Battle;
using Game2Week.Battle.Patterns;
using Game2Week.Battle.View.Animation;
using Game2Week.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;

namespace Game2Week.Tests
{
    public class AnimationPhaseRuntimeTests:InputTestFixture
    {
        public override void Setup(){base.Setup();TestSave.Begin();InputSystem.AddDevice<Keyboard>();InputSystem.AddDevice<Mouse>();}
        public override void TearDown(){Time.timeScale=1;TestSave.End();base.TearDown();}
        static void Capture(string name)
        {
            var canvas=Object.FindAnyObjectByType<Canvas>();bool enabled=canvas&&canvas.enabled;
            if(canvas)canvas.enabled=false;
            try{SceneCapture.Save(name);}finally{if(canvas)canvas.enabled=enabled;}
        }
        [UnityTest] public IEnumerator 록온여덟방향과멈춤강약피격을실제클립으로재생한다()
        {
            BattleController battle=null;yield return SceneFlowTests.EnterStage(0,value=>battle=value);
            var mover=battle.World.Player;var driver=mover.GetComponent<PlayerAnimationDriver>();driver.ClearDialoguePose();
            for(int i=0;i<8;i++)
            {
                float angle=i*Mathf.PI/4;var input=new Vector2(Mathf.Sin(angle),Mathf.Cos(angle));
                mover.Move(input,.05f,Vector3.forward);yield return null;yield return null;
                Assert.AreEqual(PlayerMotion.Strafe,driver.Current);
                var expected=PlayerAnimationMap.LocalDirection(mover.GroundVelocity,mover.transform.rotation);
                Assert.AreEqual(expected.x,driver.Animator.GetFloat("MoveX"),.001f);Assert.AreEqual(expected.y,driver.Animator.GetFloat("MoveY"),.001f);
            }
            SceneCapture.Save("phase12_lockon");mover.StopActions();yield return null;yield return null;Assert.AreEqual(PlayerMotion.Stop,driver.Current);
            battle.Context.Events.RaisePlayerDamaged(1);yield return null;yield return null;Assert.AreEqual(PlayerMotion.Hit,driver.Current);
            yield return new WaitForSeconds(.32f);battle.Context.Events.RaisePlayerDamaged(4);yield return null;yield return null;Assert.AreEqual(PlayerMotion.HitStrong,driver.Current);
            SceneCapture.Save("phase12_hit_strong");
            Time.timeScale=0;var before=driver.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;yield return new WaitForSecondsRealtime(.12f);
            Assert.AreEqual(before,driver.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime,.001f);
        }
        [UnityTest] public IEnumerator 대화자세와승리자비가실제전투사건에연결된다()
        {
            BattleController battle=null;yield return SceneFlowTests.EnterStage(0,value=>battle=value);
            var driver=battle.World.Player.GetComponent<PlayerAnimationDriver>();
            foreach(var id in new[]{"talk","nod","surprise"})
            {
                battle.Context.Events.RaiseDialogueCue("heroine",id,"player_close");yield return new WaitForSeconds(.2f);
                Assert.IsTrue(PlayerAnimationMap.TryPose(id,out var expected));Assert.AreEqual(expected,driver.Current);Capture("phase12_dialogue_"+id);
            }
            battle.Context.Events.RaiseDialogueCue("enemy","nod","enemy_close");yield return null;yield return null;
            Assert.AreEqual(EnemyMotion.Nod,battle.Spawner.Enemy.CurrentMotion);
            battle.Context.Events.RaiseDialogueCue("heroine","","player_close");
            battle.Context.Events.RaiseBattleEnded(BattleOutcome.EnemyDefeated);yield return new WaitForSeconds(.6f);Assert.AreEqual(PlayerMotion.Victory,driver.Current);Capture("phase12_victory");
            battle.Context.Events.RaiseBattleEnded(BattleOutcome.EnemySpared);yield return new WaitForSeconds(.15f);Assert.AreEqual(PlayerMotion.Spare,driver.Current);Capture("phase12_spare");
            battle.Context.Events.RaiseDialogueCue("heroine","talk","player_close");yield return null;yield return null;Assert.AreEqual(PlayerMotion.Talk,driver.Current);
            Assert.Greater(battle.Spawner.Enemy.Body.localScale.magnitude,.1f,"종료대화 중 적 외형을 유지한다.");
        }
        [UnityTest] public IEnumerator 적준비발사단계누그러짐과일시정지를연결한다()
        {
            BattleController battle=null;yield return SceneFlowTests.EnterStage(0,value=>battle=value);
            var enemy=battle.Spawner.Enemy;var feedback=battle.World.Feedback;var at=enemy.transform.position;
            feedback.RaiseWarningStarted(AttackColor.Yellow,at);yield return null;yield return null;Assert.AreEqual(EnemyMotion.Warning,enemy.CurrentMotion);SceneCapture.Save("phase12_enemy_warning");
            feedback.RaiseBulletFired(AttackColor.Yellow,at);yield return null;yield return null;Assert.AreEqual(EnemyMotion.Fire,enemy.CurrentMotion);
            feedback.RaiseEnemyPhaseChanged(1);yield return null;yield return null;Assert.AreEqual(EnemyMotion.Phase,enemy.CurrentMotion);SceneCapture.Save("phase12_enemy_phase");
            Time.timeScale=0;var rotation=enemy.Body.localRotation;yield return new WaitForSecondsRealtime(.12f);Assert.Less(Quaternion.Angle(rotation,enemy.Body.localRotation),.001f);Time.timeScale=1;
            yield return new WaitForSeconds(1.1f);enemy.ClearDialoguePose();battle.Context.Events.RaiseSpareChanged("●●●");yield return null;yield return null;Assert.AreEqual(EnemyMotion.Relaxed,enemy.CurrentMotion);
            battle.Context.Events.RaiseEnemyDamaged(1);yield return null;yield return null;Assert.AreEqual(EnemyMotion.Hit,enemy.CurrentMotion);
        }
    }
}
