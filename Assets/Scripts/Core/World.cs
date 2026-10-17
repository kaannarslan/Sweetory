using System.Collections.Generic;

namespace Sweetory.Core
{
    // Oyun dünyası: grid'i tutar, yerleştirme/silme kurallarını uygular ve simülasyonu tick tick ilerletir.
    // Oyuncu girdisiyle çağrılacağı için geçersiz isteklerde hata fırlatmaz, false/null döner.
    public class World
    {
        private readonly Grid<Structure> grid;

        // Tick'te her hücreyi taramak yerine sadece bantları dolaşmak için ayrı liste.
        private readonly List<Belt> belts = new List<Belt>();

        public int Width => grid.Width;
        public int Height => grid.Height;

        // Şimdiye kadar çalışan tick sayısı. Gerçek zamanla bağlantısı yok; onu oyun katmanı belirler.
        public long TickCount { get; private set; }

        public World(int width, int height)
        {
            grid = new Grid<Structure>(width, height);
        }

        public bool InBounds(GridPos pos) => grid.InBounds(pos);

        // Hücredeki yapı; boşsa veya grid dışındaysa null.
        public Structure GetAt(GridPos pos) => grid.InBounds(pos) ? grid.Get(pos) : null;

        public bool CanPlace(GridPos pos) => grid.InBounds(pos) && grid.IsEmpty(pos);

        // Yapıyı kendi Position'ına yerleştirir. Hücre doluysa veya grid dışındaysa false.
        public bool TryPlace(Structure structure)
        {
            if (!CanPlace(structure.Position)) return false;

            grid.Set(structure.Position, structure);
            if (structure is Belt belt) belts.Add(belt);
            return true;
        }

        // Hücredeki yapıyı kaldırır ve döndürür; kaldırılacak bir şey yoksa null.
        public Structure Remove(GridPos pos)
        {
            var existing = GetAt(pos);
            if (existing == null) return null;

            grid.Set(pos, null);
            if (existing is Belt belt) belts.Remove(belt);
            return existing;
        }

        // Simülasyonu bir adım ilerletir.
        public void Tick()
        {
            TickCount++;

            foreach (var belt in belts)
                UpdateBelt(belt);
        }

        // Bandın eşyasını önündeki banda taşımaya çalışır.
        // Önce öndeki bandın ilerlemesine izin verir; böylece dolu bir hat tek tick'te bütün olarak kayar.
        // Her bant tick başına bir kez işlenir: eşya 1 hücreden fazla gidemez ve döngüler sonsuza girmez.
        private void UpdateBelt(Belt belt)
        {
            if (belt.LastUpdateTick == TickCount) return;
            belt.LastUpdateTick = TickCount;

            if (belt.Item == null) return;
            if (GetAt(belt.OutputPosition) is not Belt next) return;

            if (next.Item != null) UpdateBelt(next);
            if (next.Item != null) return; // öndeki ilerleyemedi: bekle

            next.TryInsert(belt.TakeItem());
            next.LastUpdateTick = TickCount; // yeni gelen eşya bu tick bir daha hareket etmesin
        }
    }
}
