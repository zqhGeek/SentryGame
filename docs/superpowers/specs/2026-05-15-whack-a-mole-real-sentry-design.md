# 打地鼠真实 Sentry 埋点设计

## 目标

在打地鼠真实玩法路径中补充 Sentry 数据上报，让进入游戏、退出游戏、随机卡顿和随机崩溃都能被 Sentry 捕获。新增能力复用现有 `SentryTelemetryService` 和 `ISentryTelemetrySink`，避免控制器直接散落调用 Sentry SDK。

## 范围

- 进入打地鼠场景时记录进入事件、标签、上下文和指标。
- 从打地鼠返回模式选择时记录退出事件、标签、上下文和指标。
- 打地鼠游玩过程中按低概率触发主线程短时阻塞，模拟真实应用卡顿。
- 打地鼠命中地鼠后按低概率触发未捕获异常，模拟真实崩溃。
- README 同步说明真实玩法路径中的 Sentry 覆盖点。

## 架构

`WhackAMoleSceneController` 只负责在生命周期和玩法事件中调用语义化遥测方法，不直接依赖 Sentry SDK。`SentryTelemetryService` 负责统一生成稳定的事件名、指标名、标签和上下文。`SentryUnityTelemetrySink` 负责把语义事件转换为 Sentry SDK 调用、Unity 日志、主线程阻塞和异常抛出。

## 测试

新增 EditMode 测试验证遥测服务会对进入、退出、随机卡顿和随机崩溃生成预期数据。自动化测试不依赖真实 Sentry 后台，不真实触发崩溃，只通过测试 sink 记录调用。真实 Sentry 后台效果通过 README 中的人工验证步骤确认。
