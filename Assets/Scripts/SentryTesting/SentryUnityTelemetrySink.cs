using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sentry;
using Sentry.Unity;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Networking;

namespace SentryGame.SentryTesting
{
    public sealed class SentryUnityTelemetrySink : ISentryTelemetrySink
    {
        public void AddBreadcrumb(string name, string category, Dictionary<string, string> data)
        {
            SentrySdk.AddBreadcrumb(name, category, "default", data, BreadcrumbLevel.Info);
            Debug.Log($"Sentry 面包屑：{category}/{name}");
        }

        public void CaptureMessage(string message, Dictionary<string, string> tags, string contextName, object context)
        {
            SentrySdk.CaptureMessage(message, scope =>
            {
                ApplyTags(scope, tags);
                scope.Contexts[contextName] = context;
            });
            Debug.Log(message);
        }

        public void CaptureException(Exception exception, Dictionary<string, string> tags, string contextName, object context)
        {
            SentrySdk.CaptureException(exception, scope =>
            {
                ApplyTags(scope, tags);
                scope.Contexts[contextName] = context;
            });
            Debug.LogException(exception);
        }

        public void SetContext(string name, object value)
        {
            SentrySdk.ConfigureScope(scope => scope.Contexts[name] = value);
        }

        public void SetTag(string key, string value)
        {
            SentrySdk.SetTag(key, value);
        }

        public void SetUser(string userId, string userName)
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.User = new SentryUser
                {
                    Id = userId,
                    Username = userName
                };
            });
        }

        public void EmitCounter(string name, double value, Dictionary<string, string> tags)
        {
            SentrySdk.Metrics.EmitCounter(name, value);
        }

        public void EmitGauge(string name, double value, Dictionary<string, string> tags)
        {
            SentrySdk.Metrics.EmitGauge(name, value);
        }

        public void EmitDistribution(string name, double value, Dictionary<string, string> tags)
        {
            SentrySdk.Metrics.EmitDistribution(name, value);
        }

        public void RunCpuStallTrace(int milliseconds, string transactionName, string spanName, Dictionary<string, string> tags, string contextName, object context)
        {
            var transaction = SentrySdk.StartTransaction(transactionName, "gameplay.performance");
            var span = transaction.StartChild(spanName, "CPU 密集卡顿模拟");
            ApplyTags(transaction, tags);
            ApplyTags(span, tags);
            transaction.Contexts[contextName] = context;
            RunCpuIntensiveWork(milliseconds);
            span.Finish(SpanStatus.Ok);
            transaction.Finish(SpanStatus.Ok);
        }

        public void RunDiagnosticCase(string id)
        {
            // 按诊断用例逐一触发 Sentry SDK 能力；破坏性用例只在用户点击对应按钮时执行。
            switch (id)
            {
                case SentryFeatureNames.Diagnostics.CaptureMessage:
                    SentrySdk.CaptureMessage("Sentry 诊断消息");
                    break;
                case SentryFeatureNames.Diagnostics.CaptureException:
                    SentrySdk.CaptureException(new InvalidOperationException("Sentry 诊断异常"));
                    break;
                case SentryFeatureNames.Diagnostics.ThrowUnhandledException:
                    throw new InvalidOperationException("Sentry 未捕获异常诊断");
                case SentryFeatureNames.Diagnostics.LogError:
                    Debug.LogError("Sentry Debug.LogError 诊断");
                    break;
                case SentryFeatureNames.Diagnostics.DuplicateErrors:
                    LogDuplicateErrors();
                    break;
                case SentryFeatureNames.Diagnostics.FailedRequest:
                    CoroutineHost.Run(SendFailedRequest());
                    break;
                case SentryFeatureNames.Diagnostics.ManualTransaction:
                    SendManualTransaction();
                    break;
                case SentryFeatureNames.Diagnostics.Metrics:
                    SendDiagnosticMetrics();
                    break;
                case SentryFeatureNames.Diagnostics.StructuredLog:
                    Debug.Log("Sentry 结构化日志诊断");
                    break;
                case SentryFeatureNames.Diagnostics.Screenshot:
                    SentrySdk.CaptureException(new InvalidOperationException("Sentry 截图附件诊断"));
                    break;
                case SentryFeatureNames.Diagnostics.ViewHierarchy:
                    SentrySdk.CaptureException(new InvalidOperationException("Sentry 视图层级附件诊断"));
                    break;
                case SentryFeatureNames.Diagnostics.OfflineCache:
                    SentrySdk.CaptureMessage("Sentry 离线缓存诊断");
                    break;
                case SentryFeatureNames.Diagnostics.BlockMainThread:
                    Task.Run(() => Thread.Sleep(6000));
                    break;
                case SentryFeatureNames.Diagnostics.NativeCrash:
                    Utils.ForceCrash(ForcedCrashCategory.AccessViolation);
                    break;
                case SentryFeatureNames.Diagnostics.FilteredEvent:
                    SentrySdk.CaptureMessage("Sentry 过滤事件诊断", scope => scope.SetTag("filtered_event", "true"));
                    break;
            }
        }

        private static void ApplyTags(Scope scope, Dictionary<string, string> tags)
        {
            foreach (var pair in tags)
            {
                scope.SetTag(pair.Key, pair.Value);
            }
        }

        private static void ApplyTags(IHasTags target, Dictionary<string, string> tags)
        {
            foreach (var pair in tags)
            {
                target.SetTag(pair.Key, pair.Value);
            }
        }

        private static void RunCpuIntensiveWork(int milliseconds)
        {
            // 使用 CPU 密集循环模拟真实主线程繁忙，避免用 Sleep 制造不可执行的等待。
            var deadline = Time.realtimeSinceStartup + Mathf.Max(1, milliseconds) / 1000f;
            var value = 0.0001d;
            while (Time.realtimeSinceStartup < deadline)
            {
                for (var i = 0; i < 2000; i += 1)
                {
                    value = Math.Sqrt(value + i + 1d);
                }
            }

            if (value < 0d)
            {
                Debug.Log(value);
            }
        }

        private static void LogDuplicateErrors()
        {
            for (var i = 0; i < 5; i += 1)
            {
                Debug.LogError("Sentry 重复错误诊断");
            }
        }

        private static IEnumerator SendFailedRequest()
        {
            using (var request = UnityWebRequest.Get("https://httpstat.us/500"))
            {
                yield return request.SendWebRequest();
                Debug.Log($"Sentry 失败请求诊断完成：{request.responseCode}");
            }
        }

        private static void SendManualTransaction()
        {
            var transaction = SentrySdk.StartTransaction(SentryFeatureNames.Transactions.ManualDiagnostics, "diagnostics");
            var span = transaction.StartChild("diagnostics.step", "手动性能追踪步骤");
            span.Finish(SpanStatus.Ok);
            transaction.Finish(SpanStatus.Ok);
        }

        private static void SendDiagnosticMetrics()
        {
            SentrySdk.Metrics.EmitCounter("diagnostics.counter", 1);
            SentrySdk.Metrics.EmitGauge("diagnostics.gauge", 42);
            SentrySdk.Metrics.EmitDistribution("diagnostics.distribution", 128);
        }

        private sealed class CoroutineHost : MonoBehaviour
        {
            public static void Run(IEnumerator routine)
            {
                var host = new GameObject(nameof(CoroutineHost));
                DontDestroyOnLoad(host);
                host.AddComponent<CoroutineHost>().StartCoroutine(RunAndDestroy(host, routine));
            }

            private static IEnumerator RunAndDestroy(GameObject host, IEnumerator routine)
            {
                yield return routine;
                Destroy(host);
            }
        }
    }
}
