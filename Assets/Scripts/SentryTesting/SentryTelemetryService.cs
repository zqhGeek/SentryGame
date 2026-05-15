using System;
using System.Collections.Generic;

namespace SentryGame.SentryTesting
{
    public static class SentryTelemetryService
    {
        private static readonly ISentryTelemetrySink NullSink = new NullSentryTelemetrySink();
        private static readonly ISentryTelemetrySink RuntimeSink = new SentryUnityTelemetrySink();

        public static ISentryTelemetrySink Sink { get; set; } = RuntimeSink;

        public static void ResetSink()
        {
            Sink = RuntimeSink;
        }

        public static void RecordLoginFailure(string userName, int failureCount)
        {
            Sink.AddBreadcrumb("login.failed", "auth", new Dictionary<string, string>
            {
                { "user_name", userName },
                { "failure_count", failureCount.ToString() }
            });
            Sink.CaptureMessage($"登录失败：{userName}，次数：{failureCount}", Tag("scene", "LoginScene"), SentryFeatureNames.Contexts.Login, new LoginContext(userName, false, failureCount));
            Sink.EmitCounter(SentryFeatureNames.Metrics.LoginFailure, 1, Tag("scene", "LoginScene"));
        }

        public static void RecordLoginSuccess(string userName)
        {
            Sink.SetUser(userName, userName);
            Sink.SetTag(SentryFeatureNames.Tags.Scene, "LoginScene");
            Sink.SetContext(SentryFeatureNames.Contexts.Login, new LoginContext(userName, true, 0));
            Sink.AddBreadcrumb("login.success", "auth", Tag("user_name", userName));
        }

        public static void RecordModeSelected(string gameMode, string targetScene)
        {
            Sink.SetTag(SentryFeatureNames.Tags.GameMode, gameMode);
            Sink.SetTag(SentryFeatureNames.Tags.Scene, targetScene);
            Sink.AddBreadcrumb("mode.selected", "navigation", new Dictionary<string, string>
            {
                { "game_mode", gameMode },
                { "target_scene", targetScene }
            });
        }

        public static void RecordSceneLoad(string targetScene)
        {
            Sink.SetTag(SentryFeatureNames.Tags.Scene, targetScene);
            Sink.AddBreadcrumb("scene.load", "navigation", Tag("target_scene", targetScene));
        }

        public static void RecordMoleHit(int score)
        {
            Sink.AddBreadcrumb("mole.hit", "gameplay", Tag("score", score.ToString()));
            Sink.EmitCounter(SentryFeatureNames.Metrics.MoleHit, 1, Tag("game_mode", "whack_a_mole"));
            Sink.EmitGauge(SentryFeatureNames.Metrics.GameScore, score, Tag("game_mode", "whack_a_mole"));
        }

        public static void RecordMoleMissed(int score)
        {
            Sink.AddBreadcrumb("mole.missed", "gameplay", Tag("score", score.ToString()));
            Sink.EmitCounter(SentryFeatureNames.Metrics.MoleMissed, 1, Tag("game_mode", "whack_a_mole"));
            Sink.EmitGauge(SentryFeatureNames.Metrics.GameScore, score, Tag("game_mode", "whack_a_mole"));
        }

        public static void RecordWhackAMoleEntered(int initialScore)
        {
            Sink.SetTag(SentryFeatureNames.Tags.GameMode, "whack_a_mole");
            Sink.SetTag(SentryFeatureNames.Tags.Scene, "WhackAMoleScene");
            Sink.SetContext(SentryFeatureNames.Contexts.WhackAMole, new WhackAMoleLifecycleContext(initialScore, "enter"));
            Sink.AddBreadcrumb("whack_a_mole.enter", "gameplay", Tag("initial_score", initialScore.ToString()));
            Sink.EmitCounter(SentryFeatureNames.Metrics.WhackAMoleEntered, 1, Tag("game_mode", "whack_a_mole"));
            Sink.EmitGauge(SentryFeatureNames.Metrics.GameScore, initialScore, Tag("game_mode", "whack_a_mole"));
        }

        public static void RecordWhackAMoleExited(int finalScore, string reason)
        {
            // 记录离开打地鼠时的最终状态，方便在 Sentry 中串联完整玩法路径。
            Sink.SetTag(SentryFeatureNames.Tags.GameMode, "whack_a_mole");
            Sink.SetTag(SentryFeatureNames.Tags.FailureReason, reason);
            Sink.SetContext(SentryFeatureNames.Contexts.WhackAMole, new WhackAMoleLifecycleContext(finalScore, reason));
            Sink.AddBreadcrumb("whack_a_mole.exit", "gameplay", new Dictionary<string, string>
            {
                { "final_score", finalScore.ToString() },
                { "reason", reason }
            });
            Sink.EmitCounter(SentryFeatureNames.Metrics.WhackAMoleExited, 1, Tag("game_mode", "whack_a_mole"));
            Sink.EmitGauge(SentryFeatureNames.Metrics.GameScore, finalScore, Tag("game_mode", "whack_a_mole"));
        }

        public static void RecordWhackAMoleRandomStall(int durationMilliseconds, int score)
        {
            // 上报真实玩法中的随机卡顿，并通过 Sentry transaction/span 包裹计算负载。
            var tags = Tag("game_mode", "whack_a_mole");
            var context = new WhackAMoleFaultContext(score, "random_stall", durationMilliseconds);
            Sink.SetContext(SentryFeatureNames.Contexts.WhackAMole, context);
            Sink.AddBreadcrumb("whack_a_mole.random_stall", "performance", new Dictionary<string, string>
            {
                { "score", score.ToString() },
                { "duration_ms", durationMilliseconds.ToString() }
            });
            Sink.EmitCounter(SentryFeatureNames.Metrics.WhackAMoleRandomStall, 1, tags);
            Sink.EmitDistribution(SentryFeatureNames.Metrics.WhackAMoleStallDuration, durationMilliseconds, tags);
            Sink.RunCpuStallTrace(durationMilliseconds, SentryFeatureNames.Transactions.WhackAMoleRandomStall, SentryFeatureNames.Spans.WhackAMoleRewardAppeared, "奖励出现了", tags, SentryFeatureNames.Contexts.WhackAMole, context);
        }

        public static void RecordSnakeRandomStall(int durationMilliseconds, int score, int length)
        {
            // 上报贪吃蛇奖励出现时的随机卡顿，并通过蛇专用 transaction/span 区分玩法。
            var tags = Tag("game_mode", "snake");
            var context = new SnakeFaultContext(score, length, 0, "reward_spawned", durationMilliseconds);
            Sink.SetTag(SentryFeatureNames.Tags.GameMode, "snake");
            Sink.SetContext(SentryFeatureNames.Contexts.Snake, context);
            Sink.AddBreadcrumb("snake.random_stall", "performance", new Dictionary<string, string>
            {
                { "score", score.ToString() },
                { "length", length.ToString() },
                { "duration_ms", durationMilliseconds.ToString() }
            });
            Sink.EmitCounter(SentryFeatureNames.Metrics.SnakeRandomStall, 1, tags);
            Sink.EmitDistribution(SentryFeatureNames.Metrics.SnakeStallDuration, durationMilliseconds, tags);
            Sink.RunCpuStallTrace(durationMilliseconds, SentryFeatureNames.Transactions.SnakeRandomStall, SentryFeatureNames.Spans.SnakeRewardAppeared, "奖励出现了", tags, SentryFeatureNames.Contexts.Snake, context);
        }

        public static void RecordWhackAMoleRandomCrash(int score, string reason)
        {
            // 先把崩溃前上下文写入 Sentry，再抛出未捕获异常模拟真实崩溃。
            var exception = new InvalidOperationException($"打地鼠随机崩溃：{reason}");
            var context = new WhackAMoleFaultContext(score, reason, 0);
            Sink.SetContext(SentryFeatureNames.Contexts.WhackAMole, context);
            Sink.AddBreadcrumb("whack_a_mole.random_crash", "error", new Dictionary<string, string>
            {
                { "score", score.ToString() },
                { "reason", reason }
            });
            Sink.EmitCounter(SentryFeatureNames.Metrics.WhackAMoleRandomCrash, 1, Tag("game_mode", "whack_a_mole"));
            Sink.CaptureException(exception, Tag("game_mode", "whack_a_mole"), SentryFeatureNames.Contexts.WhackAMole, context);
            throw exception;
        }

        public static void RecordSnakeRandomCrash(int score, int length, int moveCount, string reason)
        {
            // 只在控制器确认步数满足条件后调用；这里负责补充崩溃前的蛇玩法上下文。
            var exception = new InvalidOperationException($"贪吃蛇随机崩溃：{reason}");
            var context = new SnakeFaultContext(score, length, moveCount, reason, 0);
            Sink.SetTag(SentryFeatureNames.Tags.GameMode, "snake");
            Sink.SetContext(SentryFeatureNames.Contexts.Snake, context);
            Sink.AddBreadcrumb("snake.random_crash", "error", new Dictionary<string, string>
            {
                { "score", score.ToString() },
                { "length", length.ToString() },
                { "move_count", moveCount.ToString() },
                { "reason", reason }
            });
            Sink.EmitCounter(SentryFeatureNames.Metrics.SnakeRandomCrash, 1, Tag("game_mode", "snake"));
            Sink.CaptureException(exception, Tag("game_mode", "snake"), SentryFeatureNames.Contexts.Snake, context);
            throw exception;
        }

        public static void RecordSnakeFoodEaten(int score, int length)
        {
            Sink.AddBreadcrumb("snake.food_eaten", "gameplay", new Dictionary<string, string>
            {
                { "score", score.ToString() },
                { "length", length.ToString() }
            });
            Sink.EmitCounter(SentryFeatureNames.Metrics.SnakeFoodEaten, 1, Tag("game_mode", "snake"));
            Sink.EmitGauge(SentryFeatureNames.Metrics.GameScore, score, Tag("game_mode", "snake"));
        }

        public static void RecordWhackAMoleGameOver(int finalScore, string reason)
        {
            Sink.SetTag(SentryFeatureNames.Tags.FailureReason, reason);
            Sink.SetContext(SentryFeatureNames.Contexts.WhackAMole, new GameOverContext(finalScore, reason));
            Sink.CaptureMessage("打地鼠游戏结束", Tag("game_mode", "whack_a_mole"), SentryFeatureNames.Contexts.WhackAMole, new GameOverContext(finalScore, reason));
        }

        public static void RecordSnakeDirection(string direction)
        {
            Sink.AddBreadcrumb("snake.direction", "gameplay", Tag("direction", direction));
        }

        public static void RecordSnakeGameOver(int finalScore, int length, string reason)
        {
            Sink.SetTag(SentryFeatureNames.Tags.FailureReason, reason);
            Sink.SetContext(SentryFeatureNames.Contexts.Snake, new SnakeGameOverContext(finalScore, length, reason));
            Sink.CaptureMessage("贪吃蛇游戏结束", Tag("game_mode", "snake"), SentryFeatureNames.Contexts.Snake, new SnakeGameOverContext(finalScore, length, reason));
        }

        public static SentryDiagnosticCase[] GetDiagnosticCases()
        {
            // 返回内部 APK 中直接可见的诊断按钮清单，覆盖无法稳定通过真实玩法触发的 Sentry 能力。
            return new[]
            {
                new SentryDiagnosticCase(SentryFeatureNames.Diagnostics.CaptureMessage, "发送消息"),
                new SentryDiagnosticCase(SentryFeatureNames.Diagnostics.CaptureException, "发送异常"),
                new SentryDiagnosticCase(SentryFeatureNames.Diagnostics.ThrowUnhandledException, "抛出未捕获异常"),
                new SentryDiagnosticCase(SentryFeatureNames.Diagnostics.LogError, "发送日志错误"),
                new SentryDiagnosticCase(SentryFeatureNames.Diagnostics.DuplicateErrors, "连续重复错误"),
                new SentryDiagnosticCase(SentryFeatureNames.Diagnostics.FailedRequest, "触发失败请求"),
                new SentryDiagnosticCase(SentryFeatureNames.Diagnostics.ManualTransaction, "发送性能追踪"),
                new SentryDiagnosticCase(SentryFeatureNames.Diagnostics.Metrics, "发送指标"),
                new SentryDiagnosticCase(SentryFeatureNames.Diagnostics.StructuredLog, "发送结构化日志"),
                new SentryDiagnosticCase(SentryFeatureNames.Diagnostics.Screenshot, "触发截图事件"),
                new SentryDiagnosticCase(SentryFeatureNames.Diagnostics.ViewHierarchy, "触发视图层级事件"),
                new SentryDiagnosticCase(SentryFeatureNames.Diagnostics.OfflineCache, "断网缓存事件"),
                new SentryDiagnosticCase(SentryFeatureNames.Diagnostics.BlockMainThread, "主线程阻塞"),
                new SentryDiagnosticCase(SentryFeatureNames.Diagnostics.NativeCrash, "原生崩溃"),
                new SentryDiagnosticCase(SentryFeatureNames.Diagnostics.FilteredEvent, "过滤事件")
            };
        }

        public static void RunDiagnosticCase(string id)
        {
            Sink.AddBreadcrumb("diagnostics.button_click", "diagnostics", Tag("test_case", id));
            Sink.EmitCounter(SentryFeatureNames.Metrics.DiagnosticsButtonClick, 1, Tag("test_case", id));
            Sink.RunDiagnosticCase(id);
        }

        private static Dictionary<string, string> Tag(string key, string value)
        {
            return new Dictionary<string, string> { { key, value } };
        }

        private readonly struct LoginContext
        {
            public LoginContext(string userName, bool success, int failureCount)
            {
                UserName = userName;
                Success = success;
                FailureCount = failureCount;
            }

            public string UserName { get; }

            public bool Success { get; }

            public int FailureCount { get; }
        }

        private readonly struct GameOverContext
        {
            public GameOverContext(int finalScore, string reason)
            {
                FinalScore = finalScore;
                Reason = reason;
            }

            public int FinalScore { get; }

            public string Reason { get; }
        }

        private readonly struct WhackAMoleLifecycleContext
        {
            public WhackAMoleLifecycleContext(int score, string reason)
            {
                Score = score;
                Reason = reason;
            }

            public int Score { get; }

            public string Reason { get; }
        }

        private readonly struct WhackAMoleFaultContext
        {
            public WhackAMoleFaultContext(int score, string reason, int durationMilliseconds)
            {
                Score = score;
                Reason = reason;
                DurationMilliseconds = durationMilliseconds;
            }

            public int Score { get; }

            public string Reason { get; }

            public int DurationMilliseconds { get; }
        }

        private readonly struct SnakeGameOverContext
        {
            public SnakeGameOverContext(int finalScore, int length, string reason)
            {
                FinalScore = finalScore;
                Length = length;
                Reason = reason;
            }

            public int FinalScore { get; }

            public int Length { get; }

            public string Reason { get; }
        }

        private readonly struct SnakeFaultContext
        {
            public SnakeFaultContext(int score, int length, int moveCount, string reason, int durationMilliseconds)
            {
                Score = score;
                Length = length;
                MoveCount = moveCount;
                Reason = reason;
                DurationMilliseconds = durationMilliseconds;
            }

            public int Score { get; }

            public int Length { get; }

            public int MoveCount { get; }

            public string Reason { get; }

            public int DurationMilliseconds { get; }
        }

        private sealed class NullSentryTelemetrySink : ISentryTelemetrySink
        {
            public void AddBreadcrumb(string name, string category, Dictionary<string, string> data)
            {
            }

            public void CaptureMessage(string message, Dictionary<string, string> tags, string contextName, object context)
            {
            }

            public void CaptureException(Exception exception, Dictionary<string, string> tags, string contextName, object context)
            {
            }

            public void SetContext(string name, object value)
            {
            }

            public void SetTag(string key, string value)
            {
            }

            public void SetUser(string userId, string userName)
            {
            }

            public void EmitCounter(string name, double value, Dictionary<string, string> tags)
            {
            }

            public void EmitGauge(string name, double value, Dictionary<string, string> tags)
            {
            }

            public void EmitDistribution(string name, double value, Dictionary<string, string> tags)
            {
            }

            public void RunCpuStallTrace(int milliseconds, string transactionName, string spanName, string spanDescription, Dictionary<string, string> tags, string contextName, object context)
            {
            }

            public void RunDiagnosticCase(string id)
            {
            }
        }
    }
}
