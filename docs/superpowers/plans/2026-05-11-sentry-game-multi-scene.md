# SentryGame 多场景游戏实现计划

> **给自动化执行者的要求：** 执行本计划时必须使用 `superpowers:subagent-driven-development` 或 `superpowers:executing-plans`。步骤使用复选框语法跟踪进度。

**目标：** 实现一个 Android 竖屏 Unity 多场景游戏，包含登录、模式选择、打地鼠和贪吃蛇。

**架构：** 项目拆成四个 Unity Scene，每个 Scene 只负责自己的界面和输入。核心规则放入可测试的纯 C# 模型，UI 控制器调用模型并更新 UGUI，避免把玩法规则散落在场景对象上。

**技术栈：** Unity 6000.3.9f1、UGUI、Unity Test Framework、C#、EditMode 测试、PlayMode 场景验证。

---

## 文件结构

- 创建 `Assets/Scripts/SentryGame.Runtime.asmdef`：运行时代码程序集。
- 创建 `Assets/Scripts/Common/GameSceneNames.cs`：统一维护场景名。
- 创建 `Assets/Scripts/Common/LoginService.cs`：固定账号密码校验。
- 创建 `Assets/Scripts/Common/GameState.cs`：通用游戏状态枚举。
- 创建 `Assets/Scripts/WhackAMole/WhackAMoleGameModel.cs`：打地鼠计分、红点状态、倒计时规则。
- 创建 `Assets/Scripts/Snake/GridPoint.cs`：贪吃蛇整数网格坐标。
- 创建 `Assets/Scripts/Snake/SnakeDirection.cs`：贪吃蛇方向枚举和方向工具。
- 创建 `Assets/Scripts/Snake/SnakeGameModel.cs`：贪吃蛇移动、食物、增长、碰撞规则。
- 创建 `Assets/Scripts/UI/SceneLoader.cs`：封装场景切换。
- 创建 `Assets/Scripts/UI/LoginSceneController.cs`：登录界面控制。
- 创建 `Assets/Scripts/UI/ModeSelectSceneController.cs`：模式选择界面控制。
- 创建 `Assets/Scripts/UI/WhackAMoleSceneController.cs`：打地鼠界面控制。
- 创建 `Assets/Scripts/UI/SnakeSceneController.cs`：贪吃蛇界面控制。
- 创建 `Assets/Editor/SentryGameSceneBuilder.cs`：生成四个场景和 UGUI 结构。
- 创建 `Assets/Tests/EditMode/SentryGame.EditModeTests.asmdef`：EditMode 测试程序集。
- 创建 `Assets/Tests/EditMode/LoginServiceTests.cs`：登录测试。
- 创建 `Assets/Tests/EditMode/WhackAMoleGameModelTests.cs`：打地鼠规则测试。
- 创建 `Assets/Tests/EditMode/SnakeGameModelTests.cs`：贪吃蛇规则测试。
- 创建 `Assets/Tests/EditMode/SceneConfigurationTests.cs`：场景构建配置测试。
- 修改 `ProjectSettings/EditorBuildSettings.asset`：包含四个场景。
- 修改 `ProjectSettings/ProjectSettings.asset`：Android 竖屏设置。
- 创建 `README.md`：中文项目说明、账号密码、玩法、验证方式。

## 验证命令

使用 Unity 执行 EditMode 测试：

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.9f1\Editor\Unity.exe" -batchmode -quit -projectPath "F:\UnityProject\SentryGame" -runTests -testPlatform editmode -testResults "F:\Caches\SentryGame\TestResults\editmode-results.xml" -logFile "F:\Caches\SentryGame\TestResults\editmode.log"
```

如果 Unity 安装路径不同，先用以下命令定位：

```powershell
Get-ChildItem -Path "C:\Program Files\Unity\Hub\Editor" -Directory
```

## Task 1：程序集与登录校验

**Files:**
- Create: `Assets/Scripts/SentryGame.Runtime.asmdef`
- Create: `Assets/Scripts/Common/GameSceneNames.cs`
- Create: `Assets/Scripts/Common/LoginService.cs`
- Test: `Assets/Tests/EditMode/SentryGame.EditModeTests.asmdef`
- Test: `Assets/Tests/EditMode/LoginServiceTests.cs`

- [ ] **Step 1：写失败测试**

```csharp
using NUnit.Framework;
using SentryGame.Common;

namespace SentryGame.Tests
{
    public sealed class LoginServiceTests
    {
        [Test]
        public void Validate_ReturnsTrue_WhenCredentialMatches()
        {
            Assert.IsTrue(LoginService.Validate("admin", "123456"));
        }

        [Test]
        public void Validate_ReturnsFalse_WhenCredentialDoesNotMatch()
        {
            Assert.IsFalse(LoginService.Validate("admin", "wrong"));
            Assert.IsFalse(LoginService.Validate("guest", "123456"));
        }
    }
}
```

- [ ] **Step 2：运行测试确认失败**

Run:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.9f1\Editor\Unity.exe" -batchmode -quit -projectPath "F:\UnityProject\SentryGame" -runTests -testPlatform editmode -testResults "F:\Caches\SentryGame\TestResults\task1-red.xml" -logFile "F:\Caches\SentryGame\TestResults\task1-red.log"
```

Expected: FAIL，原因是 `SentryGame.Common.LoginService` 不存在。

- [ ] **Step 3：写最小实现**

```csharp
namespace SentryGame.Common
{
    public static class GameSceneNames
    {
        public const string Login = "LoginScene";
        public const string ModeSelect = "ModeSelectScene";
        public const string WhackAMole = "WhackAMoleScene";
        public const string Snake = "SnakeScene";
    }
}
```

```csharp
namespace SentryGame.Common
{
    public static class LoginService
    {
        public const string FixedUserName = "admin";
        public const string FixedPassword = "123456";

        public static bool Validate(string userName, string password)
        {
            return userName == FixedUserName && password == FixedPassword;
        }
    }
}
```

- [ ] **Step 4：运行测试确认通过**

Run 同 Step 2，结果应为 PASS。

- [ ] **Step 5：提交**

```powershell
git add -- Assets/Scripts Assets/Tests/EditMode
git commit -m "feat: 添加登录校验"
```

## Task 2：打地鼠核心规则

**Files:**
- Create: `Assets/Scripts/Common/GameState.cs`
- Create: `Assets/Scripts/WhackAMole/WhackAMoleGameModel.cs`
- Test: `Assets/Tests/EditMode/WhackAMoleGameModelTests.cs`

- [ ] **Step 1：写失败测试**

```csharp
using NUnit.Framework;
using SentryGame.Common;
using SentryGame.WhackAMole;

namespace SentryGame.Tests
{
    public sealed class WhackAMoleGameModelTests
    {
        [Test]
        public void HitActiveMole_AddsScoreAndClearsMole()
        {
            var model = new WhackAMoleGameModel(5);
            model.SpawnMole(1.5f);

            model.HitActiveMole();

            Assert.AreEqual(6, model.Score);
            Assert.IsFalse(model.HasActiveMole);
            Assert.AreEqual(GameState.Running, model.State);
        }

        [Test]
        public void Tick_RemovesExpiredMoleAndSubtractsScore()
        {
            var model = new WhackAMoleGameModel(5);
            model.SpawnMole(1f);

            model.Tick(1.1f);

            Assert.AreEqual(4, model.Score);
            Assert.IsFalse(model.HasActiveMole);
        }

        [Test]
        public void Tick_EndsGame_WhenScoreDropsToZero()
        {
            var model = new WhackAMoleGameModel(1);
            model.SpawnMole(1f);

            model.Tick(1.1f);

            Assert.AreEqual(0, model.Score);
            Assert.AreEqual(GameState.GameOver, model.State);
        }
    }
}
```

- [ ] **Step 2：运行测试确认失败**

Expected: FAIL，原因是 `WhackAMoleGameModel` 不存在。

- [ ] **Step 3：写最小实现**

```csharp
namespace SentryGame.Common
{
    public enum GameState
    {
        Ready,
        Running,
        GameOver
    }
}
```

```csharp
using SentryGame.Common;

namespace SentryGame.WhackAMole
{
    public sealed class WhackAMoleGameModel
    {
        public int Score { get; private set; }
        public GameState State { get; private set; }
        public bool HasActiveMole { get; private set; }

        private float remainingTime;

        public WhackAMoleGameModel(int initialScore)
        {
            Score = initialScore;
            State = GameState.Running;
        }

        // 生成一个新红点，并设置本轮可点击倒计时。
        public void SpawnMole(float lifeTime)
        {
            if (State != GameState.Running)
            {
                return;
            }

            HasActiveMole = true;
            remainingTime = lifeTime;
        }

        // 命中当前红点后加分，并清除当前红点。
        public void HitActiveMole()
        {
            if (State != GameState.Running || !HasActiveMole)
            {
                return;
            }

            Score += 1;
            HasActiveMole = false;
            remainingTime = 0f;
        }

        // 推进红点倒计时，红点超时未点击时扣分。
        public void Tick(float deltaTime)
        {
            if (State != GameState.Running || !HasActiveMole)
            {
                return;
            }

            remainingTime -= deltaTime;
            if (remainingTime > 0f)
            {
                return;
            }

            Score -= 1;
            HasActiveMole = false;
            remainingTime = 0f;
            if (Score <= 0)
            {
                Score = 0;
                State = GameState.GameOver;
            }
        }
    }
}
```

- [ ] **Step 4：运行测试确认通过**

Run EditMode 测试，结果应为 PASS。

- [ ] **Step 5：提交**

```powershell
git add -- Assets/Scripts/Common Assets/Scripts/WhackAMole Assets/Tests/EditMode/WhackAMoleGameModelTests.cs
git commit -m "feat: 添加打地鼠规则"
```

## Task 3：贪吃蛇核心规则

**Files:**
- Create: `Assets/Scripts/Snake/GridPoint.cs`
- Create: `Assets/Scripts/Snake/SnakeDirection.cs`
- Create: `Assets/Scripts/Snake/SnakeGameModel.cs`
- Test: `Assets/Tests/EditMode/SnakeGameModelTests.cs`

- [ ] **Step 1：写失败测试**

```csharp
using NUnit.Framework;
using SentryGame.Common;
using SentryGame.Snake;

namespace SentryGame.Tests
{
    public sealed class SnakeGameModelTests
    {
        [Test]
        public void ChangeDirection_IgnoresDirectReverse()
        {
            var model = new SnakeGameModel(10, 10, new[] { new GridPoint(3, 3), new GridPoint(2, 3) }, new GridPoint(5, 5), SnakeDirection.Right);

            model.ChangeDirection(SnakeDirection.Left);

            Assert.AreEqual(SnakeDirection.Right, model.Direction);
        }

        [Test]
        public void Step_MovesSnakeForward()
        {
            var model = new SnakeGameModel(10, 10, new[] { new GridPoint(3, 3) }, new GridPoint(5, 5), SnakeDirection.Right);

            model.Step();

            Assert.AreEqual(new GridPoint(4, 3), model.Head);
        }

        [Test]
        public void Step_EatsFoodAddsScoreAndGrows()
        {
            var model = new SnakeGameModel(10, 10, new[] { new GridPoint(3, 3) }, new GridPoint(4, 3), SnakeDirection.Right);

            model.Step();

            Assert.AreEqual(1, model.Score);
            Assert.AreEqual(2, model.Body.Count);
            Assert.IsTrue(model.NeedsFood);
        }

        [Test]
        public void Step_EndsGame_WhenHitsWall()
        {
            var model = new SnakeGameModel(5, 5, new[] { new GridPoint(4, 2) }, new GridPoint(1, 1), SnakeDirection.Right);

            model.Step();

            Assert.AreEqual(GameState.GameOver, model.State);
        }

        [Test]
        public void Step_EndsGame_WhenHitsBody()
        {
            var body = new[] { new GridPoint(2, 2), new GridPoint(2, 3), new GridPoint(1, 3), new GridPoint(1, 2) };
            var model = new SnakeGameModel(6, 6, body, new GridPoint(5, 5), SnakeDirection.Down);

            model.Step();

            Assert.AreEqual(GameState.GameOver, model.State);
        }
    }
}
```

- [ ] **Step 2：运行测试确认失败**

Expected: FAIL，原因是贪吃蛇模型类型不存在。

- [ ] **Step 3：写最小实现**

核心实现必须包含：`GridPoint` 值对象、方向反向判断、`Step()` 中的撞墙、撞身、吃食物、增长和普通移动。

- [ ] **Step 4：运行测试确认通过**

Run EditMode 测试，结果应为 PASS。

- [ ] **Step 5：提交**

```powershell
git add -- Assets/Scripts/Snake Assets/Tests/EditMode/SnakeGameModelTests.cs
git commit -m "feat: 添加贪吃蛇规则"
```

## Task 4：创建四个场景和 UI 控制器

**Files:**
- Create: `Assets/Scripts/UI/SceneLoader.cs`
- Create: `Assets/Scripts/UI/LoginSceneController.cs`
- Create: `Assets/Scripts/UI/ModeSelectSceneController.cs`
- Create: `Assets/Scripts/UI/WhackAMoleSceneController.cs`
- Create: `Assets/Scripts/UI/SnakeSceneController.cs`
- Create: `Assets/Editor/SentryGameSceneBuilder.cs`
- Create: `Assets/Scenes/LoginScene.unity`
- Create: `Assets/Scenes/ModeSelectScene.unity`
- Create: `Assets/Scenes/WhackAMoleScene.unity`
- Create: `Assets/Scenes/SnakeScene.unity`
- Modify: `ProjectSettings/EditorBuildSettings.asset`
- Test: `Assets/Tests/EditMode/SceneConfigurationTests.cs`

- [ ] **Step 1：写失败测试**

```csharp
using NUnit.Framework;
using SentryGame.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SentryGame.Tests
{
    public sealed class SceneConfigurationTests
    {
        [Test]
        public void BuildSettings_ContainsAllGameScenes()
        {
            var paths = EditorBuildSettings.scenes;

            Assert.IsTrue(HasScene(paths, GameSceneNames.Login));
            Assert.IsTrue(HasScene(paths, GameSceneNames.ModeSelect));
            Assert.IsTrue(HasScene(paths, GameSceneNames.WhackAMole));
            Assert.IsTrue(HasScene(paths, GameSceneNames.Snake));
        }

        [TestCase("Assets/Scenes/LoginScene.unity", "LoginSceneController")]
        [TestCase("Assets/Scenes/ModeSelectScene.unity", "ModeSelectSceneController")]
        [TestCase("Assets/Scenes/WhackAMoleScene.unity", "WhackAMoleSceneController")]
        [TestCase("Assets/Scenes/SnakeScene.unity", "SnakeSceneController")]
        public void Scene_ContainsCanvasAndController(string scenePath, string controllerName)
        {
            EditorSceneManager.OpenScene(scenePath);

            Assert.IsNotNull(GameObject.Find("Canvas"));
            Assert.IsNotNull(GameObject.Find(controllerName));
        }

        private static bool HasScene(EditorBuildSettingsScene[] scenes, string sceneName)
        {
            foreach (var scene in scenes)
            {
                if (scene.enabled && scene.path.EndsWith(sceneName + ".unity"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
```

- [ ] **Step 2：运行测试确认失败**

Expected: FAIL，原因是四个场景和控制器不存在。

- [ ] **Step 3：写场景生成器和控制器**

`SentryGameSceneBuilder` 提供菜单项 `SentryGame/Build Scenes`，自动创建四个 Scene、Canvas、EventSystem、主控制器对象、按钮和文本，并更新 Build Settings。所有运行时注释使用中文。

- [ ] **Step 4：执行场景生成**

Run:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.9f1\Editor\Unity.exe" -batchmode -quit -projectPath "F:\UnityProject\SentryGame" -executeMethod SentryGame.EditorTools.SentryGameSceneBuilder.BuildAllScenes -logFile "F:\Caches\SentryGame\SceneBuild\build-scenes.log"
```

Expected: 创建四个场景并写入 Build Settings。

- [ ] **Step 5：运行测试确认通过**

Run EditMode 测试，结果应为 PASS。

- [ ] **Step 6：提交**

```powershell
git add -- Assets/Scripts/UI Assets/Editor Assets/Scenes ProjectSettings/EditorBuildSettings.asset Assets/Tests/EditMode/SceneConfigurationTests.cs
git commit -m "feat: 添加多场景界面"
```

## Task 5：Android 竖屏设置和 README

**Files:**
- Modify: `ProjectSettings/ProjectSettings.asset`
- Create: `README.md`

- [ ] **Step 1：确认竖屏设置**

在 `ProjectSettings/ProjectSettings.asset` 中将 Android 默认方向设置为 Portrait，并禁用横屏方向。

- [ ] **Step 2：写中文 README**

README 必须包含：项目简介、固定账号密码、四个场景、两种玩法、Editor 预览、Android 构建要求、测试命令。

- [ ] **Step 3：运行完整验证**

Run:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.9f1\Editor\Unity.exe" -batchmode -quit -projectPath "F:\UnityProject\SentryGame" -runTests -testPlatform editmode -testResults "F:\Caches\SentryGame\TestResults\final-editmode-results.xml" -logFile "F:\Caches\SentryGame\TestResults\final-editmode.log"
```

Expected: 所有 EditMode 测试通过。

- [ ] **Step 4：提交**

```powershell
git add -- ProjectSettings/ProjectSettings.asset README.md
git commit -m "docs: 添加游戏说明和竖屏配置"
```

## 自审结果

- 设计文档中的登录、模式选择、打地鼠、贪吃蛇、多场景、竖屏、测试、README 要求均有对应任务。
- 所有临时输出路径使用 `F:\Caches\SentryGame`。
- 所有 Markdown 文档为中文且不包含表情符号。
- 计划未使用占位标记。
