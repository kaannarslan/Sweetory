using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Sweetory.Core.Tests
{
    public class GameDataTests
    {
        // Newtonsoft tek tırnaklı JSON'u da kabul eder; C# string'i içinde "" kaçışından kurtarıyor.
        private const string ValidJson = @"{
            'recipes': [
                { 'id': 'wheat', 'durationTicks': 2, 'outputs': [ { 'item': 'wheat', 'count': 1 } ] },
                { 'id': 'flour', 'durationTicks': 3,
                  'inputs':  [ { 'item': 'wheat', 'count': 1 } ],
                  'outputs': [ { 'item': 'flour', 'count': 1 } ] }
            ],
            'buildings': [
                { 'id': 'wheat_field', 'recipe': 'wheat' },
                { 'id': 'mill', 'recipe': 'flour' },
                { 'id': 'decoration' }
            ]
        }";

        [Test]
        public void ValidJson_LoadsRecipesAndBuildings()
        {
            var data = GameData.FromJson(ValidJson);

            var flour = data.Recipes["flour"];
            Assert.AreEqual(3, flour.DurationTicks);
            Assert.AreEqual(1, flour.InputCountOf("wheat"));
            Assert.AreEqual(1, flour.OutputCountOf("flour"));
            Assert.AreEqual(0, data.Recipes["wheat"].Inputs.Count);

            // Bina, aynı Recipe nesnesine bağlanmış olmalı (kopya değil).
            Assert.AreSame(flour, data.BuildingTypes["mill"].Recipe);
            Assert.IsNull(data.BuildingTypes["decoration"].Recipe);
        }

        [Test]
        public void BuildingType_CreateAt_MakesWorkingBuilding()
        {
            var data = GameData.FromJson(ValidJson);
            var world = new World(3, 1);
            world.TryPlace(data.BuildingTypes["wheat_field"].CreateAt(new GridPos(0, 0)));
            world.TryPlace(new Belt(new GridPos(1, 0), Direction.Right));
            var mill = data.BuildingTypes["mill"].CreateAt(new GridPos(2, 0));
            world.TryPlace(mill);

            for (int i = 0; i < 20; i++) world.Tick();

            Assert.AreEqual("mill", mill.TypeId);
            Assert.Greater(mill.GetOutputCount("flour"), 0);
        }

        [TestCase("{ bu json değil")]
        [TestCase("{ 'recipes': [ { 'id': 'x', 'durationTicks': 1, 'outputs': [ { 'item': 'a', 'count': 1 } ], 'duraton': 5 } ] }")]
        [TestCase("{ 'recipes': [ { 'id': 'x', 'durationTicks': 0, 'outputs': [ { 'item': 'a', 'count': 1 } ] } ] }")]
        [TestCase("{ 'recipes': [ { 'id': 'x', 'durationTicks': 1, 'outputs': [ { 'item': 'a', 'count': 0 } ] } ] }")]
        [TestCase("{ 'recipes': [ { 'id': 'x', 'durationTicks': 1 } ] }")]
        [TestCase("{ 'buildings': [ { 'id': 'mill', 'recipe': 'yok_boyle_tarif' } ] }")]
        [TestCase("{ 'buildings': [ { 'recipe': 'x' } ] }")]
        public void InvalidJson_ThrowsGameDataException(string json)
        {
            Assert.Throws<GameDataException>(() => GameData.FromJson(json));
        }

        [Test]
        public void DuplicateIds_Throw()
        {
            const string recipeTwice = @"{ 'recipes': [
                { 'id': 'x', 'durationTicks': 1, 'outputs': [ { 'item': 'a', 'count': 1 } ] },
                { 'id': 'x', 'durationTicks': 2, 'outputs': [ { 'item': 'b', 'count': 1 } ] } ] }";
            const string buildingTwice = "{ 'buildings': [ { 'id': 'mill' }, { 'id': 'mill' } ] }";

            Assert.Throws<GameDataException>(() => GameData.FromJson(recipeTwice));
            Assert.Throws<GameDataException>(() => GameData.FromJson(buildingTwice));
        }

        [Test]
        public void InvalidRecipe_ErrorMessageNamesTheRecipe()
        {
            const string json = "{ 'recipes': [ { 'id': 'kek', 'durationTicks': 0, 'outputs': [ { 'item': 'kek', 'count': 1 } ] } ] }";

            var e = Assert.Throws<GameDataException>(() => GameData.FromJson(json));
            StringAssert.Contains("kek", e.Message);
        }

        // Projedeki gerçek veri dosyası her zaman geçerli olmalı.
        [Test]
        public void ProjectGameDataFile_IsValid()
        {
            // Application.dataPath = projedeki Assets klasörünün tam yolu.
            var path = Path.Combine(Application.dataPath, "Data", "gamedata.json");
            var data = GameData.FromJson(File.ReadAllText(path));

            Assert.IsTrue(data.BuildingTypes.ContainsKey("wheat_field"));
            Assert.IsTrue(data.BuildingTypes.ContainsKey("mill"));
            Assert.IsTrue(data.BuildingTypes.ContainsKey("oven"));
        }
    }
}
