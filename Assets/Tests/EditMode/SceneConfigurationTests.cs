using System.IO;
using System.Linq;
using NUnit.Framework;
using SentryGame.Common;
using UnityEditor;

namespace SentryGame.Tests.EditMode
{
    public sealed class SceneConfigurationTests
    {
        private static readonly string[] ExpectedScenePaths =
        {
            $"Assets/Scenes/{GameSceneNames.LoginScene}.unity",
            $"Assets/Scenes/{GameSceneNames.ModeSelectScene}.unity",
            $"Assets/Scenes/{GameSceneNames.WhackAMoleScene}.unity",
            $"Assets/Scenes/{GameSceneNames.SnakeScene}.unity"
        };

        [Test]
        public void BuildSettingsContainsAllGameScenes()
        {
            var configuredPaths = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();

            CollectionAssert.AreEqual(ExpectedScenePaths, configuredPaths);
        }

        [Test]
        public void AndroidUsesLegacyInputManagerOnly()
        {
            var projectSettingsText = File.ReadAllText("ProjectSettings/ProjectSettings.asset");
            var packageManifestText = File.ReadAllText("Packages/manifest.json");

            StringAssert.Contains("activeInputHandler: 0", projectSettingsText);
            StringAssert.DoesNotContain("activeInputHandler: 2", projectSettingsText);
            StringAssert.DoesNotContain("com.unity.inputsystem", packageManifestText);
        }

        [Test]
        public void AndroidUsesAutoRotationWithSensorPortraitManifest()
        {
            var projectSettingsText = File.ReadAllText("ProjectSettings/ProjectSettings.asset");
            var manifestPostProcessorText = File.ReadAllText("Assets/Editor/AndroidManifestOrientationPostProcessor.cs");

            StringAssert.Contains("defaultScreenOrientation: 5", projectSettingsText);
            StringAssert.Contains("allowedAutorotateToPortrait: 1", projectSettingsText);
            StringAssert.Contains("allowedAutorotateToPortraitUpsideDown: 1", projectSettingsText);
            StringAssert.Contains("allowedAutorotateToLandscapeRight: 0", projectSettingsText);
            StringAssert.Contains("allowedAutorotateToLandscapeLeft: 0", projectSettingsText);
            StringAssert.Contains("useOSAutorotation: 1", projectSettingsText);
            StringAssert.Contains("\"sensorPortrait\"", manifestPostProcessorText);
            StringAssert.DoesNotContain("reversePortrait", manifestPostProcessorText);
            StringAssert.Contains("UnityPlayerGameActivity", manifestPostProcessorText);
        }

        [TestCase(GameSceneNames.LoginScene, nameof(SentryGame.UI.LoginSceneController))]
        [TestCase(GameSceneNames.ModeSelectScene, nameof(SentryGame.UI.ModeSelectSceneController))]
        [TestCase(GameSceneNames.WhackAMoleScene, nameof(SentryGame.UI.WhackAMoleSceneController))]
        [TestCase(GameSceneNames.SnakeScene, nameof(SentryGame.UI.SnakeSceneController))]
        public void SceneFileContainsCanvasAndControllerName(string sceneName, string controllerName)
        {
            var sceneText = File.ReadAllText($"Assets/Scenes/{sceneName}.unity");

            StringAssert.Contains("m_Name: Canvas", sceneText);
            StringAssert.Contains($"m_Name: {controllerName}", sceneText);
            StringAssert.Contains("m_UiScaleMode: 1", sceneText);
            StringAssert.Contains("m_ReferenceResolution: {x: 1080, y: 1920}", sceneText);
        }

        [Test]
        public void SceneBuilderExposesBuildAllScenes()
        {
            var method = typeof(SentryGame.Editor.SentryGameSceneBuilder).GetMethod(nameof(SentryGame.Editor.SentryGameSceneBuilder.BuildAllScenes));

            Assert.IsNotNull(method);
            Assert.IsTrue(method.IsStatic);
        }

        [Test]
        public void AndroidBuilderExposesBuildApk()
        {
            var method = typeof(SentryGame.Editor.SentryGameAndroidBuilder).GetMethod(nameof(SentryGame.Editor.SentryGameAndroidBuilder.BuildApk));

            Assert.IsNotNull(method);
            Assert.IsTrue(method.IsStatic);
        }

        [Test]
        public void RuntimeControllersCanCreateMissingUiReferences()
        {
            Assert.IsNotNull(typeof(SentryGame.UI.RuntimeUiFactory));
            Assert.IsNotNull(typeof(SentryGame.UI.LoginSceneController).GetMethod("OnLoginClicked"));
            Assert.IsNotNull(typeof(SentryGame.UI.ModeSelectSceneController).GetMethod("OpenWhackAMole"));
            Assert.IsNotNull(typeof(SentryGame.UI.WhackAMoleSceneController).GetMethod("HitMole"));
            Assert.IsNotNull(typeof(SentryGame.UI.WhackAMoleSceneController).GetMethod("RestartGame"));
            Assert.IsNotNull(typeof(SentryGame.UI.SnakeSceneController).GetMethod("ReturnToModeSelect"));
        }

        [Test]
        public void LoginSceneUsesLegacyInputModuleForInputFieldText()
        {
            var sceneText = File.ReadAllText($"Assets/Scenes/{GameSceneNames.LoginScene}.unity");

            StringAssert.Contains("UnityEngine.EventSystems.StandaloneInputModule", sceneText);
            StringAssert.DoesNotContain("UnityEngine.InputSystem.UI.InputSystemUIInputModule", sceneText);
        }

        [Test]
        public void WhackAMoleSceneShowsScoreAndRestartControls()
        {
            var sceneText = File.ReadAllText($"Assets/Scenes/{GameSceneNames.WhackAMoleScene}.unity");

            StringAssert.Contains("scoreText: {fileID:", sceneText);
            StringAssert.Contains("restartButton: {fileID:", sceneText);
            StringAssert.Contains(@"\u5206\u6570\uFF1A5", sceneText);
            StringAssert.Contains(@"\u91CD\u65B0\u5F00\u59CB", sceneText);
            StringAssert.Contains("m_Name: ScorePanel", sceneText);
        }
    }
}
