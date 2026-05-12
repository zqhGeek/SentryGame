using SentryGame.Common;
using SentryGame.SentryTesting;
using SentryGame.WhackAMole;
using UnityEngine;
using UnityEngine.UI;

namespace SentryGame.UI
{
    public sealed class WhackAMoleSceneController : MonoBehaviour
    {
        private const int InitialScore = 5;
        private const float MoleLifeTime = 1.2f;

        [SerializeField] private Text scoreText = null;
        [SerializeField] private Text statusText = null;
        [SerializeField] private RectTransform playArea = null;
        [SerializeField] private Image moleImage = null;
        [SerializeField] private Button moleButton = null;
        [SerializeField] private Button restartButton = null;
        [SerializeField] private Button backButton = null;

        private WhackAMoleGameModel model;
        private bool gameOverRecorded;

        private void Awake()
        {
            // 绑定红点、重开和返回按钮，确保场景引用或运行时补齐的 UI 都能响应点击。
            EnsureReferences();
            if (moleButton != null)
            {
                moleButton.onClick.AddListener(HitMole);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(ReturnToModeSelect);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartGame);
            }
        }

        private void Start()
        {
            RestartGame();
        }

        private void Update()
        {
            if (model == null || model.State == GameState.GameOver)
            {
                return;
            }

            var hadActiveMole = model.HasActiveMole;
            var previousScore = model.Score;
            model.Tick(Time.deltaTime);
            if (hadActiveMole && !model.HasActiveMole && model.Score < previousScore)
            {
                SentryTelemetryService.RecordMoleMissed(model.Score);
            }

            if (model.State == GameState.GameOver)
            {
                RecordGameOverOnce("score_zero");
            }

            if (!model.HasActiveMole && model.State == GameState.Running)
            {
                SpawnMole();
            }

            RefreshView();
        }

        public void HitMole()
        {
            if (model == null)
            {
                return;
            }

            var previousScore = model.Score;
            model.HitActiveMole();
            if (model.Score > previousScore)
            {
                SentryTelemetryService.RecordMoleHit(model.Score);
            }

            if (model.State == GameState.Running)
            {
                SpawnMole();
            }

            RefreshView();
        }

        public void ReturnToModeSelect()
        {
            SentryTelemetryService.RecordModeSelected("return", GameSceneNames.ModeSelectScene);
            SceneLoader.LoadScene(GameSceneNames.ModeSelectScene);
        }

        public void RestartGame()
        {
            model = new WhackAMoleGameModel(InitialScore);
            gameOverRecorded = false;
            SentryTelemetryService.RecordModeSelected("whack_a_mole_restart", GameSceneNames.WhackAMoleScene);
            SpawnMole();
            RefreshView();
        }

        private void RecordGameOverOnce(string reason)
        {
            if (gameOverRecorded || model == null)
            {
                return;
            }

            gameOverRecorded = true;
            SentryTelemetryService.RecordWhackAMoleGameOver(model.Score, reason);
        }

        private void SpawnMole()
        {
            // 在玩法区域内随机移动红点，并通知模型开始新的地鼠生命周期。
            if (model == null || moleImage == null)
            {
                return;
            }

            model.SpawnMole(MoleLifeTime);
            var target = moleImage.rectTransform;
            var areaSize = playArea == null ? new Vector2(800f, 800f) : playArea.rect.size;
            var halfWidth = Mathf.Max(0f, (areaSize.x - target.rect.width) * 0.5f);
            var halfHeight = Mathf.Max(0f, (areaSize.y - target.rect.height) * 0.5f);
            target.anchoredPosition = new Vector2(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight));
        }

        private void RefreshView()
        {
            // 同步分数、结束状态和红点显隐，避免模型状态与画面脱节。
            if (scoreText != null)
            {
                scoreText.text = $"分数：{(model == null ? 0 : model.Score)}";
            }

            var isGameOver = model != null && model.State == GameState.GameOver;
            if (statusText != null)
            {
                statusText.text = isGameOver ? "游戏结束" : string.Empty;
            }

            if (moleImage != null)
            {
                moleImage.gameObject.SetActive(model != null && model.HasActiveMole && !isGameOver);
            }
        }

        private void EnsureReferences()
        {
            // 当场景只包含控制器时，运行时创建打地鼠界面。
            if (scoreText != null && statusText != null && playArea != null && moleImage != null && moleButton != null && restartButton != null && backButton != null)
            {
                return;
            }

            var canvas = RuntimeUiFactory.EnsureCanvas();
            RuntimeUiFactory.CreatePanel(canvas.transform, "ScorePanel", new Vector2(0f, 770f), new Vector2(860f, 120f), new Color(0.86f, 0.91f, 0.96f));
            scoreText = RuntimeUiFactory.CreateText(canvas.transform, "ScoreText", "分数：5", new Vector2(0f, 760f), new Vector2(760f, 80f), 42);
            statusText = RuntimeUiFactory.CreateText(canvas.transform, "StatusText", string.Empty, new Vector2(0f, 650f), new Vector2(760f, 80f), 38);
            statusText.color = new Color(0.82f, 0.12f, 0.12f);
            playArea = RuntimeUiFactory.CreatePanel(canvas.transform, "PlayArea", new Vector2(0f, 130f), new Vector2(860f, 980f), new Color(0.92f, 0.95f, 0.98f));
            moleImage = RuntimeUiFactory.CreateImage(playArea, "Mole", Vector2.zero, new Vector2(130f, 130f), new Color(0.9f, 0.08f, 0.08f));
            moleButton = moleImage.gameObject.AddComponent<Button>();
            moleButton.targetGraphic = moleImage;
            restartButton = RuntimeUiFactory.CreateButton(canvas.transform, "重新开始", new Vector2(-230f, -700f), new Vector2(360f, 100f));
            backButton = RuntimeUiFactory.CreateButton(canvas.transform, "返回", new Vector2(230f, -700f), new Vector2(360f, 100f));
        }
    }
}
