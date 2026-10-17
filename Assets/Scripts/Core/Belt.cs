namespace Sweetory.Core
{
    // Konveyör bant. Eşyaları Direction yönündeki komşu hücreye taşır (taşıma mantığı sonraki adımda).
    public class Belt : Structure
    {
        public Direction Direction { get; }

        public Belt(GridPos position, Direction direction) : base(position)
        {
            Direction = direction;
        }

        // Bandın eşyayı bırakacağı hücre.
        public GridPos OutputPosition => Position + Direction.ToOffset();
    }
}
