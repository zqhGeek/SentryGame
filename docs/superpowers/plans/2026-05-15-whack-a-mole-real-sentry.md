# 打地鼠真实 Sentry 埋点实施计划

> **给自动化执行者：** 必须使用测试驱动开发。每个行为先写失败测试并运行确认失败，再写最小实现并运行确认通过。步骤使用复选框跟踪。

**目标：** 在打地鼠真实玩法中增加进入、退出、随机卡顿和随机崩溃 Sentry 上报。

**架构：** 扩展现有 `SentryTelemetryService` 和 `ISentryTelemetrySink`，让打地鼠控制器只调用项目语义方法。运行时 sink 负责调用 Sentry SDK、阻塞主线程和抛出异常，测试 sink 只记录调用。

**技术栈：** Unity 6000.3.9f1、UGUI、NUnit EditMode、Sentry Unity SDK。

---

## 文件结构

- 修改 `Assets/Scripts/SentryTesting/SentryFeatureNames.cs`：增加打地鼠生命周期、卡顿和崩溃指标名。
- 修改 `Assets/Scripts/SentryTesting/SentryTelemetrySink.cs`：增加异常捕获和主线程阻塞抽象。
- 修改 `Assets/Scripts/SentryTesting/SentryTelemetryService.cs`：增加打地鼠真实玩法语义埋点。
- 修改 `Assets/Scripts/SentryTesting/SentryUnityTelemetrySink.cs`：实现异常捕获和主线程阻塞。
- 修改 `Assets/Scripts/UI/WhackAMoleSceneController.cs`：接入进入、退出、随机卡顿和随机崩溃。
- 修改 `Assets/Tests/EditMode/SentryTelemetryServiceTests.cs`：增加功能测试。
- 修改 `README.md`：补充功能说明和人工验证步骤。

## 任务 1：遥测服务测试

- [ ] 在 `SentryTelemetryServiceTests` 中新增测试：进入和退出打地鼠会记录对应 breadcrumb、指标、标签和上下文。
- [ ] 运行 EditMode 测试，确认新增测试因方法或指标不存在失败。
- [ ] 在 `SentryFeatureNames`、`ISentryTelemetrySink` 和 `SentryTelemetryService` 中补充最小实现。
- [ ] 再次运行 EditMode 测试，确认测试通过。

## 任务 2：随机卡顿和随机崩溃测试

- [ ] 在 `SentryTelemetryServiceTests` 中新增测试：随机卡顿会记录 breadcrumb、指标、上下文并调用阻塞接口。
- [ ] 在 `SentryTelemetryServiceTests` 中新增测试：随机崩溃会记录 breadcrumb、指标、上下文并调用异常接口。
- [ ] 运行 EditMode 测试，确认新增测试失败。
- [ ] 在服务和运行时 sink 中补充最小实现。
- [ ] 再次运行 EditMode 测试，确认测试通过。

## 任务 3：打地鼠控制器接入

- [ ] 修改 `WhackAMoleSceneController`：`Start` 上报进入，返回模式选择前上报退出。
- [ ] 修改 `WhackAMoleSceneController`：命中或生成地鼠时按低概率触发卡顿，命中后按低概率触发崩溃。
- [ ] 保持概率和卡顿时长为私有常量，避免散落魔法数。
- [ ] 运行 EditMode 测试，确认现有 UI 和遥测测试仍通过。

## 任务 4：文档和最终验证

- [ ] 修改 `README.md`，说明打地鼠真实玩法内新增的 Sentry 上报。
- [ ] 运行完整 EditMode 测试，结果输出到 `F:\Caches\SentryGame\TestResults\whack-a-mole-real-sentry-results.xml`。
- [ ] 检查测试日志，确认没有失败用例。
