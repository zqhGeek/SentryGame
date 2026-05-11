using SentryGame.Common;
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
        [SerializeField] private Button backButton = null;

        private WhackAMoleGameModel model;

        private void Awake()
        {
            EnsureReferences();
            if (moleButton != null)
            {
                moleButton.onClick.AddListener(HitMole);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(ReturnToModeSelect);
            }
        }

        private void Start()
        {
            model = new WhackAMoleGameModel(InitialScore);
            SpawnMole();
            RefreshView();
        }

        private void Update()
        {
            if (model == null || model.State == GameState.GameOver)
            {
                return;
            }

            model.Tick(Time.deltaTime);
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

            model.HitActiveMole();
            if (model.State == GameState.Running)
            {
                SpawnMole();
            }

            RefreshView();
        }

        public void ReturnToModeSelect()
        {
            SceneLoader.LoadScene(GameSceneNames.ModeSelectScene);
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
            if (scoreText != null && statusText != null && playArea != null && moleImage != null && moleButton != null && backButton != null)
            {
                return;
            }

            var canvas = RuntimeUiFactory.EnsureCanvas();
            scoreText = RuntimeUiFactory.CreateText(canvas.transform, "ScoreText", "分数：5", new Vector2(0f, 760f), new Vector2(760f, 80f), 42);
            statusText = RuntimeUiFactory.CreateText(canvas.transform, "StatusText", string.Empty, new Vector2(0f, 650f), new Vector2(760f, 80f), 38);
            statusText.color = new Color(0.82f, 0.12f, 0.12f);
            playArea = RuntimeUiFactory.CreatePanel(canvas.transform, "PlayArea", new Vector2(0f, 130f), new Vector2(860f, 980f), new Color(0.92f, 0.95f, 0.98f));
            moleImage = RuntimeUiFactory.CreateImage(playArea, "Mole", Vector2.zero, new Vector2(130f, 130f), new Color(0.9f, 0.08f, 0.08f));
            moleButton = moleImage.gameObject.AddComponent<Button>();
            moleButton.targetGraphic = moleImage;
            backButton = RuntimeUiFactory.CreateButton(canvas.transform, "返回", new Vector2(0f, -700f), new Vector2(420f, 100f));
        }
    }
}
