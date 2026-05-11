using System.Collections.Generic;
using SentryGame.Common;
using SentryGame.Snake;
using UnityEngine;
using UnityEngine.UI;

namespace SentryGame.UI
{
    public sealed class SnakeSceneController : MonoBehaviour
    {
        [SerializeField] private RectTransform mapRoot = null;
        [SerializeField] private Image cellPrefab = null;
        [SerializeField] private Text scoreText = null;
        [SerializeField] private Text statusText = null;
        [SerializeField] private Button upButton = null;
        [SerializeField] private Button downButton = null;
        [SerializeField] private Button leftButton = null;
        [SerializeField] private Button rightButton = null;
        [SerializeField] private Button backButton = null;
        [SerializeField] private int width = 12;
        [SerializeField] private int height = 16;
        [SerializeField] private float stepInterval = 0.35f;

        private readonly Dictionary<GridPoint, Image> cells = new Dictionary<GridPoint, Image>();
        private readonly System.Random random = new System.Random();
        private SnakeGameModel model;
        private float stepTimer;
        private bool noFoodGameOver;

        private void Awake()
        {
            EnsureReferences();
            BindButtons();
        }

        private void Start()
        {
            CreateMap();
            model = new SnakeGameModel(width, height, new[] { new GridPoint(width / 2, height / 2) }, new GridPoint(width / 2 + 2, height / 2), SnakeDirection.Right);
            Render();
        }

        private void Update()
        {
            if (model == null || model.State == GameState.GameOver || noFoodGameOver)
            {
                return;
            }

            stepTimer += Time.deltaTime;
            if (stepTimer < stepInterval)
            {
                return;
            }

            stepTimer = 0f;
            model.Step();
            if (model.NeedsFood)
            {
                TryPlaceNextFood();
            }

            Render();
        }

        public void ReturnToModeSelect()
        {
            SceneLoader.LoadScene(GameSceneNames.ModeSelectScene);
        }

        private void BindButtons()
        {
            // 绑定四个方向按钮和返回按钮；反向输入是否合法由模型规则判断。
            if (upButton != null)
            {
                upButton.onClick.AddListener(() => ChangeDirection(SnakeDirection.Up));
            }

            if (downButton != null)
            {
                downButton.onClick.AddListener(() => ChangeDirection(SnakeDirection.Down));
            }

            if (leftButton != null)
            {
                leftButton.onClick.AddListener(() => ChangeDirection(SnakeDirection.Left));
            }

            if (rightButton != null)
            {
                rightButton.onClick.AddListener(() => ChangeDirection(SnakeDirection.Right));
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(ReturnToModeSelect);
            }
        }

        private void ChangeDirection(SnakeDirection direction)
        {
            if (model != null)
            {
                model.ChangeDirection(direction);
            }
        }

        private void CreateMap()
        {
            // 按网格尺寸创建所有格子，后续渲染只改颜色以减少运行时对象抖动。
            if (mapRoot == null || cellPrefab == null)
            {
                return;
            }

            cells.Clear();
            cellPrefab.gameObject.SetActive(false);
            var mapSize = mapRoot.rect.size;
            var cellSize = Mathf.Min(mapSize.x / width, mapSize.y / height);
            for (var y = 0; y < height; y += 1)
            {
                for (var x = 0; x < width; x += 1)
                {
                    var point = new GridPoint(x, y);
                    var cell = Instantiate(cellPrefab, mapRoot);
                    cell.gameObject.SetActive(true);
                    cell.rectTransform.sizeDelta = new Vector2(cellSize - 2f, cellSize - 2f);
                    cell.rectTransform.anchoredPosition = new Vector2((x + 0.5f) * cellSize - mapSize.x * 0.5f, mapSize.y * 0.5f - (y + 0.5f) * cellSize);
                    cells[point] = cell;
                }
            }
        }

        private bool TryPlaceNextFood()
        {
            // 从所有空格中随机选择食物位置；如果蛇已经占满地图，则以游戏结束处理。
            var occupied = new HashSet<GridPoint>(model.Body);
            var emptyCells = new List<GridPoint>();
            for (var y = 0; y < height; y += 1)
            {
                for (var x = 0; x < width; x += 1)
                {
                    var point = new GridPoint(x, y);
                    if (!occupied.Contains(point))
                    {
                        emptyCells.Add(point);
                    }
                }
            }

            if (emptyCells.Count <= 0)
            {
                statusText.text = "游戏结束";
                noFoodGameOver = true;
                return false;
            }

            model.SetFood(emptyCells[random.Next(emptyCells.Count)]);
            return true;
        }

        private void Render()
        {
            // 将模型的蛇身、食物、分数和结束状态同步到 UI。
            foreach (var pair in cells)
            {
                pair.Value.color = new Color(0.9f, 0.94f, 0.98f);
            }

            if (model != null && cells.TryGetValue(model.Food, out var foodCell))
            {
                foodCell.color = new Color(0.9f, 0.18f, 0.18f);
            }

            if (model != null)
            {
                foreach (var point in model.Body)
                {
                    if (cells.TryGetValue(point, out var bodyCell))
                    {
                        bodyCell.color = new Color(0.1f, 0.58f, 0.2f);
                    }
                }
            }

            if (scoreText != null)
            {
                scoreText.text = $"分数：{(model == null ? 0 : model.Score)}";
            }

            if (statusText != null)
            {
                statusText.text = model != null && (model.State == GameState.GameOver || noFoodGameOver) ? "游戏结束" : string.Empty;
            }
        }

        private void EnsureReferences()
        {
            // 当场景只包含控制器时，运行时创建贪吃蛇地图、方向按钮和状态文本。
            if (mapRoot != null && cellPrefab != null && scoreText != null && statusText != null && upButton != null && downButton != null && leftButton != null && rightButton != null && backButton != null)
            {
                return;
            }

            var canvas = RuntimeUiFactory.EnsureCanvas();
            scoreText = RuntimeUiFactory.CreateText(canvas.transform, "ScoreText", "分数：0", new Vector2(0f, 820f), new Vector2(760f, 80f), 42);
            statusText = RuntimeUiFactory.CreateText(canvas.transform, "StatusText", string.Empty, new Vector2(0f, 730f), new Vector2(760f, 80f), 38);
            statusText.color = new Color(0.82f, 0.12f, 0.12f);
            mapRoot = RuntimeUiFactory.CreatePanel(canvas.transform, "MapRoot", new Vector2(0f, 130f), new Vector2(840f, 1120f), new Color(0.82f, 0.87f, 0.9f));
            cellPrefab = RuntimeUiFactory.CreateImage(mapRoot, "CellPrefab", Vector2.zero, new Vector2(40f, 40f), new Color(0.9f, 0.94f, 0.98f));
            cellPrefab.gameObject.SetActive(false);
            upButton = RuntimeUiFactory.CreateButton(canvas.transform, "上", new Vector2(290f, -570f), new Vector2(120f, 90f));
            downButton = RuntimeUiFactory.CreateButton(canvas.transform, "下", new Vector2(290f, -770f), new Vector2(120f, 90f));
            leftButton = RuntimeUiFactory.CreateButton(canvas.transform, "左", new Vector2(150f, -670f), new Vector2(120f, 90f));
            rightButton = RuntimeUiFactory.CreateButton(canvas.transform, "右", new Vector2(430f, -670f), new Vector2(120f, 90f));
            backButton = RuntimeUiFactory.CreateButton(canvas.transform, "返回", new Vector2(-260f, -690f), new Vector2(300f, 100f));
        }
    }
}
