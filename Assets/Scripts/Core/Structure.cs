namespace Sweetory.Core
{
    // Grid'de bir hücre kaplayan her şeyin ortak tabanı (bina, bant).
    // abstract: tek başına oluşturulamaz, sadece alt sınıfları (Building, Belt) oluşturulur.
    public abstract class Structure
    {
        public GridPos Position { get; }

        protected Structure(GridPos position)
        {
            Position = position;
        }
    }
}
