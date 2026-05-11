# 哨兵游戏

这是一个 Unity 竖屏 Android 游戏示例，包含登录、模式选择、打地鼠和贪吃蛇四个场景。项目使用 UGUI 构建界面，玩法对象使用运行时生成的简单图形，不依赖外部美术资源。

## 登录信息

- 账号：`admin`
- 密码：`123456`

## 场景结构

- `LoginScene`：登录入口，固定账号密码校验成功后进入模式选择。
- `ModeSelectScene`：提供“打地鼠”和“贪吃蛇”两个模式入口。
- `WhackAMoleScene`：打地鼠玩法场景。
- `SnakeScene`：贪吃蛇玩法场景。

构建场景顺序为 `LoginScene`、`ModeSelectScene`、`WhackAMoleScene`、`SnakeScene`，Android 启动后首先进入登录界面。

## 打地鼠玩法

打地鼠初始分数为 5 分。顶部统计条显示当前分数。红点会在玩法区域中随机出现。玩家点击红点后红点消失并加 1 分。红点倒计时结束后会自动消失，如果玩家没有点击则扣 1 分。分数降到 0 时游戏结束，可以点击“重新开始”重开本模式，也可以返回模式选择界面。

## 贪吃蛇玩法

贪吃蛇使用固定网格地图。蛇会按固定节奏自动移动。右下角提供上下左右四个方向按钮，移动端点击按钮即可改变方向。模型层会阻止蛇直接反向移动。食物随机生成在蛇身以外的空格。蛇吃到食物后分数加 1，并增长 1 节。蛇撞墙或撞到自己身体时游戏结束，可以返回模式选择界面。

## Editor 预览

1. 使用 Unity 6000.3.9f1 打开项目。
2. 打开 `Assets/Scenes/LoginScene.unity`。
3. 将 Game 视图设置为 `9:16` 或 `1080x1920`。
4. 点击 Play。
5. 输入账号 `admin` 和密码 `123456`，进入模式选择并测试两个玩法。

如果需要重建场景，可以在 Unity 菜单中执行 `SentryGame/Build All Scenes`。

## Android 运行要求

项目已设置为竖屏运行：

- 默认屏幕方向：Portrait，Android 构建时通过 Manifest 后处理器把主 Activity 强制为 `portrait`，避免 Unity 6000 意外生成横屏或反向竖屏方向。
- 禁止横屏自动旋转。
- Canvas 参考分辨率：`1080x1920`。
- Android 包名：`com.sentrygame.mobile`。
- 输入处理：Input Manager（旧输入系统），场景事件系统使用旧版 `StandaloneInputModule`，保证 UGUI 输入框在 Editor 和 Android 上都能接收账号密码文本。项目不安装新 Input System 包，避免 Android 上启用 Both 或新输入后端产生的输入警告。

Android 打包需要安装 Unity Android Build Support，并配置可用的 Android SDK、NDK 和 JDK。建议在真机上验证按钮触控、屏幕比例和安全区域表现。

## Android 命令行打包

可以使用以下命令生成 APK，输出路径为 `F:\Caches\SentryGame\Builds\Android\SentryGame.apk`：

```powershell
& "F:\Unity\6000.3.9f1\Editor\Unity.exe" -batchmode -quit -projectPath "F:\UnityProject\SentryGame" -buildTarget Android -executeMethod SentryGame.Editor.SentryGameAndroidBuilder.BuildApk -logFile "F:\Caches\SentryGame\Builds\Android\android-build.log"
```

## 自动化测试

EditMode 测试覆盖登录校验、打地鼠规则、贪吃蛇规则和场景配置。可使用以下命令运行测试：

```powershell
Unity.exe -batchmode -quit -projectPath "F:\UnityProject\SentryGame" -runTests -testPlatform editmode -testResults "F:\Caches\SentryGame\TestResults\editmode-results.xml" -logFile "F:\Caches\SentryGame\TestResults\editmode.log"
```

当前环境如果无法从 PATH 找到 `Unity.exe`，需要使用本机 Unity Editor 的完整路径替换命令中的 `Unity.exe`。
