using System;

namespace Sweetory.Core
{
    // Tarifte "şu eşyadan şu kadar" bilgisi. Örn: (flour, 2).
    public readonly struct ItemAmount
    {
        public readonly string ItemId;
        public readonly int Count;

        public ItemAmount(string itemId, int count)
        {
            if (string.IsNullOrEmpty(itemId)) throw new ArgumentException("Eşya id'si boş olamaz", nameof(itemId));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "Adet pozitif olmalı");

            ItemId = itemId;
            Count = count;
        }

        public override string ToString() => $"{Count}x {ItemId}";
    }
}
