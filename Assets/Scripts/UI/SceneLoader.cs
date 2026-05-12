using SentryGame.Common;
using SentryGame.SentryTesting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SentryGame.UI
{
    public sealed class SceneLoader : MonoBehaviour
    {
        public static void LoadScene(string sceneName)
        {
            SentryTelemetryService.RecordSceneLoad(sceneName);
            SceneManager.LoadScene(sceneName);
        }

        public void LoadLoginScene()
        {
            LoadScene(GameSceneNames.LoginScene);
        }

        public void LoadModeSelectScene()
        {
            LoadScene(GameSceneNames.ModeSelectScene);
        }

        public void LoadWhackAMoleScene()
        {
            LoadScene(GameSceneNames.WhackAMoleScene);
        }

        public void LoadSnakeScene()
        {
            LoadScene(GameSceneNames.SnakeScene);
        }

        public void LoadSentryDiagnosticScene()
        {
            LoadScene(GameSceneNames.SentryDiagnosticScene);
        }
    }
}
