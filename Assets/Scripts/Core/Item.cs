namespace Sweetory.Core
{
    // Tek bir eşya (buğday, un, ekmek...). Tipi veri odaklı: TypeId ileride tarif/eşya tanımlarına bağlanacak.
    public class Item
    {
        public string TypeId { get; }

        public Item(string typeId)
        {
            TypeId = typeId;
        }

        public override string ToString() => TypeId;
    }
}
