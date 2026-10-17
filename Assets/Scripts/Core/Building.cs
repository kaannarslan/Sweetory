namespace Sweetory.Core
{
    // Makine veya üretici (tarla, değirmen, fırın...). MVP'de 1x1.
    // Davranış (girdi/çıktı/tarif) sonraki adımlarda veri odaklı olarak eklenecek;
    // şimdilik sadece hangi tip olduğunu biliyor.
    public class Building : Structure
    {
        public string TypeId { get; }

        public Building(GridPos position, string typeId) : base(position)
        {
            TypeId = typeId;
        }
    }
}
