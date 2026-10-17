using System;

namespace Sweetory.Core
{
    // Sabit boyutlu 2D hücre deposu. Boş hücre = null.
    // Veriler tek boyutlu dizide tutulur (index = y * Width + x); bellekte ardışık, hızlı erişim.
    public class Grid<T> where T : class
    {
        public int Width { get; }
        public int Height { get; }

        private readonly T[] cells;

        public Grid(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            Width = width;
            Height = height;
            cells = new T[width * height];
        }

        public bool InBounds(GridPos pos) =>
            pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;

        public T Get(GridPos pos) => cells[IndexOf(pos)];

        public void Set(GridPos pos, T value) => cells[IndexOf(pos)] = value;

        public bool IsEmpty(GridPos pos) => Get(pos) == null;

        private int IndexOf(GridPos pos)
        {
            if (!InBounds(pos)) throw new ArgumentOutOfRangeException(nameof(pos), $"{pos} grid dışında");
            return pos.Y * Width + pos.X;
        }
    }
}
