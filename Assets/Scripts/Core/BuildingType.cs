using System;

namespace Sweetory.Core
{
    // Bina tanımı (veriden gelir): "mill" = flour tarifini çalıştıran bina gibi.
    // Building ise bu tipin dünyaya yerleştirilmiş tek bir örneği.
    public class BuildingType
    {
        public string Id { get; }

        // null olabilir: tarifsiz bina (dekor vb.) hiçbir şey üretmez.
        public Recipe Recipe { get; }

        public BuildingType(string id, Recipe recipe)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Bina id'si boş olamaz", nameof(id));

            Id = id;
            Recipe = recipe;
        }

        public Building CreateAt(GridPos position) => new Building(position, Id, Recipe);
    }
}
