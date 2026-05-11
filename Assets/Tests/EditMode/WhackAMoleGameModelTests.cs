using NUnit.Framework;
using SentryGame.Common;
using SentryGame.WhackAMole;

namespace SentryGame.Tests
{
    public sealed class WhackAMoleGameModelTests
    {
        [Test]
        public void HitActiveMole_AddsScoreAndClearsMole()
        {
            var model = new WhackAMoleGameModel(5);
            model.SpawnMole(1.5f);

            model.HitActiveMole();

            Assert.AreEqual(6, model.Score);
            Assert.IsFalse(model.HasActiveMole);
            Assert.AreEqual(GameState.Running, model.State);
        }

        [Test]
        public void Tick_RemovesExpiredMoleAndSubtractsScore()
        {
            var model = new WhackAMoleGameModel(5);
            model.SpawnMole(1f);

            model.Tick(1.1f);

            Assert.AreEqual(4, model.Score);
            Assert.IsFalse(model.HasActiveMole);
        }

        [Test]
        public void Tick_EndsGame_WhenScoreDropsToZero()
        {
            var model = new WhackAMoleGameModel(1);
            model.SpawnMole(1f);

            model.Tick(1.1f);

            Assert.AreEqual(0, model.Score);
            Assert.AreEqual(GameState.GameOver, model.State);
        }

        [Test]
        public void Constructor_EndsGame_WhenInitialScoreIsNotPositive()
        {
            var model = new WhackAMoleGameModel(0);

            Assert.AreEqual(0, model.Score);
            Assert.AreEqual(GameState.GameOver, model.State);
        }

        [Test]
        public void SpawnMole_IgnoresNonPositiveLifeTime()
        {
            var model = new WhackAMoleGameModel(5);

            model.SpawnMole(0f);

            Assert.IsFalse(model.HasActiveMole);
        }
    }
}
