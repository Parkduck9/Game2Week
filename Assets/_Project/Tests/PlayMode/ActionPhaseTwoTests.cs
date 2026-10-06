using System.Collections;
using System.Collections.Generic;
using Game2Week.Battle;
using Game2Week.Battle.Patterns;
using Game2Week.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    public class ActionPhaseTwoTests : InputTestFixture
    {
        // 입력을 쓰는 씬은 같은 격리된 Input System 수명으로 실행한다.
        // 일반 fixture에서 공유 액션 에셋을 켜면 다음 입력 fixture의 reset과 섞인다.
        public override void Setup(){base.Setup();TestSave.Begin();InputSystem.AddDevice<Keyboard>();InputSystem.AddDevice<Mouse>();}
        public override void TearDown(){Time.timeScale=1f;TestSave.End();base.TearDown();}
        [UnityTest] public IEnumerator StageThree_SideWarning_WaveAndFeedback_PauseAndCleanup()
        {
            BattleController battle=null;
            yield return SceneFlowTests.EnterStage(2,c=>battle=c);
            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            // 5단계부터 1-3은 PatternDirector가 첫 턴에 새 패턴(측면 교대)을 단독으로 돌린다 (간격 배율 1.10/1.40 적용)
            var pattern=battle.World.Patterns.CurrentObject.GetComponentInChildren<YellowTrainingPattern>();
            var firstTarget=pattern.WarningTarget;
            yield return new WaitForSeconds(1.2f);
            Assert.Greater(pattern.Bullets.Count,0);
            Assert.IsInstanceOf<SineBullet>(pattern.Bullets[0]);
            Assert.AreEqual(firstTarget,pattern.WarningTarget,"한 예고 동안 조준 위치 고정");
            yield return new WaitForSeconds(.7f);
            Assert.IsTrue(pattern.WarningActive);
            var local=battle.Spawner.Arena.transform.InverseTransformPoint(pattern.WarningOrigin);
            Assert.Less(local.x,-battle.Spawner.Arena.Size.x*.4f,"두 번째 공격은 왼쪽 경계에서 진입");
            var feedback=battle.World.ThreatFeedback;
            Assert.Greater(feedback.ActiveVoices,0);Assert.LessOrEqual(feedback.ActiveVoices,3);
            var voices=feedback.GetComponentsInChildren<AudioSource>();
            Assert.AreEqual(3,voices.Length);
            var bulletPosition=pattern.Bullets[0].transform.position;
            battle.Pause.Pause();yield return new WaitForSecondsRealtime(.12f);
            Assert.AreEqual(bulletPosition,pattern.Bullets[0].transform.position);
            foreach(var voice in voices)Assert.IsFalse(voice.isPlaying,"일시정지 경고음 멈춤");
            battle.Pause.Resume();yield return null;yield return null;
            // 화면 위의 시험 위험을 실제 카메라로 투영해 표시 경로 확인.
            var bullet=pattern.Bullets[0];
            bullet.Launch(battle.World.Player.transform.position+Vector3.up*4f,Vector3.zero);
            yield return null;yield return null;
            Assert.Greater(feedback.VisibleIndicators,0,"화면 밖 위험은 가장자리 표시");
            SceneCapture.Save("action_phase2_side_warning");
            battle.Context.ChangeState(BattleStateId.ItemMenu);yield return null;
            Assert.AreEqual(0,feedback.ActiveVoices);Assert.AreEqual(0,feedback.VisibleIndicators);
            foreach(var voice in voices)Assert.IsFalse(voice.isPlaying);
            Assert.IsNull(battle.World.Patterns.CurrentObject);
        }
    }
}
