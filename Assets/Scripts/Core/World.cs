namespace Sweetory.Core
{
    // Oyun dünyası: grid'i tutar ve yerleştirme/silme kurallarını uygular.
    // Oyuncu girdisiyle çağrılacağı için geçersiz isteklerde hata fırlatmaz, false/null döner.
    public class World
    {
        private readonly Grid<Structure> grid;

        public int Width => grid.Width;
        public int Height => grid.Height;

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
            return true;
        }

        // Hücredeki yapıyı kaldırır ve döndürür; kaldırılacak bir şey yoksa null.
        public Structure Remove(GridPos pos)
        {
            var existing = GetAt(pos);
            if (existing != null) grid.Set(pos, null);
            return existing;
        }
    }
}
