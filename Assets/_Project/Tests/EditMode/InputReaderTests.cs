using Game2Week.Core;
using NUnit.Framework;
using UnityEngine;

namespace Game2Week.Tests
{
    public class InputReaderTests
    {
        [TestCase(0f, 0f, 0, 0)]
        [TestCase(0.3f, 0.2f, 0, 0)]
        [TestCase(1f, 0f, 1, 0)]
        [TestCase(-1f, 0f, -1, 0)]
        [TestCase(0f, 1f, 0, 1)]
        [TestCase(0f, -1f, 0, -1)]
        [TestCase(0.9f, 0.4f, 1, 0)]
        [TestCase(-0.3f, -0.8f, 0, -1)]
        public void ToDirection_PicksDominantAxis(float x, float y, int expectedX, int expectedY)
        {
            Assert.AreEqual(new Vector2Int(expectedX, expectedY), InputReader.ToDirection(new Vector2(x, y)));
        }
    }
}
