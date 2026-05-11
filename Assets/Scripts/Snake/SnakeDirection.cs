namespace SentryGame.Snake
{
    public enum SnakeDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public static class SnakeDirectionExtensions
    {
        public static bool IsDirectReverseOf(this SnakeDirection direction, SnakeDirection other)
        {
            return direction == SnakeDirection.Up && other == SnakeDirection.Down
                || direction == SnakeDirection.Down && other == SnakeDirection.Up
                || direction == SnakeDirection.Left && other == SnakeDirection.Right
                || direction == SnakeDirection.Right && other == SnakeDirection.Left;
        }

        public static GridPoint ToVector(this SnakeDirection direction)
        {
            // 将输入方向转换成单步网格偏移，供蛇头计算下一格位置。
            switch (direction)
            {
                case SnakeDirection.Up:
                    return new GridPoint(0, -1);
                case SnakeDirection.Down:
                    return new GridPoint(0, 1);
                case SnakeDirection.Left:
                    return new GridPoint(-1, 0);
                default:
                    return new GridPoint(1, 0);
            }
        }
    }
}
