using NUnit.Framework;
using SentryGame.UI;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SentryGame.Tests.EditMode
{
    public sealed class WhackAMoleSceneControllerTests
    {
        private const float ReferenceWidth = 1080f;
        private const float ReferenceHeight = 1920f;

        [SetUp]
        public void SetUp()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [Test]
        public void RuntimeUiKeepsScoreAndRestartInsidePortraitCanvas()
        {
            // 创建控制器触发运行时 UI 补齐，再验证关键控件没有越出竖屏参考画布。
            var controllerObject = new GameObject("WhackAMoleControllerUnderTest");
            var controller = controllerObject.AddComponent<WhackAMoleSceneController>();
            InvokeAwake(controller);

            AssertInsidePortraitCanvas("ScorePanel");
            AssertInsidePortraitCanvas("ScoreText");
            AssertInsidePortraitCanvas("重新开始");
            AssertInsidePortraitCanvas("返回");
        }

        [Test]
        public void RandomStallDurationIsOneSecond()
        {
            var field = typeof(WhackAMoleSceneController).GetField("RandomStallMilliseconds", BindingFlags.Static | BindingFlags.NonPublic);

            Assert.IsNotNull(field, "打地鼠随机卡顿时长常量不存在");
            Assert.That(field.GetRawConstantValue(), Is.EqualTo(1000));
        }

        private static void InvokeAwake(WhackAMoleSceneController controller)
        {
            typeof(WhackAMoleSceneController)
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(controller, null);
        }

        private static void AssertInsidePortraitCanvas(string objectName)
        {
            // 按 1080x1920 参考画布计算控件四边，确保按钮和分数显示不会跑出可见区域。
            var target = GameObject.Find(objectName);
            Assert.IsNotNull(target, $"{objectName} 不存在");

            var rectTransform = target.GetComponent<RectTransform>();
            Assert.IsNotNull(rectTransform, $"{objectName} 没有 RectTransform");

            var halfWidth = ReferenceWidth * 0.5f;
            var halfHeight = ReferenceHeight * 0.5f;
            var minX = rectTransform.anchoredPosition.x - rectTransform.sizeDelta.x * 0.5f;
            var maxX = rectTransform.anchoredPosition.x + rectTransform.sizeDelta.x * 0.5f;
            var minY = rectTransform.anchoredPosition.y - rectTransform.sizeDelta.y * 0.5f;
            var maxY = rectTransform.anchoredPosition.y + rectTransform.sizeDelta.y * 0.5f;

            Assert.GreaterOrEqual(minX, -halfWidth, $"{objectName} 左侧越界");
            Assert.LessOrEqual(maxX, halfWidth, $"{objectName} 右侧越界");
            Assert.GreaterOrEqual(minY, -halfHeight, $"{objectName} 底部越界");
            Assert.LessOrEqual(maxY, halfHeight, $"{objectName} 顶部越界");
        }
    }
}
