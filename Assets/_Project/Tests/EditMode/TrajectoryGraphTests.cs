using System.Collections.Generic;
using Game2Week.Battle.Patterns;
using Game2Week.Battle.Patterns.Trajectories;
using Game2Week.Data.Patterns;
using NUnit.Framework;
using UnityEngine;

namespace Game2Week.Tests
{
    public class TrajectoryGraphTests
    {
        GraphPatternDefinition d;
        [SetUp] public void Setup(){d=ScriptableObject.CreateInstance<GraphPatternDefinition>();d.aimRadius=0;d.lifetime=2;d.speed=2;d.amplitude=1;d.frequency=.5f;d.arcRadius=2;}
        [TearDown] public void Cleanup()=>Object.DestroyImmediate(d);
        [TestCase(TrajectoryKind.Straight)][TestCase(TrajectoryKind.Sine)][TestCase(TrajectoryKind.Zigzag)][TestCase(TrajectoryKind.Parabola)]
        [TestCase(TrajectoryKind.Arc)][TestCase(TrajectoryKind.Spiral)][TestCase(TrajectoryKind.FigureEight)][TestCase(TrajectoryKind.Bezier)]
        public void EveryTrajectory_StartsAtOrigin_UsesLaunchBasis(TrajectoryKind kind)
        {
            d.trajectory=kind;var path=GraphTrajectory.Create(d);var origin=new Vector3(3,.32f,4);
            var forward=new TrajectoryLaunch(origin,Vector3.forward*2);var right=new TrajectoryLaunch(origin,Vector3.right*2);
            Assert.That(Vector3.Distance(origin,path.Evaluate(forward,0)),Is.LessThan(.0001f));
            var a=path.Evaluate(forward,.5f)-origin;var b=path.Evaluate(right,.5f)-origin;
            Assert.That(Vector3.Distance(Quaternion.AngleAxis(90,Vector3.up)*a,b),Is.LessThan(.0001f));
            Assert.IsTrue(PatternGraphRules.Finite(a.x)&&PatternGraphRules.Finite(a.y)&&PatternGraphRules.Finite(a.z));
        }
        [Test] public void TrajectoryShapes_HaveExpectedIndependentReferenceValues()
        {
            var launch=new TrajectoryLaunch(Vector3.zero,Vector3.forward*2);
            d.trajectory=TrajectoryKind.Sine;Assert.That(GraphTrajectory.Create(d).Evaluate(launch,.25f).x,Is.EqualTo(1).Within(.0001));
            d.trajectory=TrajectoryKind.Zigzag;Assert.That(GraphTrajectory.Create(d).Evaluate(launch,.25f).x,Is.EqualTo(1).Within(.0001));
            d.trajectory=TrajectoryKind.Parabola;Assert.That(GraphTrajectory.Create(d).Evaluate(launch,1).y,Is.EqualTo(1).Within(.0001));
            d.trajectory=TrajectoryKind.Arc;var arc=GraphTrajectory.Create(d).Evaluate(launch,1);
            Assert.That(arc.x,Is.EqualTo(2*(1-Mathf.Cos(1))).Within(.0001));Assert.That(arc.z,Is.EqualTo(2*Mathf.Sin(1)).Within(.0001));
            d.trajectory=TrajectoryKind.Spiral;var spiral=GraphTrajectory.Create(d).Evaluate(launch,.25f);
            Assert.That(spiral.x,Is.EqualTo(.125f).Within(.0001));Assert.That(spiral.z,Is.EqualTo(0).Within(.0001));
            d.trajectory=TrajectoryKind.FigureEight;var eight=GraphTrajectory.Create(d);
            Assert.That(eight.Evaluate(launch,.25f).x,Is.EqualTo(1).Within(.0001));
            Assert.That(eight.Evaluate(launch,.5f).magnitude,Is.LessThan(.0001));
            Assert.That(eight.Evaluate(launch,.75f).x,Is.EqualTo(-1).Within(.0001));
            d.trajectory=TrajectoryKind.Bezier;Assert.That(Vector3.Distance(d.bezierEnd,GraphTrajectory.Create(d).Evaluate(launch,2)),Is.LessThan(.0001f));
        }
        [Test] public void SpeedCurve_IntegratesTime_AndSnapshotDoesNotChange()
        {
            d.speedMultiplier=AnimationCurve.Linear(0,0,1,2);var path=GraphTrajectory.Create(d);
            Assert.That(path.Evaluate(new TrajectoryLaunch(Vector3.zero,Vector3.forward*2),1).z,Is.EqualTo(1).Within(.001));
            d.speedMultiplier=AnimationCurve.Linear(0,5,1,5);
            Assert.That(path.Evaluate(new TrajectoryLaunch(Vector3.zero,Vector3.forward*2),1).z,Is.EqualTo(1).Within(.001));
        }
        [Test] public void Timeline_ReservationIsFrozen_AndAimDoesNotFollowPlayer()
        {
            d.warningSeconds=.5f;d.interval=AnimationCurve.Linear(0,2,30,2);
            var timeline=new PatternTimeline(d);var shots=new List<PatternShot>();
            timeline.Advance(0,new Vector3(0,0,3),new Vector3(0,0,-3),new Vector2(8,10),shots);
            var fixedTarget=timeline.WarningTarget;
            timeline.Advance(.5f,new Vector3(0,0,3),Vector3.right*3,new Vector2(8,10),shots);
            Assert.AreEqual(1,shots.Count);Assert.AreEqual(fixedTarget,timeline.WarningTarget);Assert.AreEqual(2.5f,timeline.NextShotTime);
            d.interval=AnimationCurve.Linear(0,.2f,30,.2f);
            timeline.Advance(.5f,Vector3.zero,Vector3.zero,new Vector2(8,10),shots);
            Assert.AreEqual(2.5f,timeline.NextShotTime);
        }
        [Test] public void Timeline_FanRingAndSeed_AreReproducibleAcrossSteps()
        {
            d.layout=ShotLayout.Fan;d.shotCount=3;d.spreadDegrees=60;d.aimRadius=.3f;
            var a=new PatternTimeline(d);var b=new PatternTimeline(d);var first=new List<PatternShot>();var second=new List<PatternShot>();
            a.Advance(4,Vector3.forward*3,Vector3.back*3,new Vector2(8,10),first);
            for(int i=0;i<240;i++)b.Advance(1/60f,Vector3.forward*3,Vector3.back*3,new Vector2(8,10),second);
            Assert.AreEqual(first.Count,second.Count);
            for(int i=0;i<first.Count;i++){Assert.That(first[i].Time,Is.EqualTo(second[i].Time).Within(.0001));Assert.AreEqual(first[i].Velocity,second[i].Velocity);}
            Assert.That(Vector3.Angle(first[0].Velocity,first[2].Velocity),Is.EqualTo(60).Within(.001));
            d.layout=ShotLayout.Ring;d.shotCount=4;var ring=new List<PatternShot>();new PatternTimeline(d).Advance(1,Vector3.zero,Vector3.forward,new Vector2(8,10),ring);
            Assert.AreEqual(4,ring.Count);Assert.That(Vector3.Angle(ring[0].Velocity,ring[1].Velocity),Is.EqualTo(90).Within(.001));
        }
        [Test] public void Timeline_SideAlternation_AndMinimumWarningInterval()
        {
            d.origin=ShotOrigin.Alternating;d.interval=AnimationCurve.Linear(0,.01f,30,.01f);d.warningSeconds=.5f;
            var shots=new List<PatternShot>();new PatternTimeline(d).Advance(1.5f,Vector3.forward*3,Vector3.back*2,new Vector2(8,10),shots);
            Assert.AreEqual(3,shots.Count);Assert.AreEqual(0,shots[0].Origin.x);Assert.Less(shots[1].Origin.x,-3);Assert.Greater(shots[2].Origin.x,3);
            Assert.That(shots[2].Time-shots[1].Time,Is.EqualTo(.5f));
        }
        [Test] public void Validation_RejectsNaN_Empty_NegativeAndOvershootingCurves()
        {
            Assert.IsEmpty(PatternGraphRules.Validate(d));d.speed=float.NaN;Assert.IsNotEmpty(PatternGraphRules.Validate(d));d.speed=2;
            d.interval=new AnimationCurve();Assert.IsNotEmpty(PatternGraphRules.Validate(d));d.interval=AnimationCurve.Linear(0,1,30,1);
            d.speedMultiplier=AnimationCurve.Linear(0,-1,1,1);Assert.IsNotEmpty(PatternGraphRules.Validate(d));
            d.speedMultiplier=new AnimationCurve(new Keyframe(0,1,0,100),new Keyframe(1,1,-100,0));Assert.IsNotEmpty(PatternGraphRules.Validate(d));
        }
    }
}
