using System.Collections.Generic;

namespace Sweetory.Core
{
    // Makine veya üretici (tarla, değirmen, fırın...). MVP'de 1x1.
    // Tarifi varsa her tick girdi deposundan tüketip çıktı deposuna üretir; tarifi yoksa hiçbir şey yapmaz.
    public class Building : Structure
    {
        // Depolar tarifteki miktarın bu katı kadar eşya tutar (örn. tarif 2 un istiyorsa en fazla 4 un).
        public const int BufferMultiplier = 2;

        public string TypeId { get; }
        public Recipe Recipe { get; }

        public bool IsCrafting { get; private set; }

        // Mevcut üretimde geçen tick sayısı (0..Recipe.DurationTicks).
        public int Progress { get; private set; }

        // eşya id → adet
        private readonly Dictionary<string, int> inputBuffer = new Dictionary<string, int>();
        private readonly Dictionary<string, int> outputBuffer = new Dictionary<string, int>();

        public Building(GridPos position, string typeId, Recipe recipe = null) : base(position)
        {
            TypeId = typeId;
            Recipe = recipe;
        }

        public int GetInputCount(string itemId) => inputBuffer.TryGetValue(itemId, out var n) ? n : 0;
        public int GetOutputCount(string itemId) => outputBuffer.TryGetValue(itemId, out var n) ? n : 0;

        // Bu eşya tarifin girdisi mi ve depoda yeri var mı?
        public bool CanAccept(string itemId)
        {
            if (Recipe == null) return false;

            int needed = Recipe.InputCountOf(itemId);
            return needed > 0 && GetInputCount(itemId) < needed * BufferMultiplier;
        }

        public bool TryInsert(Item item)
        {
            if (!CanAccept(item.TypeId)) return false;

            inputBuffer[item.TypeId] = GetInputCount(item.TypeId) + 1;
            return true;
        }

        // Çıktı deposundan bir eşya alır (tarifteki çıktı sırasıyla); depo boşsa null.
        public Item TakeOutput()
        {
            if (Recipe == null) return null;

            foreach (var output in Recipe.Outputs)
            {
                int count = GetOutputCount(output.ItemId);
                if (count == 0) continue;

                outputBuffer[output.ItemId] = count - 1;
                return new Item(output.ItemId);
            }
            return null;
        }

        // Üretimi bir tick ilerletir.
        public void Tick()
        {
            if (Recipe == null) return;

            if (!IsCrafting && HasInputs() && HasOutputSpace())
            {
                foreach (var input in Recipe.Inputs)
                    inputBuffer[input.ItemId] -= input.Count;

                IsCrafting = true;
                Progress = 0;
            }

            if (!IsCrafting) return;

            Progress++;
            if (Progress < Recipe.DurationTicks) return;

            foreach (var output in Recipe.Outputs)
                outputBuffer[output.ItemId] = GetOutputCount(output.ItemId) + output.Count;

            IsCrafting = false;
            Progress = 0;
        }

        private bool HasInputs()
        {
            foreach (var input in Recipe.Inputs)
                if (GetInputCount(input.ItemId) < input.Count) return false;
            return true;
        }

        // Üretim bitince çıktılar depoya sığacak mı? Başlamadan kontrol edilir ki bitince taşma olmasın.
        private bool HasOutputSpace()
        {
            foreach (var output in Recipe.Outputs)
                if (GetOutputCount(output.ItemId) + output.Count > output.Count * BufferMultiplier) return false;
            return true;
        }
    }
}
