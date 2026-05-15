# 哨兵游戏

这是一个 Unity 竖屏 Android 游戏示例，包含登录、模式选择、Sentry 诊断、打地鼠和贪吃蛇五个场景。项目使用 UGUI 构建界面，玩法对象使用运行时生成的简单图形，不依赖外部美术资源。

## 登录信息

- 账号：`admin`
- 密码：`123456`

## 场景结构

- `LoginScene`：登录入口，固定账号密码校验成功后进入模式选择。
- `ModeSelectScene`：提供“打地鼠”“贪吃蛇”和“Sentry 功能测试”三个入口。
- `SentryDiagnosticScene`：提供 Sentry 功能诊断入口，用于内部包直接触发各类 Sentry 能力。
- `WhackAMoleScene`：打地鼠玩法场景。
- `SnakeScene`：贪吃蛇玩法场景。

构建场景顺序为 `LoginScene`、`ModeSelectScene`、`SentryDiagnosticScene`、`WhackAMoleScene`、`SnakeScene`，Android 启动后首先进入登录界面。

## 打地鼠玩法

打地鼠初始分数为 5 分。顶部统计条显示当前分数。红点会在玩法区域中随机出现。玩家点击红点后红点消失并加 1 分。红点倒计时结束后会自动消失，如果玩家没有点击则扣 1 分。分数降到 0 时游戏结束，可以点击“重新开始”重开本模式，也可以返回模式选择界面。

## 贪吃蛇玩法

贪吃蛇使用固定网格地图。蛇会按固定节奏自动移动。右下角提供上下左右四个方向按钮，移动端点击按钮即可改变方向。模型层会阻止蛇直接反向移动。食物随机生成在蛇身以外的空格。蛇吃到食物后分数加 1，并增长 1 节。蛇撞墙或撞到自己身体时游戏结束，可以返回模式选择界面。

## Sentry 功能测试

项目包含面向内部 APK 的 Sentry 功能测试能力。测试入口不隐藏，登录成功进入模式选择后，可以点击“哨兵测试”进入 `SentryDiagnosticScene`。

真实玩法流程会自动记录 Sentry 数据：

- 登录失败：记录面包屑、日志和 `login.failure` 指标。
- 登录成功：设置测试用户、登录上下文和当前场景标签。
- 模式选择：记录目标玩法、目标场景和场景跳转面包屑。
- 打地鼠进入、退出、命中、漏点、游戏结束和重开：记录指标、分数、场景、上下文和失败原因。
- 打地鼠命中红点后会低概率触发真实玩法内的随机卡顿和随机崩溃：随机卡顿通过 1 秒 CPU 密集计算模拟主线程性能问题，并用 Sentry transaction 和 span 包裹，随机崩溃会在上报上下文后抛出未捕获异常。
- 贪吃蛇转向、吃食物、撞墙、撞到自身和返回：记录操作链路、分数、蛇身长度和失败原因。

诊断场景提供以下按钮，用于补齐无法稳定通过真实玩法触发的能力：

- 发送消息：验证 `CaptureMessage`。
- 发送异常：验证 `CaptureException`。
- 抛出未捕获异常：验证自动异常捕获。
- 发送日志错误：验证 `Debug.LogError` 捕获。
- 连续重复错误：验证日志防抖和错误限流。
- 触发失败请求：验证失败请求捕获。
- 发送性能追踪：验证手动 transaction 和 span。
- 发送指标：验证 counter、gauge 和 distribution。
- 发送结构化日志：验证 Sentry Logs。
- 触发截图事件：验证截图附件。
- 触发视图层级事件：验证 View Hierarchy 附件。
- 断网缓存事件：断网点击后再联网重启，验证离线缓存补发。
- 主线程阻塞：主要用于 Android 真机验证 ANR。
- 原生崩溃：验证 native crash。
- 过滤事件：发送带 `filtered_event=true` 标签的测试事件，便于配合 Sentry 过滤配置验证。

当前 Sentry 测试配置已打开 tracing、自动启动追踪、自动场景加载追踪、Awake 追踪、截图、视图层级、结构化日志、日志面包屑、用户信息、离线缓存、Metrics、失败请求捕获、原生支持和 IL2CPP 行号支持。

建议人工验证顺序：

1. 在 Editor Play 模式执行一次“登录失败、登录成功、进入打地鼠、命中红点、游戏结束、返回模式选择”，检查 Sentry 后台是否出现用户、标签、上下文、面包屑、日志、指标和性能数据。
2. 在打地鼠中连续命中红点，观察是否触发 1 秒随机卡顿或随机崩溃；卡顿事件会记录 `whack_a_mole.random_stall`、`whack_a_mole.cpu_stall` 和 `whack_a_mole.stall_duration`，并能在 Sentry Performance 中查看对应 transaction 和 span，崩溃事件会记录 `whack_a_mole.random_crash` 以及崩溃前分数。
3. 进入“哨兵测试”，依次点击非破坏性按钮，检查 Sentry Issues、Logs、Metrics 和 Performance 页面。
4. 断网后点击“断网缓存事件”，恢复网络并重启游戏，检查事件是否补发。
5. 在 Android 真机点击“主线程阻塞”，检查是否出现 ANR 或相关卡顿事件。
6. 在 Windows 或 Android 内部包点击“原生崩溃”，重启后检查 native crash 和 Release Health。
7. IL2CPP 构建后触发异常或崩溃，检查堆栈方法名和行号是否可读。

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

如果需要在 PowerShell 脚本或 CI 中明确阻塞等待 Android 编译完成，并把 Unity 退出码传递给外层流程，可以使用以下命令：

```powershell
$unity = "F:\Unity\6000.3.9f1\Editor\Unity.exe"
$arguments = @(
    "-batchmode",
    "-quit",
    "-projectPath", "F:\UnityProject\SentryGame",
    "-buildTarget", "Android",
    "-executeMethod", "SentryGame.Editor.SentryGameAndroidBuilder.BuildApk",
    "-logFile", "F:\Caches\SentryGame\Builds\Android\android-build.log"
)
$process = Start-Process -FilePath $unity -ArgumentList $arguments -Wait -PassThru
exit $process.ExitCode
```

## 自动化测试

EditMode 测试覆盖登录校验、打地鼠规则、贪吃蛇规则和场景配置。可使用以下命令运行测试：

```powershell
Unity.exe -batchmode -quit -projectPath "F:\UnityProject\SentryGame" -runTests -testPlatform editmode -testResults "F:\Caches\SentryGame\TestResults\editmode-results.xml" -logFile "F:\Caches\SentryGame\TestResults\editmode.log"
```

当前环境如果无法从 PATH 找到 `Unity.exe`，需要使用本机 Unity Editor 的完整路径替换命令中的 `Unity.exe`。
