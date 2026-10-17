namespace Sweetory.Core
{
    // Saat yönünde sıralı; böylece döndürme = (değer + 1) % 4.
    public enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }

    public static class DirectionExtensions
    {
        // Bu yönde bir adım atınca koordinata eklenecek fark. Y yukarı doğru artar.
        public static GridPos ToOffset(this Direction dir) => dir switch
        {
            Direction.Up => new GridPos(0, 1),
            Direction.Right => new GridPos(1, 0),
            Direction.Down => new GridPos(0, -1),
            Direction.Left => new GridPos(-1, 0),
            _ => throw new System.ArgumentOutOfRangeException(nameof(dir))
        };

        public static Direction RotateClockwise(this Direction dir) => (Direction)(((int)dir + 1) % 4);

        public static Direction Opposite(this Direction dir) => (Direction)(((int)dir + 2) % 4);
    }
}
