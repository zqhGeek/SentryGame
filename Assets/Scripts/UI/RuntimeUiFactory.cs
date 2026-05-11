using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace SentryGame.UI
{
    public static class RuntimeUiFactory
    {
        private static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);

        public static Canvas EnsureCanvas()
        {
            // 查找或创建竖屏 Canvas，并确保场景中存在可接收 UI 点击的事件系统。
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                EnsureEventSystem();
                return canvas;
            }

            var canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();
            return canvas;
        }

        public static Text CreateText(Transform parent, string name, string text, Vector2 position, Vector2 size, int fontSize)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            var rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
            var label = gameObject.AddComponent<Text>();
            label.font = GetDefaultFont();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.08f, 0.12f, 0.18f);
            return label;
        }

        public static Button CreateButton(Transform parent, string text, Vector2 position, Vector2 size)
        {
            var image = CreateImage(parent, text, position, size, new Color(0.16f, 0.42f, 0.78f));
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var label = CreateText(image.transform, text + "Label", text, Vector2.zero, size, 36);
            label.color = Color.white;
            return button;
        }

        public static InputField CreateInput(Transform parent, string placeholder, Vector2 position, bool password)
        {
            // 组装 UGUI 输入框所需的背景、正文文本、占位文本和密码显示规则。
            var background = CreateImage(parent, placeholder, position, new Vector2(720f, 110f), Color.white);
            var input = background.gameObject.AddComponent<InputField>();
            input.targetGraphic = background;
            var text = CreateText(background.transform, placeholder + "Text", string.Empty, Vector2.zero, new Vector2(660f, 90f), 38);
            var hint = CreateText(background.transform, placeholder + "Placeholder", placeholder, Vector2.zero, new Vector2(660f, 90f), 38);
            hint.color = new Color(0.55f, 0.6f, 0.66f);
            input.textComponent = text;
            input.placeholder = hint;
            input.contentType = password ? InputField.ContentType.Password : InputField.ContentType.Standard;
            return input;
        }

        public static RectTransform CreatePanel(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            return CreateImage(parent, name, position, size, color).rectTransform;
        }

        public static Image CreateImage(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            // 创建居中锚点的 UI 图形对象，供按钮、面板、文本容器和棋盘格复用。
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            var rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
            var image = gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static void EnsureEventSystem()
        {
            // 创建或修补兼容新旧输入系统的事件系统，确保 Editor 和 Android 触控都能点击 UGUI。
            var eventSystem = Object.FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                var eventSystemObject = new GameObject("EventSystem");
                eventSystem = eventSystemObject.AddComponent<EventSystem>();
            }

            if (eventSystem.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }

        private static Font GetDefaultFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf")
                ?? Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "Arial" }, 32);
        }
    }
}
