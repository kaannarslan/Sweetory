using System;
using NUnit.Framework;

namespace Sweetory.Core.Tests
{
    public class GridTests
    {
        [Test]
        public void NewGrid_AllCellsEmpty()
        {
            var grid = new Grid<string>(3, 2);

            for (int y = 0; y < grid.Height; y++)
                for (int x = 0; x < grid.Width; x++)
                    Assert.IsTrue(grid.IsEmpty(new GridPos(x, y)));
        }

        [Test]
        public void Set_ThenGet_ReturnsSameValue()
        {
            var grid = new Grid<string>(4, 4);
            var pos = new GridPos(2, 3);

            grid.Set(pos, "değirmen");

            Assert.AreEqual("değirmen", grid.Get(pos));
            Assert.IsFalse(grid.IsEmpty(pos));
        }

        [Test]
        public void Set_DoesNotAffectOtherCells()
        {
            var grid = new Grid<string>(4, 4);

            grid.Set(new GridPos(1, 2), "fırın");

            // (2,1) ile (1,2) karışırsa index hesabı yanlıştır.
            Assert.IsTrue(grid.IsEmpty(new GridPos(2, 1)));
        }

        [Test]
        public void SetNull_ClearsCell()
        {
            var grid = new Grid<string>(2, 2);
            var pos = new GridPos(0, 0);

            grid.Set(pos, "bant");
            grid.Set(pos, null);

            Assert.IsTrue(grid.IsEmpty(pos));
        }

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(3, 0)]
        [TestCase(0, 2)]
        public void OutOfBounds_InBoundsFalse_AndAccessThrows(int x, int y)
        {
            var grid = new Grid<string>(3, 2);
            var pos = new GridPos(x, y);

            Assert.IsFalse(grid.InBounds(pos));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(pos));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Set(pos, "x"));
        }

        [TestCase(0, 3)]
        [TestCase(3, 0)]
        [TestCase(-1, 5)]
        public void InvalidSize_Throws(int width, int height)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Grid<string>(width, height));
        }

        [Test]
        public void GridPos_EqualityAndAddition()
        {
            Assert.AreEqual(new GridPos(1, 2), new GridPos(1, 2));
            Assert.AreNotEqual(new GridPos(1, 2), new GridPos(2, 1));
            Assert.AreEqual(new GridPos(3, 1), new GridPos(1, 2) + new GridPos(2, -1));
        }
    }
}
