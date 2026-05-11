using System.Collections.Generic;
using System.Linq;
using SentryGame.Common;
using SentryGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SentryGame.Editor
{
    public static class SentryGameSceneBuilder
    {
        private const string SceneFolder = "Assets/Scenes";
        private static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);

        [MenuItem("SentryGame/Build All Scenes")]
        public static void BuildAllScenes()
        {
            EnsureFolders();
            BuildLoginScene();
            BuildModeSelectScene();
            BuildWhackAMoleScene();
            BuildSnakeScene();
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void BuildLoginScene()
        {
            // 创建登录场景的 Canvas、输入框、错误提示、登录按钮，并绑定到登录控制器。
            var scene = CreateBaseScene();
            var canvas = CreateCanvas();
            var controller = new GameObject(nameof(LoginSceneController)).AddComponent<LoginSceneController>();
            var title = CreateText(canvas.transform, "哨兵游戏", new Vector2(0f, 520f), new Vector2(760f, 120f), 64);
            var userName = CreateInput(canvas.transform, "账号", new Vector2(0f, 300f), false);
            var password = CreateInput(canvas.transform, "密码", new Vector2(0f, 140f), true);
            var error = CreateText(canvas.transform, string.Empty, new Vector2(0f, 20f), new Vector2(760f, 80f), 34);
            var loginButton = CreateButton(canvas.transform, "登录", new Vector2(0f, -140f), new Vector2(520f, 110f));

            title.color = new Color(0.08f, 0.12f, 0.18f);
            error.color = new Color(0.82f, 0.12f, 0.12f);
            Bind(controller, ("userNameInput", userName), ("passwordInput", password), ("errorText", error), ("loginButton", loginButton));
            SaveScene(scene, GameSceneNames.LoginScene);
        }

        private static void BuildModeSelectScene()
        {
            // 创建模式选择场景的标题和两个模式入口按钮，并绑定到模式选择控制器。
            var scene = CreateBaseScene();
            var canvas = CreateCanvas();
            var controller = new GameObject(nameof(ModeSelectSceneController)).AddComponent<ModeSelectSceneController>();
            CreateText(canvas.transform, "选择模式", new Vector2(0f, 460f), new Vector2(760f, 120f), 64);
            var whackButton = CreateButton(canvas.transform, "打地鼠", new Vector2(0f, 160f), new Vector2(560f, 120f));
            var snakeButton = CreateButton(canvas.transform, "贪吃蛇", new Vector2(0f, -20f), new Vector2(560f, 120f));

            Bind(controller, ("whackAMoleButton", whackButton), ("snakeButton", snakeButton));
            SaveScene(scene, GameSceneNames.ModeSelectScene);
        }

        private static void BuildWhackAMoleScene()
        {
            // 创建打地鼠场景的分数、状态、玩法区域、红点按钮和返回按钮。
            var scene = CreateBaseScene();
            var canvas = CreateCanvas();
            var controller = new GameObject(nameof(WhackAMoleSceneController)).AddComponent<WhackAMoleSceneController>();
            var score = CreateText(canvas.transform, "分数：5", new Vector2(0f, 760f), new Vector2(760f, 80f), 42);
            var status = CreateText(canvas.transform, string.Empty, new Vector2(0f, 650f), new Vector2(760f, 80f), 38);
            var playArea = CreatePanel(canvas.transform, "PlayArea", new Vector2(0f, 130f), new Vector2(860f, 980f), new Color(0.92f, 0.95f, 0.98f));
            var mole = CreateCircleButton(playArea.transform, "Mole", new Vector2(0f, 0f), new Vector2(130f, 130f), new Color(0.9f, 0.08f, 0.08f));
            var back = CreateButton(canvas.transform, "返回", new Vector2(0f, -700f), new Vector2(420f, 100f));

            status.color = new Color(0.82f, 0.12f, 0.12f);
            Bind(controller, ("scoreText", score), ("statusText", status), ("playArea", playArea), ("moleImage", mole.GetComponent<Image>()), ("moleButton", mole), ("backButton", back));
            SaveScene(scene, GameSceneNames.WhackAMoleScene);
        }

        private static void BuildSnakeScene()
        {
            // 创建贪吃蛇玩法 UI：地图、隐藏格子模板、分数、方向按钮和返回按钮。
            var scene = CreateBaseScene();
            var canvas = CreateCanvas();
            var controller = new GameObject(nameof(SnakeSceneController)).AddComponent<SnakeSceneController>();
            var score = CreateText(canvas.transform, "分数：0", new Vector2(0f, 820f), new Vector2(760f, 80f), 42);
            var status = CreateText(canvas.transform, string.Empty, new Vector2(0f, 730f), new Vector2(760f, 80f), 38);
            var map = CreatePanel(canvas.transform, "MapRoot", new Vector2(0f, 130f), new Vector2(840f, 1120f), new Color(0.82f, 0.87f, 0.9f));
            var cell = CreateImage(map.transform, "CellPrefab", Vector2.zero, new Vector2(40f, 40f), new Color(0.9f, 0.94f, 0.98f));
            var up = CreateButton(canvas.transform, "上", new Vector2(290f, -570f), new Vector2(120f, 90f));
            var down = CreateButton(canvas.transform, "下", new Vector2(290f, -770f), new Vector2(120f, 90f));
            var left = CreateButton(canvas.transform, "左", new Vector2(150f, -670f), new Vector2(120f, 90f));
            var right = CreateButton(canvas.transform, "右", new Vector2(430f, -670f), new Vector2(120f, 90f));
            var back = CreateButton(canvas.transform, "返回", new Vector2(-260f, -690f), new Vector2(300f, 100f));

            cell.gameObject.SetActive(false);
            status.color = new Color(0.82f, 0.12f, 0.12f);
            Bind(controller, ("mapRoot", map), ("cellPrefab", cell), ("scoreText", score), ("statusText", status), ("upButton", up), ("downButton", down), ("leftButton", left), ("rightButton", right), ("backButton", back));
            SaveScene(scene, GameSceneNames.SnakeScene);
        }

        private static Scene CreateBaseScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.AddComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            camera.AddComponent<AudioListener>();
            CreateEventSystem();
            return scene;
        }

        private static Canvas CreateCanvas()
        {
            var canvasObject = new GameObject("Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void CreateEventSystem()
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private static Text CreateText(Transform parent, string text, Vector2 position, Vector2 size, int fontSize)
        {
            var image = CreateImage(parent, "Text", position, size, Color.clear);
            var label = image.gameObject.AddComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.08f, 0.12f, 0.18f);
            return label;
        }

        private static InputField CreateInput(Transform parent, string placeholder, Vector2 position, bool password)
        {
            // 组装 UnityEngine.UI.InputField 所需的背景、输入文本和占位文本。
            var background = CreateImage(parent, placeholder, position, new Vector2(720f, 110f), Color.white);
            var input = background.gameObject.AddComponent<InputField>();
            var text = CreateText(background.transform, string.Empty, Vector2.zero, new Vector2(660f, 90f), 38);
            var hint = CreateText(background.transform, placeholder, Vector2.zero, new Vector2(660f, 90f), 38);
            hint.color = new Color(0.55f, 0.6f, 0.66f);
            input.textComponent = text;
            input.placeholder = hint;
            input.contentType = password ? InputField.ContentType.Password : InputField.ContentType.Standard;
            return input;
        }

        private static Button CreateButton(Transform parent, string text, Vector2 position, Vector2 size)
        {
            var image = CreateImage(parent, text, position, size, new Color(0.16f, 0.42f, 0.78f));
            var button = image.gameObject.AddComponent<Button>();
            var label = CreateText(image.transform, text, Vector2.zero, size, 36);
            label.color = Color.white;
            return button;
        }

        private static Button CreateCircleButton(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            var image = CreateImage(parent, name, position, size, color);
            return image.gameObject.AddComponent<Button>();
        }

        private static RectTransform CreatePanel(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            return CreateImage(parent, name, position, size, color).rectTransform;
        }

        private static Image CreateImage(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
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

        private static void Bind(Object target, params (string fieldName, Object value)[] bindings)
        {
            var serializedObject = new SerializedObject(target);
            foreach (var binding in bindings)
            {
                serializedObject.FindProperty(binding.fieldName).objectReferenceValue = binding.value;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SaveScene(Scene scene, string sceneName)
        {
            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/{sceneName}.unity");
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(SceneFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
        }

        private static void UpdateBuildSettings()
        {
            // 用四个正式游戏场景重建构建列表，确保 Android 启动入口是登录场景。
            var requiredScenes = new[]
            {
                $"{SceneFolder}/{GameSceneNames.LoginScene}.unity",
                $"{SceneFolder}/{GameSceneNames.ModeSelectScene}.unity",
                $"{SceneFolder}/{GameSceneNames.WhackAMoleScene}.unity",
                $"{SceneFolder}/{GameSceneNames.SnakeScene}.unity"
            };
            EditorBuildSettings.scenes = requiredScenes.Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
        }
    }
}
