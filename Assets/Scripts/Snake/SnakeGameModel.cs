using System;
using System.Collections.Generic;
using SentryGame.Common;

namespace SentryGame.Snake
{
    public sealed class SnakeGameModel
    {
        private readonly List<GridPoint> body;
        private readonly IReadOnlyList<GridPoint> readOnlyBody;

        public SnakeGameModel(int width, int height, IEnumerable<GridPoint> initialBody, GridPoint food, SnakeDirection initialDirection)
        {
            // 初始化地图、蛇身和食物，并拒绝会让规则状态不明确的输入。
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            if (initialBody == null)
            {
                throw new ArgumentNullException(nameof(initialBody));
            }

            Width = width;
            Height = height;
            body = new List<GridPoint>(initialBody);
            readOnlyBody = body.AsReadOnly();

            if (body.Count == 0)
            {
                throw new ArgumentException("初始蛇身不能为空。", nameof(initialBody));
            }

            ValidateInsideMap(food, nameof(food));
            if (body.Contains(food))
            {
                throw new ArgumentException("食物不能位于蛇身上。", nameof(food));
            }

            Food = food;
            Direction = initialDirection;
            State = GameState.Running;
        }

        public int Width { get; }

        public int Height { get; }

        public IReadOnlyList<GridPoint> Body => readOnlyBody;

        public GridPoint Head => body[0];

        public GridPoint Food { get; private set; }

        public SnakeDirection Direction { get; private set; }

        public int Score { get; private set; }

        public GameState State { get; private set; }

        public bool NeedsFood { get; private set; }

        public void ChangeDirection(SnakeDirection direction)
        {
            if (direction.IsDirectReverseOf(Direction))
            {
                return;
            }

            Direction = direction;
        }

        public void Step()
        {
            // 推进一格蛇身，按撞墙、撞身、吃食物和普通移动顺序更新状态。
            if (State != GameState.Running)
            {
                return;
            }

            var nextHead = Head.Add(Direction.ToVector());
            var eatsFood = nextHead == Food;
            if (IsOutsideMap(nextHead) || HitsBody(nextHead, eatsFood))
            {
                State = GameState.GameOver;
                return;
            }

            body.Insert(0, nextHead);
            if (eatsFood)
            {
                Score += 1;
                NeedsFood = true;
                return;
            }

            body.RemoveAt(body.Count - 1);
        }

        public void SetFood(GridPoint food)
        {
            ValidateInsideMap(food, nameof(food));
            if (body.Contains(food))
            {
                throw new ArgumentException("食物不能位于蛇身上。", nameof(food));
            }

            Food = food;
            NeedsFood = false;
        }

        private bool IsOutsideMap(GridPoint point)
        {
            return point.X < 0 || point.X >= Width || point.Y < 0 || point.Y >= Height;
        }

        private bool HitsBody(GridPoint point, bool grows)
        {
            // 判断下一格是否撞到蛇身；普通移动时尾巴会先离开，可排除最后一节。
            var checkedLength = grows ? body.Count : body.Count - 1;
            for (var i = 0; i < checkedLength; i += 1)
            {
                if (body[i] == point)
                {
                    return true;
                }
            }

            return false;
        }

        private void ValidateInsideMap(GridPoint point, string parameterName)
        {
            if (IsOutsideMap(point))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
