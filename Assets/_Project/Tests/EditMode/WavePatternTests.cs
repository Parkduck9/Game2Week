using Game2Week.Battle;
using Game2Week.Battle.Patterns;
using NUnit.Framework;
using UnityEngine;

namespace Game2Week.Tests
{
    public class WavePatternTests
    {
        [Test] public void Sine_HasExpectedQuarterPeriodOffset_AndFrozenOrigin()
        {
            var origin=new Vector3(3f,.32f,4f);
            var result=WaveTrajectory.Evaluate(origin,Vector3.forward,.25f,2f,.4f,1f);
            Assert.AreEqual(3.4f,result.x,.001f); Assert.AreEqual(.32f,result.y,.001f); Assert.AreEqual(4.5f,result.z,.001f);
            Assert.AreEqual(origin,WaveTrajectory.Evaluate(origin,Vector3.forward,0f,2f,.4f,1f));
        }
        [Test] public void RandomAim_IsSeeded_AndStaysInsideRadius()
        {
            var first=new System.Random(42);var second=new System.Random(42);
            for(int i=0;i<100;i++)
            { var offset=WaveTrajectory.AimOffset(first,.25f);Assert.AreEqual(offset,WaveTrajectory.AimOffset(second,.25f));Assert.LessOrEqual(offset.magnitude,.25001f); }
        }
        [Test] public void ProximityVolume_IncreasesNearPlayer_AndHonorsMute()
        {
            Assert.Greater(ThreatFeedbackModel.Volume(1f,7f,.12f,.8f),ThreatFeedbackModel.Volume(6f,7f,.12f,.8f));
            Assert.AreEqual(0f,ThreatFeedbackModel.Volume(1f,7f,.12f,0f));
            Assert.AreEqual(0f,ThreatFeedbackModel.Volume(8f,7f,.12f,1f));
        }
        [TestCase(2f,.5f,1f,.92f,.5f)]
        [TestCase(.5f,-1f,1f,.5f,.08f)]
        [TestCase(2f,.5f,-1f,.08f,.5f)]
        public void ScreenEdge_HandlesSideAndBehind(float x,float y,float z,float expectedX,float expectedY)
        { var edge=ThreatFeedbackModel.EdgePosition(new Vector3(x,y,z));Assert.AreEqual(expectedX,edge.x,.001f);Assert.AreEqual(expectedY,edge.y,.001f); }
    }
}
