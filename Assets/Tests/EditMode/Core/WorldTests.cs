using NUnit.Framework;

namespace Sweetory.Core.Tests
{
    public class WorldTests
    {
        [Test]
        public void TryPlace_OnEmptyCell_Succeeds()
        {
            var world = new World(5, 5);
            var mill = new Building(new GridPos(2, 2), "mill");

            Assert.IsTrue(world.TryPlace(mill));
            Assert.AreSame(mill, world.GetAt(new GridPos(2, 2)));
        }

        [Test]
        public void TryPlace_OnOccupiedCell_FailsAndKeepsOriginal()
        {
            var world = new World(5, 5);
            var pos = new GridPos(1, 1);
            var mill = new Building(pos, "mill");
            world.TryPlace(mill);

            Assert.IsFalse(world.TryPlace(new Belt(pos, Direction.Up)));
            Assert.AreSame(mill, world.GetAt(pos));
        }

        [Test]
        public void TryPlace_OutOfBounds_Fails()
        {
            var world = new World(3, 3);

            Assert.IsFalse(world.TryPlace(new Belt(new GridPos(3, 0), Direction.Right)));
            Assert.IsFalse(world.TryPlace(new Belt(new GridPos(-1, 0), Direction.Right)));
        }

        [Test]
        public void Remove_ReturnsStructure_AndEmptiesCell()
        {
            var world = new World(3, 3);
            var belt = new Belt(new GridPos(0, 0), Direction.Right);
            world.TryPlace(belt);

            Assert.AreSame(belt, world.Remove(new GridPos(0, 0)));
            Assert.IsNull(world.GetAt(new GridPos(0, 0)));
            Assert.IsTrue(world.CanPlace(new GridPos(0, 0)));
        }

        [Test]
        public void Remove_EmptyOrOutOfBounds_ReturnsNull()
        {
            var world = new World(3, 3);

            Assert.IsNull(world.Remove(new GridPos(1, 1)));
            Assert.IsNull(world.Remove(new GridPos(10, 10)));
        }

        [Test]
        public void GetAt_OutOfBounds_ReturnsNull()
        {
            var world = new World(3, 3);

            Assert.IsNull(world.GetAt(new GridPos(-5, 2)));
        }

        [Test]
        public void Belt_OutputPosition_FollowsDirection()
        {
            var pos = new GridPos(2, 2);

            Assert.AreEqual(new GridPos(2, 3), new Belt(pos, Direction.Up).OutputPosition);
            Assert.AreEqual(new GridPos(3, 2), new Belt(pos, Direction.Right).OutputPosition);
            Assert.AreEqual(new GridPos(2, 1), new Belt(pos, Direction.Down).OutputPosition);
            Assert.AreEqual(new GridPos(1, 2), new Belt(pos, Direction.Left).OutputPosition);
        }

        [Test]
        public void Direction_RotateAndOpposite()
        {
            Assert.AreEqual(Direction.Right, Direction.Up.RotateClockwise());
            Assert.AreEqual(Direction.Up, Direction.Left.RotateClockwise());
            Assert.AreEqual(Direction.Down, Direction.Up.Opposite());
            Assert.AreEqual(Direction.Right, Direction.Left.Opposite());
        }
    }
}
