using NUnit.Framework;

namespace Sweetory.Core.Tests
{
    // Bantlar ile binaların World.Tick() içinde birlikte çalışması.
    public class ProductionTests
    {
        private static Recipe FieldRecipe() => new Recipe("wheat", 2,
            null,
            new[] { new ItemAmount("wheat", 1) });

        private static Recipe MillRecipe() => new Recipe("flour", 3,
            new[] { new ItemAmount("wheat", 1) },
            new[] { new ItemAmount("flour", 1) });

        private static Recipe OvenRecipe() => new Recipe("bread", 4,
            new[] { new ItemAmount("flour", 1) },
            new[] { new ItemAmount("bread", 1) });

        private static void TickTimes(World world, int times)
        {
            for (int i = 0; i < times; i++) world.Tick();
        }

        [Test]
        public void Belt_DeliversAcceptedItem_IntoBuilding()
        {
            var world = new World(2, 1);
            var belt = new Belt(new GridPos(0, 0), Direction.Right);
            var mill = new Building(new GridPos(1, 0), "mill", MillRecipe());
            world.TryPlace(belt);
            world.TryPlace(mill);
            belt.TryInsert(new Item("wheat"));

            world.Tick();

            Assert.IsNull(belt.Item);
            // Aynı tick'te üretime başladığı için girdi zaten tüketildi.
            Assert.IsTrue(mill.IsCrafting);
        }

        [Test]
        public void Belt_KeepsItem_WhenBuildingDoesNotAcceptIt()
        {
            var world = new World(2, 1);
            var belt = new Belt(new GridPos(0, 0), Direction.Right);
            var mill = new Building(new GridPos(1, 0), "mill", MillRecipe());
            world.TryPlace(belt);
            world.TryPlace(mill);
            belt.TryInsert(new Item("sugar"));

            TickTimes(world, 3);

            Assert.AreEqual("sugar", belt.Item.TypeId);
        }

        [Test]
        public void Building_PushesOutput_ToBeltPointingAway()
        {
            // (0,0) bant → değirmen (1,0) → (2,0) bant
            var world = new World(3, 1);
            var input = new Belt(new GridPos(0, 0), Direction.Right);
            var mill = new Building(new GridPos(1, 0), "mill", MillRecipe());
            var output = new Belt(new GridPos(2, 0), Direction.Right);
            world.TryPlace(input);
            world.TryPlace(mill);
            world.TryPlace(output);
            input.TryInsert(new Item("wheat"));

            TickTimes(world, 3); // tick 1: teslim + başla, tick 3: un üretildi ve bırakıldı

            Assert.AreEqual("flour", output.Item?.TypeId);
            Assert.IsNull(input.Item); // giriş bandına un bırakılmamalı
        }

        [Test]
        public void Building_DoesNotPushOutput_ToBeltPointingIntoIt()
        {
            // Tarlanın tek komşusu ona bakan bir bant: çıktı bırakılmamalı.
            var world = new World(2, 1);
            var field = new Building(new GridPos(1, 0), "field", FieldRecipe());
            var belt = new Belt(new GridPos(0, 0), Direction.Right);
            world.TryPlace(field);
            world.TryPlace(belt);

            TickTimes(world, 10);

            Assert.IsNull(belt.Item);
            Assert.AreEqual(2, field.GetOutputCount("wheat"));
        }

        [Test]
        public void RemovedBuilding_IsNoLongerUpdated()
        {
            var world = new World(1, 1);
            var field = new Building(new GridPos(0, 0), "field", FieldRecipe());
            world.TryPlace(field);

            world.Remove(new GridPos(0, 0));
            TickTimes(world, 5);

            Assert.AreEqual(0, field.GetOutputCount("wheat"));
        }

        [Test]
        public void FullChain_FieldToMillToOven_ProducesBread()
        {
            // tarla → bant → değirmen → bant → fırın → bant
            var world = new World(6, 1);
            world.TryPlace(new Building(new GridPos(0, 0), "field", FieldRecipe()));
            world.TryPlace(new Belt(new GridPos(1, 0), Direction.Right));
            world.TryPlace(new Building(new GridPos(2, 0), "mill", MillRecipe()));
            world.TryPlace(new Belt(new GridPos(3, 0), Direction.Right));
            world.TryPlace(new Building(new GridPos(4, 0), "oven", OvenRecipe()));
            var end = new Belt(new GridPos(5, 0), Direction.Right);
            world.TryPlace(end);

            TickTimes(world, 30);

            Assert.AreEqual("bread", end.Item?.TypeId);
        }
    }
}
