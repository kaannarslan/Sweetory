using System;
using NUnit.Framework;

namespace Sweetory.Core.Tests
{
    public class BuildingTests
    {
        private static readonly GridPos Origin = new GridPos(0, 0);

        // 1 buğday → 1 un, 3 tick
        private static Recipe MillRecipe() => new Recipe("flour", 3,
            new[] { new ItemAmount("wheat", 1) },
            new[] { new ItemAmount("flour", 1) });

        // 2 un + 1 şeker → 1 kek, 2 tick
        private static Recipe CakeRecipe() => new Recipe("cake", 2,
            new[] { new ItemAmount("flour", 2), new ItemAmount("sugar", 1) },
            new[] { new ItemAmount("cake", 1) });

        // girdi yok → 1 buğday, 2 tick
        private static Recipe FieldRecipe() => new Recipe("wheat", 2,
            null,
            new[] { new ItemAmount("wheat", 1) });

        private static void TickTimes(Building building, int times)
        {
            for (int i = 0; i < times; i++) building.Tick();
        }

        [Test]
        public void Recipe_InvalidValues_Throw()
        {
            var output = new[] { new ItemAmount("flour", 1) };

            Assert.Throws<ArgumentOutOfRangeException>(() => new Recipe("x", 0, null, output));
            Assert.Throws<ArgumentException>(() => new Recipe("", 1, null, output));
            Assert.Throws<ArgumentException>(() => new Recipe("x", 1, null, new ItemAmount[0]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ItemAmount("wheat", 0));
        }

        [Test]
        public void CanAccept_OnlyRecipeInputs()
        {
            var mill = new Building(Origin, "mill", MillRecipe());

            Assert.IsTrue(mill.CanAccept("wheat"));
            Assert.IsFalse(mill.CanAccept("flour"));
            Assert.IsFalse(mill.TryInsert(new Item("sugar")));
        }

        [Test]
        public void InputBuffer_IsLimited()
        {
            var mill = new Building(Origin, "mill", MillRecipe());

            // tarif 1 buğday istiyor → depo en fazla 1 * BufferMultiplier = 2
            Assert.IsTrue(mill.TryInsert(new Item("wheat")));
            Assert.IsTrue(mill.TryInsert(new Item("wheat")));
            Assert.IsFalse(mill.TryInsert(new Item("wheat")));
            Assert.AreEqual(2, mill.GetInputCount("wheat"));
        }

        [Test]
        public void Crafting_ConsumesInputs_AndProducesAfterDuration()
        {
            var mill = new Building(Origin, "mill", MillRecipe());
            mill.TryInsert(new Item("wheat"));

            mill.Tick(); // başlar: girdi hemen tüketilir
            Assert.IsTrue(mill.IsCrafting);
            Assert.AreEqual(0, mill.GetInputCount("wheat"));

            mill.Tick();
            Assert.AreEqual(0, mill.GetOutputCount("flour"));

            mill.Tick(); // 3. tick: süre doldu
            Assert.AreEqual(1, mill.GetOutputCount("flour"));
            Assert.IsFalse(mill.IsCrafting);
        }

        [Test]
        public void WithoutInputs_DoesNotCraft()
        {
            var mill = new Building(Origin, "mill", MillRecipe());

            TickTimes(mill, 10);

            Assert.IsFalse(mill.IsCrafting);
            Assert.AreEqual(0, mill.GetOutputCount("flour"));
        }

        [Test]
        public void MissingOneOfMultipleInputs_DoesNotCraft()
        {
            var oven = new Building(Origin, "oven", CakeRecipe());
            oven.TryInsert(new Item("flour"));
            oven.TryInsert(new Item("flour"));

            TickTimes(oven, 5);
            Assert.IsFalse(oven.IsCrafting);

            oven.TryInsert(new Item("sugar"));
            TickTimes(oven, 2);
            Assert.AreEqual(1, oven.GetOutputCount("cake"));
            Assert.AreEqual(0, oven.GetInputCount("flour"));
        }

        [Test]
        public void Field_ProducesWithoutInputs_UntilOutputFull()
        {
            var field = new Building(Origin, "field", FieldRecipe());

            TickTimes(field, 20);

            // çıktı deposu 1 * BufferMultiplier = 2 ile dolu; yeni üretime başlamamalı
            Assert.AreEqual(2, field.GetOutputCount("wheat"));
            Assert.IsFalse(field.IsCrafting);
        }

        [Test]
        public void TakingOutput_ResumesProduction()
        {
            var field = new Building(Origin, "field", FieldRecipe());
            TickTimes(field, 20);

            var taken = field.TakeOutput();
            Assert.AreEqual("wheat", taken.TypeId);
            Assert.AreEqual(1, field.GetOutputCount("wheat"));

            TickTimes(field, 2);
            Assert.AreEqual(2, field.GetOutputCount("wheat"));
        }

        [Test]
        public void TakeOutput_WhenEmpty_ReturnsNull()
        {
            var mill = new Building(Origin, "mill", MillRecipe());

            Assert.IsNull(mill.TakeOutput());
        }

        [Test]
        public void BuildingWithoutRecipe_DoesNothing()
        {
            var building = new Building(Origin, "decoration");

            building.Tick();

            Assert.IsFalse(building.CanAccept("wheat"));
            Assert.IsNull(building.TakeOutput());
            Assert.IsFalse(building.IsCrafting);
        }
    }
}
