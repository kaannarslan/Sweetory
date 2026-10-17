using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sweetory.Core
{
    // Oyunun veri tanımları (tarifler, bina tipleri). JSON metninden yüklenir.
    // Dosya okumayı bilmez: metni oyun katmanı verir (Unity'de TextAsset, testte düz string).
    public class GameData
    {
        public IReadOnlyDictionary<string, Recipe> Recipes { get; }
        public IReadOnlyDictionary<string, BuildingType> BuildingTypes { get; }

        private GameData(Dictionary<string, Recipe> recipes, Dictionary<string, BuildingType> buildingTypes)
        {
            Recipes = recipes;
            BuildingTypes = buildingTypes;
        }

        // Bilinmeyen alan adı (yazım hatası) da hata sayılır; sessizce yok sayılmasın.
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Error
        };

        public static GameData FromJson(string json)
        {
            GameDataJson raw;
            try
            {
                raw = JsonConvert.DeserializeObject<GameDataJson>(json, Settings);
            }
            catch (JsonException e)
            {
                throw new GameDataException($"Oyun verisi okunamadı: {e.Message}", e);
            }
            if (raw == null) throw new GameDataException("Oyun verisi boş");

            var recipes = new Dictionary<string, Recipe>();
            foreach (var r in raw.Recipes ?? new List<RecipeJson>())
            {
                var recipe = BuildRecipe(r);
                if (recipes.ContainsKey(recipe.Id))
                    throw new GameDataException($"'{recipe.Id}' tarifi birden fazla tanımlı");
                recipes.Add(recipe.Id, recipe);
            }

            var buildingTypes = new Dictionary<string, BuildingType>();
            foreach (var b in raw.Buildings ?? new List<BuildingTypeJson>())
            {
                var type = BuildBuildingType(b, recipes);
                if (buildingTypes.ContainsKey(type.Id))
                    throw new GameDataException($"'{type.Id}' binası birden fazla tanımlı");
                buildingTypes.Add(type.Id, type);
            }

            return new GameData(recipes, buildingTypes);
        }

        private static Recipe BuildRecipe(RecipeJson r)
        {
            try
            {
                return new Recipe(r.Id, r.DurationTicks, BuildAmounts(r.Inputs), BuildAmounts(r.Outputs));
            }
            catch (ArgumentException e)
            {
                // Recipe/ItemAmount kendi doğrulamasını yapar; biz sadece hangi tarif olduğunu ekliyoruz.
                throw new GameDataException($"'{r.Id}' tarifi geçersiz: {e.Message}", e);
            }
        }

        private static ItemAmount[] BuildAmounts(List<ItemAmountJson> list)
        {
            if (list == null) return Array.Empty<ItemAmount>();

            var result = new ItemAmount[list.Count];
            for (int i = 0; i < list.Count; i++)
                result[i] = new ItemAmount(list[i].Item, list[i].Count);
            return result;
        }

        private static BuildingType BuildBuildingType(BuildingTypeJson b, Dictionary<string, Recipe> recipes)
        {
            if (string.IsNullOrEmpty(b.Id)) throw new GameDataException("Id'si olmayan bina tanımı var");

            Recipe recipe = null;
            if (b.Recipe != null && !recipes.TryGetValue(b.Recipe, out recipe))
                throw new GameDataException($"'{b.Id}' binası bilinmeyen tarife bağlı: '{b.Recipe}'");

            return new BuildingType(b.Id, recipe);
        }

        // --- JSON'un birebir karşılığı olan ham sınıflar (DTO). Sadece okuma için; oyun mantığı bunları görmez. ---
        // Newtonsoft alan adlarını büyük/küçük harf duyarsız eşler: "durationTicks" ↔ DurationTicks.

        private class GameDataJson
        {
            public List<RecipeJson> Recipes { get; set; }
            public List<BuildingTypeJson> Buildings { get; set; }
        }

        private class RecipeJson
        {
            public string Id { get; set; }
            public int DurationTicks { get; set; }
            public List<ItemAmountJson> Inputs { get; set; }
            public List<ItemAmountJson> Outputs { get; set; }
        }

        private class ItemAmountJson
        {
            public string Item { get; set; }
            public int Count { get; set; }
        }

        private class BuildingTypeJson
        {
            public string Id { get; set; }
            public string Recipe { get; set; }
        }
    }
}
