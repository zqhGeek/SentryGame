using SentryGame.Common;
using SentryGame.SentryTesting;
using UnityEngine;
using UnityEngine.UI;

namespace SentryGame.UI
{
    public sealed class LoginSceneController : MonoBehaviour
    {
        [SerializeField] private InputField userNameInput = null;
        [SerializeField] private InputField passwordInput = null;
        [SerializeField] private Text errorText = null;
        [SerializeField] private Button loginButton = null;
        private int failureCount;

        private void Awake()
        {
            EnsureReferences();
            if (loginButton != null)
            {
                loginButton.onClick.AddListener(OnLoginClicked);
            }

            ShowError(string.Empty);
        }

        public void OnLoginClicked()
        {
            var userName = userNameInput == null ? string.Empty : userNameInput.text;
            var password = passwordInput == null ? string.Empty : passwordInput.text;
            if (LoginService.Validate(userName, password))
            {
                SentryTelemetryService.RecordLoginSuccess(userName);
                SceneLoader.LoadScene(GameSceneNames.ModeSelectScene);
                return;
            }

            failureCount += 1;
            SentryTelemetryService.RecordLoginFailure(userName, failureCount);
            ShowError("账号或密码错误");
        }

        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
            }
        }

        private void EnsureReferences()
        {
            // 当场景只包含控制器时，运行时补齐登录界面，保证 Editor 可直接预览。
            if (userNameInput != null && passwordInput != null && errorText != null && loginButton != null)
            {
                return;
            }

            var canvas = RuntimeUiFactory.EnsureCanvas();
            RuntimeUiFactory.CreateText(canvas.transform, "Title", "哨兵游戏", new Vector2(0f, 520f), new Vector2(760f, 120f), 64);
            userNameInput = RuntimeUiFactory.CreateInput(canvas.transform, "账号", new Vector2(0f, 300f), false);
            passwordInput = RuntimeUiFactory.CreateInput(canvas.transform, "密码", new Vector2(0f, 140f), true);
            errorText = RuntimeUiFactory.CreateText(canvas.transform, "ErrorText", string.Empty, new Vector2(0f, 20f), new Vector2(760f, 80f), 34);
            errorText.color = new Color(0.82f, 0.12f, 0.12f);
            loginButton = RuntimeUiFactory.CreateButton(canvas.transform, "登录", new Vector2(0f, -140f), new Vector2(520f, 110f));
        }
    }
}
