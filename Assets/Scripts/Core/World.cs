using System.Collections.Generic;

namespace Sweetory.Core
{
    // Oyun dünyası: grid'i tutar, yerleştirme/silme kurallarını uygular ve simülasyonu tick tick ilerletir.
    // Oyuncu girdisiyle çağrılacağı için geçersiz isteklerde hata fırlatmaz, false/null döner.
    public class World
    {
        private static readonly Direction[] AllDirections =
            { Direction.Up, Direction.Right, Direction.Down, Direction.Left };

        private readonly Grid<Structure> grid;

        // Tick'te her hücreyi taramak yerine sadece bantları/binaları dolaşmak için ayrı listeler.
        private readonly List<Belt> belts = new List<Belt>();
        private readonly List<Building> buildings = new List<Building>();

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
            if (structure is Building building) buildings.Add(building);
            return true;
        }

        // Hücredeki yapıyı kaldırır ve döndürür; kaldırılacak bir şey yoksa null.
        public Structure Remove(GridPos pos)
        {
            var existing = GetAt(pos);
            if (existing == null) return null;

            grid.Set(pos, null);
            if (existing is Belt belt) belts.Remove(belt);
            if (existing is Building building) buildings.Remove(building);
            return existing;
        }

        // Simülasyonu bir adım ilerletir.
        // Sıra: bantlar ilerler (binalara teslim dahil) → makineler üretir → makineler çıktıyı bantlara bırakır.
        // Bantların en başta olması, bu tick banda bırakılan eşyanın aynı tick'te bir de ilerlemesini engeller.
        public void Tick()
        {
            TickCount++;

            foreach (var belt in belts)
                UpdateBelt(belt);

            foreach (var building in buildings)
                building.Tick();

            foreach (var building in buildings)
                PushOutputs(building);
        }

        // Bandın eşyasını önündeki banda veya binaya taşımaya çalışır.
        // Önce öndeki bandın ilerlemesine izin verir; böylece dolu bir hat tek tick'te bütün olarak kayar.
        // Her bant tick başına bir kez işlenir: eşya 1 hücreden fazla gidemez ve döngüler sonsuza girmez.
        private void UpdateBelt(Belt belt)
        {
            if (belt.LastUpdateTick == TickCount) return;
            belt.LastUpdateTick = TickCount;

            if (belt.Item == null) return;

            var target = GetAt(belt.OutputPosition);

            if (target is Building building)
            {
                // Bina kabul etmezse (tarifte yok / depo dolu) eşya bantta bekler.
                if (building.TryInsert(belt.Item)) belt.TakeItem();
                return;
            }

            if (target is not Belt next) return;

            if (next.Item != null) UpdateBelt(next);
            if (next.Item != null) return; // öndeki ilerleyemedi: bekle

            next.TryInsert(belt.TakeItem());
            next.LastUpdateTick = TickCount; // yeni gelen eşya bu tick bir daha hareket etmesin
        }

        // Binanın çıktı deposundaki eşyaları komşu boş bantlara bırakır (her banda en fazla 1).
        // Binanın içine doğru bakan bantlar girdi bandıdır; onlara çıktı bırakılmaz.
        private void PushOutputs(Building building)
        {
            foreach (var dir in AllDirections)
            {
                if (GetAt(building.Position + dir.ToOffset()) is not Belt belt) continue;
                if (belt.Item != null || belt.OutputPosition == building.Position) continue;

                var item = building.TakeOutput();
                if (item == null) return; // depo boşaldı

                belt.TryInsert(item);
            }
        }
    }
}
