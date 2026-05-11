using SentryGame.Common;

namespace SentryGame.WhackAMole
{
    public sealed class WhackAMoleGameModel
    {
        private float remainingMoleTime;

        public WhackAMoleGameModel(int initialScore)
        {
            Score = initialScore;
            State = GameState.Running;
        }

        public int Score { get; private set; }

        public GameState State { get; private set; }

        public bool HasActiveMole { get; private set; }

        public void SpawnMole(float lifeTime)
        {
            if (State == GameState.GameOver)
            {
                return;
            }

            remainingMoleTime = lifeTime;
            HasActiveMole = true;
        }

        public void HitActiveMole()
        {
            if (State == GameState.GameOver || !HasActiveMole)
            {
                return;
            }

            Score += 1;
            HasActiveMole = false;
        }

        public void Tick(float deltaTime)
        {
            // 推进当前地鼠生命周期，并在超时后扣分或结束游戏。
            if (State == GameState.GameOver || !HasActiveMole)
            {
                return;
            }

            remainingMoleTime -= deltaTime;
            if (remainingMoleTime > 0f)
            {
                return;
            }

            Score -= 1;
            HasActiveMole = false;
            if (Score <= 0)
            {
                Score = 0;
                State = GameState.GameOver;
            }
        }
    }
}
