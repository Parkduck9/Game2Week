using Game2Week.Battle;
using Game2Week.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
namespace Game2Week.Tests
{
    public class CombatFeelTests
    {
        [Test] public void 공격영향은한번만적용하고정지시간에는진행하지않는다()
        {
            var clock=new ActionPresentationClock(.7f,.3f,.055f);
            Assert.IsFalse(clock.Advance(0));Assert.IsFalse(clock.Advance(.2f));Assert.IsTrue(clock.Advance(.11f));Assert.IsTrue(clock.Holding);
            float position=clock.Progress;Assert.IsFalse(clock.Advance(0));Assert.AreEqual(position,clock.Progress);
            Assert.IsFalse(clock.Advance(.06f));Assert.IsFalse(clock.Holding);Assert.IsFalse(clock.Advance(1));Assert.IsTrue(clock.Complete);
            Assert.IsFalse(clock.Advance(1));
        }
        [Test] public void 큰프레임에서도후퇴와행동영향은한번만끝난다()
        {var clock=new ActionPresentationClock(1,1,0);Assert.IsFalse(clock.Advance(.5f));Assert.IsTrue(clock.Advance(2));Assert.IsTrue(clock.Complete);Assert.IsFalse(clock.Advance(2));}
        [Test] public void 후퇴는반대방향과경기장경계를지킨다()
        {
            Vector3 Clamp(Vector3 p)=>new Vector3(Mathf.Clamp(p.x,-5,5),p.y,Mathf.Clamp(p.z,-5,5));
            var target=RetreatPath.Target(Vector3.zero,Vector3.back,3,Clamp);Assert.AreEqual(new Vector3(0,0,3),target);
            var enemy=new Vector3(0,0,4.8f);var player=new Vector3(0,0,4);target=RetreatPath.Target(enemy,player,3,Clamp);
            Assert.LessOrEqual(Mathf.Abs(target.x),5);Assert.LessOrEqual(Mathf.Abs(target.z),5);Assert.Greater(Vector3.Distance(target,player),Vector3.Distance(enemy,player));
        }
        /// <summary>사용자 요청 (행동 뒤 너무 조금 멀어짐): 후퇴 목표 거리는 처음 시작 거리 기준, 시간은 거리에 비례.</summary>
        [Test] public void 후퇴거리는처음시작거리기준이고시간은거리에비례한다()
        {
            Assert.AreEqual(14.4f,RetreatPath.DesiredSeparation(4.5f,18f,.8f),1e-4f);
            Assert.AreEqual(4.5f,RetreatPath.DesiredSeparation(4.5f,3f,.8f),1e-4f,"최소 거리 보장");
            Assert.IsTrue(RetreatPath.FarEnough(13.5f,14.4f));Assert.IsFalse(RetreatPath.FarEnough(4.5f,14.4f),"예전 4.5m는 부족");
            Assert.AreEqual(1.2f,RetreatPath.Duration(14.4f,12f,.9f,2f),1e-4f);
            Assert.AreEqual(.9f,RetreatPath.Duration(3f,12f,.9f,2f),1e-4f);Assert.AreEqual(2f,RetreatPath.Duration(40f,12f,.9f,2f),1e-4f);
            // 적 뒤가 벽이면 옆이나 주인공 너머로라도 멀어진다
            Vector3 Clamp(Vector3 p)=>new Vector3(Mathf.Clamp(p.x,-10,10),p.y,Mathf.Clamp(p.z,-11,11));
            var enemy=new Vector3(0,0,9);var player=new Vector3(0,0,8.4f);
            var target=RetreatPath.Target(enemy,player,13.8f,Clamp);
            Assert.Greater(Vector3.Distance(target,player),10f,"벽에 막혀도 10m 이상 확보");
        }
        [Test] public void 공동행동종류는데이터로선택하고전투수치는유지한다()
        {
            var first=AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/Enemy_Test.asset");
            var second=AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/Enemy_Test2.asset");
            Assert.AreEqual(70,first.MaxHp);Assert.AreEqual(90,second.MaxHp);Assert.AreEqual(4,first.Attack);Assert.AreEqual(5,second.Attack);
            Assert.IsTrue(System.Linq.Enumerable.Any(first.Acts,a=>a.Motion==ActionCue.Play));Assert.IsTrue(System.Linq.Enumerable.Any(second.Acts,a=>a.Motion==ActionCue.RunTogether));
        }
    }
}
