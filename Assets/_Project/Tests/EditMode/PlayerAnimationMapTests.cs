using Game2Week.Battle.View.Animation;
using NUnit.Framework;

namespace Game2Week.Tests
{
    public class PlayerAnimationMapTests
    {
        [Test] public void 실제속도와액션우선순위를사용한다()
        {
            Assert.AreEqual(PlayerMotion.Idle,PlayerAnimationMap.Resolve(0,false,false,false,false,false,1,false,false,false));
            Assert.AreEqual(PlayerMotion.Run,PlayerAnimationMap.Resolve(2,false,false,false,false,false,1,false,false,false));
            Assert.AreEqual(PlayerMotion.Dodge,PlayerAnimationMap.Resolve(2,true,true,true,true,false,1,false,false,false));
            Assert.AreEqual(PlayerMotion.Jump,PlayerAnimationMap.Resolve(0,false,true,false,false,false,1,false,false,false));
            Assert.AreEqual(PlayerMotion.Brace,PlayerAnimationMap.Resolve(3,false,false,false,true,true,1,false,false,false));
        }
        [Test] public void 좌우쳐내기와피격착지쓰러짐을구분한다()
        {
            Assert.AreEqual(PlayerMotion.ParryLeft,PlayerAnimationMap.Resolve(1,false,false,true,false,false,1,false,false,false));
            Assert.AreEqual(PlayerMotion.ParryRight,PlayerAnimationMap.Resolve(1,false,false,true,false,false,-1,false,false,false));
            Assert.AreEqual(PlayerMotion.Land,PlayerAnimationMap.Resolve(0,false,false,false,false,false,1,true,false,false));
            Assert.AreEqual(PlayerMotion.Hit,PlayerAnimationMap.Resolve(0,true,true,true,true,true,1,true,true,false));
            Assert.AreEqual(PlayerMotion.Fall,PlayerAnimationMap.Resolve(0,true,true,true,true,true,1,true,true,true));
        }
    }
}
