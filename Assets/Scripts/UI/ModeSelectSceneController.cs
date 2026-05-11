using SentryGame.Common;
using UnityEngine;
using UnityEngine.UI;

namespace SentryGame.UI
{
    public sealed class ModeSelectSceneController : MonoBehaviour
    {
        [SerializeField] private Button whackAMoleButton = null;
        [SerializeField] private Button snakeButton = null;

        private void Awake()
        {
            EnsureReferences();
            if (whackAMoleButton != null)
            {
                whackAMoleButton.onClick.AddListener(OpenWhackAMole);
            }

            if (snakeButton != null)
            {
                snakeButton.onClick.AddListener(OpenSnake);
            }
        }

        public void OpenWhackAMole()
        {
            SceneLoader.LoadScene(GameSceneNames.WhackAMoleScene);
        }

        public void OpenSnake()
        {
            SceneLoader.LoadScene(GameSceneNames.SnakeScene);
        }

        private void EnsureReferences()
        {
            // 当场景只包含控制器时，运行时创建模式选择按钮。
            if (whackAMoleButton != null && snakeButton != null)
            {
                return;
            }

            var canvas = RuntimeUiFactory.EnsureCanvas();
            RuntimeUiFactory.CreateText(canvas.transform, "Title", "选择模式", new Vector2(0f, 460f), new Vector2(760f, 120f), 64);
            whackAMoleButton = RuntimeUiFactory.CreateButton(canvas.transform, "打地鼠", new Vector2(0f, 160f), new Vector2(560f, 120f));
            snakeButton = RuntimeUiFactory.CreateButton(canvas.transform, "贪吃蛇", new Vector2(0f, -20f), new Vector2(560f, 120f));
        }
    }
}
