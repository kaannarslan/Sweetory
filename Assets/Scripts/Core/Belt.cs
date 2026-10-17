namespace Sweetory.Core
{
    // Konveyör bant. En fazla 1 eşya tutar; her tick eşyayı Direction yönündeki komşu banda taşır.
    public class Belt : Structure
    {
        public Direction Direction { get; }

        // Üzerindeki eşya; boşsa null. Dışarıdan sadece TryInsert/TakeItem ile değişir.
        public Item Item { get; private set; }

        // World'ün bu bandı en son işlediği tick. Aynı tick'te iki kez işlenmesini engeller.
        // internal: sadece Sweetory.Core assembly'si görür, oyun katmanı dokunamaz.
        internal long LastUpdateTick { get; set; } = -1;

        public Belt(GridPos position, Direction direction) : base(position)
        {
            Direction = direction;
        }

        // Bandın eşyayı bırakacağı hücre.
        public GridPos OutputPosition => Position + Direction.ToOffset();

        public bool TryInsert(Item item)
        {
            if (Item != null) return false;

            Item = item;
            return true;
        }

        // Eşyayı banttan alır ve döndürür; bant boşsa null.
        public Item TakeItem()
        {
            var item = Item;
            Item = null;
            return item;
        }
    }
}
