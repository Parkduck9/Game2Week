using Game2Week.UI;
using NUnit.Framework;

namespace Game2Week.Tests
{
    public class MenuListTests
    {
        [Test]
        public void Move_WrapsAroundBothEnds()
        {
            var list = new MenuList();
            list.SetCount(3);

            Assert.IsTrue(list.Move(-1));
            Assert.AreEqual(2, list.Index);
            Assert.IsTrue(list.Move(1));
            Assert.AreEqual(0, list.Index);
        }

        [Test]
        public void Move_WithZeroOrOneItem_DoesNothing()
        {
            var list = new MenuList();
            Assert.IsFalse(list.Move(1));

            list.SetCount(1);
            Assert.IsFalse(list.Move(1));
            Assert.AreEqual(0, list.Index);
        }

        [Test]
        public void SetCount_ClampsIndex()
        {
            var list = new MenuList();
            list.SetCount(5);
            list.Move(4);

            list.SetCount(2);

            Assert.AreEqual(1, list.Index);
        }
    }
}
