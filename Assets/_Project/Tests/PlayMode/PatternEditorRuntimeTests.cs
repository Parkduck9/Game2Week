using System.Collections;
using System.Collections.Generic;
using Game2Week.Battle;
using Game2Week.Battle.Patterns;
using Game2Week.Battle.Patterns.Trajectories;
using Game2Week.Core;
using Game2Week.Data;
using Game2Week.Data.Patterns;
using Game2Week.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    public class ActionPhaseThreePatternEditorTests : InputTestFixture
    {
        public override void Setup(){base.Setup();TestSave.Begin();InputSystem.AddDevice<Keyboard>();InputSystem.AddDevice<Mouse>();}
        public override void TearDown(){TestSave.End();base.TearDown();}
        [UnityTest] public IEnumerator SavedPresets_UseSharedTrajectory_ColorThreats_PauseAndEnd()
        {
            BattleController battle=null;yield return SceneFlowTests.EnterStage(0,c=>battle=c);
            battle.Context.ChangeState(BattleStateId.EnemyTurn);battle.World.Patterns.End();
            foreach(TrajectoryKind kind in System.Enum.GetValues(typeof(TrajectoryKind)))
            {
                var session=Resources.FindObjectsOfTypeAll<GameSession>()[0];
                var data=session.Catalog.FindPattern("Attack_Graph_"+kind);
                Assert.IsNotNull(data,"8종 저장 에셋 등록");
                var mover=battle.World.Player;
                var context=new PatternContext(battle.Spawner.Arena,battle.Spawner.Enemy.transform,battle.Spawner.Enemy,mover.transform,mover.Radius,4,_=>{},mover);
                battle.World.Patterns.Begin(data,context,battle.Spawner.Arena.transform);
                var runtime=(GraphAttackPattern)battle.World.Patterns.Current;
                runtime.Tick(.86f);Assert.Greater(runtime.Bullets.Count,0);
                var definition=Resources.FindObjectsOfTypeAll<GraphPatternDefinition>();
                GraphPatternDefinition asset=null;foreach(var d in definition)if(d.name=="Graph_"+kind)asset=d;
                Assert.IsNotNull(asset);
                var bullet=runtime.Bullets[0];
                var shotOrigin=runtime.Timeline.WarningOrigin;
                var forward=(runtime.Timeline.WarningTarget-shotOrigin).normalized;
                var launch=new TrajectoryLaunch(battle.Spawner.Arena.transform.TransformPoint(shotOrigin),forward*asset.speed);
                var expected=GraphTrajectory.Create(asset).Evaluate(launch,.01f);
                Assert.That(Vector3.Distance(expected,bullet.transform.position),Is.LessThan(.002f),kind.ToString());
                var threats=new List<ThreatPoint>();runtime.CollectThreats(threats);Assert.Greater(threats.Count,0);Assert.AreEqual(AttackColor.Yellow,threats[0].Color);
                var position=bullet.transform.position;runtime.Tick(0);Assert.AreEqual(position,bullet.transform.position);
                if(kind==TrajectoryKind.Sine){yield return new WaitForSeconds(1.1f);SceneCapture.Save("pattern_editor_sine_runtime");}
                battle.World.Patterns.End();Assert.IsFalse(bullet.Active);Assert.IsNull(battle.World.Patterns.Current);yield return null;
            }
            battle.Context.ChangeState(BattleStateId.ItemMenu);
        }
    }
}
