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
        [Test] public void 공동행동종류는데이터로선택하고전투수치는유지한다()
        {
            var first=AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/Enemy_Test.asset");
            var second=AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/Enemy_Test2.asset");
            Assert.AreEqual(70,first.MaxHp);Assert.AreEqual(90,second.MaxHp);Assert.AreEqual(4,first.Attack);Assert.AreEqual(5,second.Attack);
            Assert.IsTrue(System.Linq.Enumerable.Any(first.Acts,a=>a.Motion==ActionCue.Play));Assert.IsTrue(System.Linq.Enumerable.Any(second.Acts,a=>a.Motion==ActionCue.RunTogether));
        }
    }
}
