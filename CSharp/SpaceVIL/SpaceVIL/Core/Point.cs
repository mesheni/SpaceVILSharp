namespace SpaceVIL.Core
{
    internal class Point
    {
        internal int X { get; set; }
        internal int Y { get; set; }

        internal Point() {
            X = 0;
            Y = 0;
        }

        internal Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        internal Point(Point point)
        {
            X = point.X;
            Y = point.Y;
        }
    }
}