using NUnit.Framework;

namespace Sweetory.Core.Tests
{
    public class BeltTests
    {
        // (0,0)'dan başlayıp sağa giden 'length' uzunluğunda bant hattı kurar.
        // reversed: bantları sondan başa doğru yerleştirir (güncelleme sırası değişsin diye).
        private static Belt[] PlaceLine(World world, int length, bool reversed = false)
        {
            var line = new Belt[length];
            for (int i = 0; i < length; i++)
                line[i] = new Belt(new GridPos(i, 0), Direction.Right);

            for (int i = 0; i < length; i++)
                world.TryPlace(line[reversed ? length - 1 - i : i]);

            return line;
        }

        [Test]
        public void TryInsert_OnFullBelt_Fails()
        {
            var belt = new Belt(new GridPos(0, 0), Direction.Right);

            Assert.IsTrue(belt.TryInsert(new Item("wheat")));
            Assert.IsFalse(belt.TryInsert(new Item("flour")));
            Assert.AreEqual("wheat", belt.Item.TypeId);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Item_MovesExactlyOneCellPerTick(bool reversed)
        {
            var world = new World(5, 1);
            var line = PlaceLine(world, 4, reversed);
            var wheat = new Item("wheat");
            line[0].TryInsert(wheat);

            world.Tick();
            Assert.IsNull(line[0].Item);
            Assert.AreSame(wheat, line[1].Item);
            Assert.IsNull(line[2].Item);

            world.Tick();
            Assert.AreSame(wheat, line[2].Item);
        }

        [Test]
        public void Item_StopsAtEndOfLine()
        {
            var world = new World(5, 1);
            var line = PlaceLine(world, 2);
            var wheat = new Item("wheat");
            line[0].TryInsert(wheat);

            for (int i = 0; i < 5; i++) world.Tick();

            Assert.AreSame(wheat, line[1].Item);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void FullLine_WithFreeHead_MovesTogether(bool reversed)
        {
            var world = new World(5, 1);
            var line = PlaceLine(world, 4, reversed);
            line[0].TryInsert(new Item("a"));
            line[1].TryInsert(new Item("b"));
            line[2].TryInsert(new Item("c"));

            world.Tick();

            Assert.IsNull(line[0].Item);
            Assert.AreEqual("a", line[1].Item.TypeId);
            Assert.AreEqual("b", line[2].Item.TypeId);
            Assert.AreEqual("c", line[3].Item.TypeId);
        }

        [Test]
        public void FullLine_WithBlockedHead_DoesNotMove()
        {
            var world = new World(5, 1);
            var line = PlaceLine(world, 3);
            line[0].TryInsert(new Item("a"));
            line[1].TryInsert(new Item("b"));
            line[2].TryInsert(new Item("c"));

            world.Tick();

            Assert.AreEqual("a", line[0].Item.TypeId);
            Assert.AreEqual("b", line[1].Item.TypeId);
            Assert.AreEqual("c", line[2].Item.TypeId);
        }

        [Test]
        public void Merge_OnlyOneItemEntersPerTick()
        {
            // (0,1)→ ve (1,0)↑ ikisi de (1,1)'e akıyor; (1,1) sağa, (2,1)'e.
            var world = new World(3, 2);
            var fromLeft = new Belt(new GridPos(0, 1), Direction.Right);
            var fromBelow = new Belt(new GridPos(1, 0), Direction.Up);
            var middle = new Belt(new GridPos(1, 1), Direction.Right);
            var end = new Belt(new GridPos(2, 1), Direction.Right);
            world.TryPlace(fromLeft);
            world.TryPlace(fromBelow);
            world.TryPlace(middle);
            world.TryPlace(end);
            fromLeft.TryInsert(new Item("a"));
            fromBelow.TryInsert(new Item("b"));

            world.Tick();

            int waiting = (fromLeft.Item != null ? 1 : 0) + (fromBelow.Item != null ? 1 : 0);
            Assert.AreEqual(1, waiting);
            Assert.IsNotNull(middle.Item);
        }

        [Test]
        public void FullLoop_DoesNotMove_AndDoesNotHang()
        {
            // 2x2 saat yönü döngü: (0,0)↑ (0,1)→ (1,1)↓ (1,0)←
            var world = new World(2, 2);
            var belts = new[]
            {
                new Belt(new GridPos(0, 0), Direction.Up),
                new Belt(new GridPos(0, 1), Direction.Right),
                new Belt(new GridPos(1, 1), Direction.Down),
                new Belt(new GridPos(1, 0), Direction.Left)
            };
            foreach (var b in belts)
            {
                world.TryPlace(b);
                b.TryInsert(new Item(b.Position.ToString()));
            }

            world.Tick();

            foreach (var b in belts)
                Assert.AreEqual(b.Position.ToString(), b.Item.TypeId);
        }

        [Test]
        public void LoopWithGap_Rotates()
        {
            var world = new World(2, 2);
            var a = new Belt(new GridPos(0, 0), Direction.Up);
            var b = new Belt(new GridPos(0, 1), Direction.Right);
            var c = new Belt(new GridPos(1, 1), Direction.Down);
            var d = new Belt(new GridPos(1, 0), Direction.Left);
            world.TryPlace(a);
            world.TryPlace(b);
            world.TryPlace(c);
            world.TryPlace(d);
            var x = new Item("x");
            var y = new Item("y");
            var z = new Item("z");
            a.TryInsert(x);
            b.TryInsert(y);
            c.TryInsert(z); // d boş

            world.Tick();

            Assert.AreSame(z, d.Item);
            Assert.AreSame(y, c.Item);
            Assert.AreSame(x, b.Item);
            Assert.IsNull(a.Item);
        }

        [Test]
        public void RemovedBelt_IsNoLongerUpdated()
        {
            var world = new World(5, 1);
            var line = PlaceLine(world, 3);
            line[0].TryInsert(new Item("wheat"));

            world.Remove(new GridPos(0, 0));
            world.Tick();

            // Silinen bant artık dünyada değil; eşyası hatta geçmemeli.
            Assert.IsNull(line[1].Item);
        }

        [Test]
        public void Tick_IncrementsTickCount()
        {
            var world = new World(1, 1);

            world.Tick();
            world.Tick();

            Assert.AreEqual(2, world.TickCount);
        }
    }
}
