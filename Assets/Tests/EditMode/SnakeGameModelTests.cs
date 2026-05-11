using System;
using NUnit.Framework;
using SentryGame.Common;
using SentryGame.Snake;

namespace SentryGame.Tests
{
    public sealed class SnakeGameModelTests
    {
        [Test]
        public void ChangeDirection_IgnoresDirectReverse()
        {
            var model = new SnakeGameModel(10, 10, new[] { new GridPoint(3, 3), new GridPoint(2, 3) }, new GridPoint(5, 5), SnakeDirection.Right);

            model.ChangeDirection(SnakeDirection.Left);

            Assert.AreEqual(SnakeDirection.Right, model.Direction);
        }

        [Test]
        public void Step_MovesSnakeForward()
        {
            var model = new SnakeGameModel(10, 10, new[] { new GridPoint(3, 3) }, new GridPoint(5, 5), SnakeDirection.Right);

            model.Step();

            Assert.AreEqual(new GridPoint(4, 3), model.Head);
        }

        [Test]
        public void Step_EatsFoodAddsScoreAndGrows()
        {
            var model = new SnakeGameModel(10, 10, new[] { new GridPoint(3, 3) }, new GridPoint(4, 3), SnakeDirection.Right);

            model.Step();

            Assert.AreEqual(1, model.Score);
            Assert.AreEqual(2, model.Body.Count);
            Assert.IsTrue(model.NeedsFood);
        }

        [Test]
        public void Step_EndsGame_WhenHitsWall()
        {
            var model = new SnakeGameModel(5, 5, new[] { new GridPoint(4, 2) }, new GridPoint(1, 1), SnakeDirection.Right);

            model.Step();

            Assert.AreEqual(GameState.GameOver, model.State);
        }

        [Test]
        public void Step_EndsGame_WhenHitsBody()
        {
            var body = new[] { new GridPoint(2, 2), new GridPoint(2, 3), new GridPoint(1, 3), new GridPoint(1, 2) };
            var model = new SnakeGameModel(6, 6, body, new GridPoint(5, 5), SnakeDirection.Down);

            model.Step();

            Assert.AreEqual(GameState.GameOver, model.State);
        }

        [Test]
        public void Constructor_Throws_WhenInitialBodyIsEmpty()
        {
            Assert.Throws<ArgumentException>(() => new SnakeGameModel(10, 10, Array.Empty<GridPoint>(), new GridPoint(5, 5), SnakeDirection.Right));
        }

        [TestCase(0, 10)]
        [TestCase(10, 0)]
        public void Constructor_Throws_WhenMapSizeIsInvalid(int width, int height)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SnakeGameModel(width, height, new[] { new GridPoint(3, 3) }, new GridPoint(5, 5), SnakeDirection.Right));
        }

        [Test]
        public void Constructor_Throws_WhenFoodIsOnBody()
        {
            Assert.Throws<ArgumentException>(() => new SnakeGameModel(10, 10, new[] { new GridPoint(3, 3) }, new GridPoint(3, 3), SnakeDirection.Right));
        }
    }
}
