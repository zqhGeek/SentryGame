# Sentry 功能覆盖测试设计

## 背景

项目是一个 Unity 竖屏 Android 游戏，已经接入 Sentry Unity SDK。当前游戏包含登录、模式选择、打地鼠和贪吃蛇四个场景。测试目标是尽可能覆盖 Sentry Unity SDK 提供的功能，并且优先使用真实玩法行为触发，只有无法自然模拟或会破坏正常流程的能力才通过诊断按钮单独触发。

本项目是内部使用 APK，因此测试入口不需要隐藏，Release 包体中可以保留所有 Sentry 测试能力。

## 目标

- 在真实游戏流程中覆盖用户、标签、上下文、面包屑、日志、指标、性能追踪、Release Health 和场景加载追踪。
- 通过明确诊断入口覆盖异常、未捕获异常、失败请求、离线缓存、主线程卡顿、原生崩溃、截图、视图层级、采样过滤和防抖限流。
- 打开 Sentry Unity SDK 当前可打开的测试相关功能，使内部包尽可能暴露完整 Sentry 行为。
- 增加功能测试，验证埋点入口、事件命名和上下文字段符合预期。
- 更新 README，说明测试入口、功能覆盖和人工验证步骤。

## 非目标

- 不接入真实账号系统。
- 不把 Sentry 后台作为自动化测试依赖。
- 不为外部玩家隐藏诊断入口，因为当前 APK 只供内部使用。
- 不实现新的玩法内容，只在现有流程中增加观测和诊断能力。

## 配置设计

修改 `Assets/Resources/Sentry/SentryOptions.asset`，打开测试覆盖所需能力：

- `TracesSampleRate` 设置为 `1`。
- 保持 `AutoStartupTraces` 和 `AutoSceneLoadTraces` 开启。
- 打开 `AutoAwakeTraces`。
- 打开 `AttachStacktrace`。
- 打开 `AttachScreenshot`。
- 打开 `AttachViewHierarchy`。
- 打开 `EnableStructuredLogging`。
- 打开 `AddBreadcrumbsWithStructuredLogs`。
- 保持 `EnableOfflineCaching` 开启。
- 保持 `EnableMetrics` 开启。
- 保持 `AutoSessionTracking` 开启。
- 保持 `CaptureFailedRequests` 开启。
- 打开 `SendDefaultPii`，便于内部测试用户信息。
- 打开 `EnableLogDebouncing`。
- 打开 `EnableErrorEventThrottling`。
- 保持 Windows、Android、iOS、macOS、Linux 原生支持开启。
- 保持 `Il2CppLineNumberSupportEnabled` 开启。

## 架构设计

新增 `SentryTelemetryService`，负责封装项目内所有 Sentry 调用。玩法控制器不直接散落调用 Sentry SDK，而是调用语义化方法，例如登录失败、选择模式、打中地鼠、贪吃蛇吃到食物、游戏结束和诊断异常。

新增 `SentryDiagnosticsPanel`，在独立诊断场景中提供可见按钮。按钮只用于触发无法通过真实玩法稳定覆盖的能力，例如未捕获异常、原生崩溃、主线程卡顿、失败请求和离线缓存验证。

新增 `SentryDiagnosticScene` 承载诊断按钮。在现有 `ModeSelectScene` 中增加可见的“哨兵测试”入口按钮，点击后进入诊断场景。诊断场景提供返回模式选择按钮，避免阻断真实玩法流程。

## 真实玩法埋点

### 登录场景

登录失败时记录面包屑、结构化日志和失败计数指标。登录成功时设置测试用户、登录标签和场景上下文。连续登录失败时记录警告日志，模拟真实线上异常行为但不抛出异常。

### 模式选择场景

进入打地鼠或贪吃蛇时记录模式选择面包屑、标签和上下文。场景切换依赖 Sentry 自动场景加载追踪，同时手动补充选择来源和目标场景。

### 打地鼠场景

打中红点时记录命中指标、分数上下文和操作面包屑。漏点扣分时记录警告日志和漏点指标。分数归零时记录游戏结束消息，并附加最终分数、持续时间和失败原因。

### 贪吃蛇场景

方向变化时记录操作面包屑。吃到食物时记录分数指标和食物计数。撞墙或撞到自身时记录游戏结束消息，并附加最终分数、蛇长度、方向和失败原因。

## 诊断功能

诊断入口提供以下可见按钮：

- 发送消息，验证 `CaptureMessage`。
- 发送异常，验证 `CaptureException`。
- 抛出未捕获异常，验证自动异常捕获。
- 发送 `Debug.LogError`，验证 Unity 日志错误捕获。
- 连续发送重复错误，验证防抖和限流。
- 发送失败请求，验证失败请求捕获。
- 发送手动性能事务和 span，验证性能追踪。
- 发送 counter、gauge 和 distribution，验证指标。
- 发送结构化日志，验证日志检索。
- 触发带截图的异常，验证截图附件。
- 触发带视图层级的异常，验证视图层级附件。
- 断网后发送事件，验证离线缓存。
- 主线程阻塞，验证 Android ANR。
- 触发原生崩溃，验证 native crash。
- 发送应被过滤的事件，验证过滤逻辑。

## 数据命名

事件名、标签名和指标名使用稳定英文键，便于在 Sentry 后台检索。界面展示和 README 使用中文。

建议命名：

- 标签：`scene`、`game_mode`、`test_case`、`failure_reason`。
- 上下文：`login`、`whack_a_mole`、`snake`、`diagnostics`。
- 指标：`login.failure`、`mole.hit`、`mole.missed`、`snake.food_eaten`、`game.score`、`diagnostics.button_click`。
- 事务：`game.login_flow`、`game.mode_select`、`game.whack_a_mole.round`、`game.snake.round`、`diagnostics.manual_transaction`。

## 测试设计

新增 EditMode 测试，不真实上报 Sentry 后台。测试验证项目自己的遥测服务能生成正确语义和字段：

- 登录失败会产生 `login.failure` 指标和登录失败面包屑。
- 登录成功会设置测试用户和登录上下文。
- 模式选择会记录目标玩法标签。
- 打地鼠命中和漏点会记录对应指标。
- 打地鼠游戏结束会记录最终分数和失败原因。
- 贪吃蛇吃食物和游戏结束会记录对应指标和上下文。
- 诊断按钮列表包含所有设计的 Sentry 功能。
- Sentry 配置资产打开预期功能。

人工验证覆盖真实 Sentry 后台：

- Editor Play 模式验证基础事件、日志、面包屑、上下文和性能事务。
- Windows 构建验证 Release Health、离线缓存、截图、视图层级和原生支持。
- Android 真机验证 Release Health、ANR、失败请求、离线缓存和 native crash。
- IL2CPP 构建验证堆栈可读性和行号支持。

## README 更新

README 需要新增 Sentry 测试说明：

- 说明模式选择中可以进入 Sentry 诊断功能。
- 说明真实玩法中的自动埋点覆盖。
- 说明每个诊断按钮对应的 Sentry 功能。
- 说明 Windows 和 Android 验证步骤。
- 说明离线缓存、ANR、原生崩溃和符号化需要人工验证。

## 验收标准

- 完成一次登录失败、登录成功、进入玩法、游戏结束和返回模式选择后，Sentry 后台能看到用户、标签、上下文、面包屑、日志、指标和场景加载追踪。
- 点击诊断入口的异常按钮后，事件详情包含堆栈、截图、视图层级、用户、标签、上下文和面包屑。
- 点击性能诊断按钮后，Sentry Performance 中能看到手动事务和 span。
- 断网触发事件并恢复网络后，缓存事件能够补发。
- Android 真机触发主线程阻塞后，可以在 Sentry 中验证 ANR 或相关卡顿事件。
- 自动化测试通过，并覆盖玩法埋点服务和 Sentry 配置检查。
- README 已同步描述 Sentry 功能测试方式。
