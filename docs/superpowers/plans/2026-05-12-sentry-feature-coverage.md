# Sentry 功能覆盖实施计划

> **给自动化执行者：** 实施时必须逐项勾选本计划。新增或改变行为前先写失败测试，再写最小实现，最后运行验证命令。所有临时输出写入 `F:\Caches\SentryGame`。

**目标：** 将 Sentry 功能测试融合进登录、模式选择、打地鼠、贪吃蛇真实流程，并增加可见诊断场景覆盖破坏性能力。

**架构：** 新增遥测抽象层记录测试意图，运行时适配器负责调用 Sentry SDK。玩法控制器只调用项目语义方法，诊断控制器只承载无法自然触发的测试能力。

**技术栈：** Unity 6000.3.9f1、UGUI、NUnit EditMode、Sentry Unity SDK。

---

## 文件结构

- 新增 `Assets/Scripts/SentryTesting/SentryFeatureNames.cs`：集中定义标签、上下文、指标、事务和诊断用例名称。
- 新增 `Assets/Scripts/SentryTesting/SentryTelemetrySink.cs`：定义遥测接收接口和记录数据结构，供测试验证。
- 新增 `Assets/Scripts/SentryTesting/SentryUnityTelemetrySink.cs`：调用 Sentry SDK、Unity 日志、UnityWebRequest 和强制崩溃能力。
- 新增 `Assets/Scripts/SentryTesting/SentryTelemetryService.cs`：玩法语义埋点入口。
- 新增 `Assets/Scripts/UI/SentryDiagnosticsSceneController.cs`：诊断场景按钮控制器。
- 修改 `Assets/Scripts/Common/GameSceneNames.cs`：增加诊断场景名。
- 修改 `Assets/Scripts/UI/LoginSceneController.cs`：接入登录成功和失败埋点。
- 修改 `Assets/Scripts/UI/ModeSelectSceneController.cs`：增加 Sentry 测试入口并记录模式选择。
- 修改 `Assets/Scripts/UI/WhackAMoleSceneController.cs`：接入命中、漏点、结束、重开和返回埋点。
- 修改 `Assets/Scripts/UI/SnakeSceneController.cs`：接入方向、吃食物、结束和返回埋点。
- 修改 `Assets/Scripts/UI/SceneLoader.cs`：场景跳转前记录面包屑。
- 修改 `Assets/Editor/SentryGameSceneBuilder.cs`：生成诊断场景，更新构建列表。
- 修改 `Assets/Resources/Sentry/SentryOptions.asset`：打开完整测试配置。
- 新增 `Assets/Tests/EditMode/SentryTelemetryServiceTests.cs`：验证遥测服务行为。
- 修改 `Assets/Tests/EditMode/SceneConfigurationTests.cs`：验证诊断场景、构建列表和配置资产。
- 修改 `Assets/Scripts/SentryGame.Runtime.asmdef`：引用 Sentry 运行时程序集。
- 修改 `Assets/Tests/EditMode/SentryGame.EditModeTests.asmdef`：必要时引用 Sentry 运行时程序集。
- 修改 `README.md`：中文说明 Sentry 测试方式。

## 任务 1：遥测服务测试和抽象

- [ ] 写失败测试：`SentryTelemetryServiceTests` 验证登录失败、登录成功、模式选择、玩法指标和诊断用例列表。
- [ ] 运行 EditMode 测试，预期因类型不存在失败。
- [ ] 新增 `SentryFeatureNames`、`SentryTelemetrySink`、`SentryTelemetryService`，只记录项目语义，不直接依赖 UI。
- [ ] 运行 EditMode 测试，预期新增测试通过。
- [ ] 提交 `feat: add sentry telemetry model`。

## 任务 2：真实玩法融合

- [ ] 写失败测试：验证控制器暴露诊断入口方法，场景名包含诊断场景，场景文件构建列表期望包含第五个场景。
- [ ] 运行 EditMode 测试，预期因诊断场景和入口不存在失败。
- [ ] 修改登录、模式选择、打地鼠、贪吃蛇和场景加载器，调用遥测服务。
- [ ] 运行 EditMode 测试，预期控制器 API 相关测试通过。
- [ ] 提交 `feat: wire sentry telemetry into gameplay`。

## 任务 3：诊断场景和 Sentry SDK 适配器

- [ ] 写失败测试：验证诊断按钮用例覆盖消息、异常、未捕获异常、日志错误、重复错误、失败请求、性能事务、指标、结构化日志、截图、视图层级、离线缓存、主线程阻塞、原生崩溃和过滤事件。
- [ ] 运行 EditMode 测试，预期因诊断控制器不存在失败。
- [ ] 新增 `SentryUnityTelemetrySink` 和 `SentryDiagnosticsSceneController`。
- [ ] 修改场景构建器，生成诊断场景和模式选择入口。
- [ ] 运行 Unity 菜单构建命令重建场景。
- [ ] 运行 EditMode 测试，预期诊断相关测试通过。
- [ ] 提交 `feat: add sentry diagnostics scene`。

## 任务 4：配置全开和文档

- [ ] 写失败测试：验证 `SentryOptions.asset` 中关键配置字段已开启。
- [ ] 运行 EditMode 测试，预期因配置未全开失败。
- [ ] 修改 `SentryOptions.asset` 打开 tracing、截图、视图层级、结构化日志、用户信息、限流、防抖和 Awake tracing。
- [ ] 修改 README，增加 Sentry 测试入口、真实玩法覆盖、诊断按钮和人工验证步骤。
- [ ] 运行 EditMode 测试，预期配置测试通过。
- [ ] 提交 `docs: document sentry feature testing`。

## 任务 5：最终验证

- [ ] 运行完整 EditMode 测试，输出到 `F:\Caches\SentryGame\TestResults\sentry-feature-results.xml`。
- [ ] 运行场景构建命令，输出到 `F:\Caches\SentryGame\SceneBuild\sentry-feature-build-scenes.log`。
- [ ] 读取测试结果和日志，确认失败数。
- [ ] 检查 `git status --short`，确认只包含本任务相关文件和既有未跟踪目录。
