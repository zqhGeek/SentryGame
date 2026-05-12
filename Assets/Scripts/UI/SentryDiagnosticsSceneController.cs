using SentryGame.Common;
using SentryGame.SentryTesting;
using UnityEngine;
using UnityEngine.UI;

namespace SentryGame.UI
{
    public sealed class SentryDiagnosticsSceneController : MonoBehaviour
    {
        [SerializeField] private Button backButton = null;

        private void Awake()
        {
            EnsureReferences();
        }

        public void RunDiagnosticCase(string id)
        {
            SentryTelemetryService.RunDiagnosticCase(id);
        }

        public void ReturnToModeSelect()
        {
            SceneLoader.LoadScene(GameSceneNames.ModeSelectScene);
        }

        private void EnsureReferences()
        {
            // 创建诊断场景的标题、所有诊断按钮和返回按钮，Release 包中保持可见。
            var canvas = RuntimeUiFactory.EnsureCanvas();
            RuntimeUiFactory.CreateText(canvas.transform, "Title", "Sentry 功能测试", new Vector2(0f, 760f), new Vector2(860f, 90f), 48);
            var cases = SentryTelemetryService.GetDiagnosticCases();
            for (var i = 0; i < cases.Length; i += 1)
            {
                var testCase = cases[i];
                var x = i % 2 == 0 ? -250f : 250f;
                var y = 590f - (i / 2) * 135f;
                var button = RuntimeUiFactory.CreateButton(canvas.transform, testCase.Title, new Vector2(x, y), new Vector2(420f, 96f));
                button.onClick.AddListener(() => RunDiagnosticCase(testCase.Id));
            }

            backButton = RuntimeUiFactory.CreateButton(canvas.transform, "返回", new Vector2(0f, -760f), new Vector2(520f, 100f));
            backButton.onClick.AddListener(ReturnToModeSelect);
        }
    }
}
