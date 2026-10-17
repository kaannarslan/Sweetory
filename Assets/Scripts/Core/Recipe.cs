using System;
using System.Collections.Generic;

namespace Sweetory.Core
{
    // Üretim tarifi: girdileri DurationTicks tick'te çıktılara dönüştürür.
    // Girdisi olmayan tarif de geçerlidir (örn. tarla: hiçlikten buğday üretir).
    public class Recipe
    {
        public string Id { get; }
        public int DurationTicks { get; }
        public IReadOnlyList<ItemAmount> Inputs { get; }
        public IReadOnlyList<ItemAmount> Outputs { get; }

        public Recipe(string id, int durationTicks, ItemAmount[] inputs, ItemAmount[] outputs)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Tarif id'si boş olamaz", nameof(id));
            if (durationTicks <= 0) throw new ArgumentOutOfRangeException(nameof(durationTicks), "Süre en az 1 tick olmalı");
            if (outputs == null || outputs.Length == 0) throw new ArgumentException("Tarifin en az bir çıktısı olmalı", nameof(outputs));

            Id = id;
            DurationTicks = durationTicks;
            // Kopyalıyoruz: dışarıdaki dizi sonradan değişirse tarif etkilenmesin.
            Inputs = (ItemAmount[])(inputs ?? Array.Empty<ItemAmount>()).Clone();
            Outputs = (ItemAmount[])outputs.Clone();
        }

        // Bu eşya tarifin girdisiyse kaç tane gerektiğini, değilse 0 döner.
        public int InputCountOf(string itemId) => CountOf(Inputs, itemId);

        public int OutputCountOf(string itemId) => CountOf(Outputs, itemId);

        private static int CountOf(IReadOnlyList<ItemAmount> list, string itemId)
        {
            foreach (var amount in list)
                if (amount.ItemId == itemId) return amount.Count;
            return 0;
        }
    }
}
