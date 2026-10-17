using System;

namespace Sweetory.Core
{
    // Grid üzerindeki bir hücrenin koordinatı. (0,0) sol alt köşe.
    // struct + readonly: değer tipi, kopyalanınca değişmez; Dictionary anahtarı olarak güvenli.
    public readonly struct GridPos : IEquatable<GridPos>
    {
        public readonly int X;
        public readonly int Y;

        public GridPos(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static GridPos operator +(GridPos a, GridPos b) => new GridPos(a.X + b.X, a.Y + b.Y);

        public static bool operator ==(GridPos a, GridPos b) => a.Equals(b);
        public static bool operator !=(GridPos a, GridPos b) => !a.Equals(b);

        public bool Equals(GridPos other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is GridPos other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X}, {Y})";
    }
}
