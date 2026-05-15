using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using SentryGame.SentryTesting;

namespace SentryGame.Tests.EditMode
{
    public sealed class SentryTelemetryServiceTests
    {
        [SetUp]
        public void SetUp()
        {
            SentryTelemetryService.Sink = new RecordingSentryTelemetrySink();
        }

        [TearDown]
        public void TearDown()
        {
            SentryTelemetryService.ResetSink();
        }

        [Test]
        public void LoginFailureRecordsBreadcrumbLogAndMetric()
        {
            var sink = (RecordingSentryTelemetrySink)SentryTelemetryService.Sink;

            SentryTelemetryService.RecordLoginFailure("admin", 2);

            Assert.That(sink.Breadcrumbs.Count(item => item == "login.failed"), Is.EqualTo(1));
            Assert.That(sink.Logs.Count(item => item == "登录失败：admin，次数：2"), Is.EqualTo(1));
            Assert.That(sink.Metrics.Count(item => item == SentryFeatureNames.Metrics.LoginFailure), Is.EqualTo(1));
        }

        [Test]
        public void LoginSuccessSetsUserAndContext()
        {
            var sink = (RecordingSentryTelemetrySink)SentryTelemetryService.Sink;

            SentryTelemetryService.RecordLoginSuccess("admin");

            Assert.That(sink.UserIds.Count(item => item == "admin"), Is.EqualTo(1));
            Assert.That(sink.Tags.Count(item => item == "scene=LoginScene"), Is.EqualTo(1));
            Assert.That(sink.Contexts.Count(item => item == "login"), Is.EqualTo(1));
        }

        [Test]
        public void GameplayEventsRecordExpectedMetricNames()
        {
            var sink = (RecordingSentryTelemetrySink)SentryTelemetryService.Sink;

            SentryTelemetryService.RecordMoleHit(6);
            SentryTelemetryService.RecordMoleMissed(4);
            SentryTelemetryService.RecordSnakeFoodEaten(3, 4);

            Assert.That(sink.Metrics, Does.Contain(SentryFeatureNames.Metrics.MoleHit));
            Assert.That(sink.Metrics, Does.Contain(SentryFeatureNames.Metrics.MoleMissed));
            Assert.That(sink.Metrics, Does.Contain(SentryFeatureNames.Metrics.SnakeFoodEaten));
            Assert.That(sink.Metrics, Does.Contain(SentryFeatureNames.Metrics.GameScore));
        }

        [Test]
        public void WhackAMoleLifecycleRecordsEnterAndExitTelemetry()
        {
            var sink = (RecordingSentryTelemetrySink)SentryTelemetryService.Sink;

            SentryTelemetryService.RecordWhackAMoleEntered(5);
            SentryTelemetryService.RecordWhackAMoleExited(7, "back_button");

            Assert.That(sink.Breadcrumbs, Does.Contain("whack_a_mole.enter"));
            Assert.That(sink.Breadcrumbs, Does.Contain("whack_a_mole.exit"));
            Assert.That(sink.Metrics, Does.Contain(SentryFeatureNames.Metrics.WhackAMoleEntered));
            Assert.That(sink.Metrics, Does.Contain(SentryFeatureNames.Metrics.WhackAMoleExited));
            Assert.That(sink.Tags, Does.Contain("game_mode=whack_a_mole"));
            Assert.That(sink.Contexts, Does.Contain(SentryFeatureNames.Contexts.WhackAMole));
        }

        [Test]
        public void WhackAMoleRandomStallRecordsTelemetryAndBlocksMainThread()
        {
            var sink = (RecordingSentryTelemetrySink)SentryTelemetryService.Sink;

            SentryTelemetryService.RecordWhackAMoleRandomStall(350, 8);

            Assert.That(sink.Breadcrumbs, Does.Contain("whack_a_mole.random_stall"));
            Assert.That(sink.Metrics, Does.Contain(SentryFeatureNames.Metrics.WhackAMoleRandomStall));
            Assert.That(sink.Distributions, Does.Contain(SentryFeatureNames.Metrics.WhackAMoleStallDuration));
            Assert.That(sink.BlockDurations, Does.Contain(350));
            Assert.That(sink.Contexts, Does.Contain(SentryFeatureNames.Contexts.WhackAMole));
        }

        [Test]
        public void WhackAMoleRandomCrashRecordsTelemetryAndCapturesException()
        {
            var sink = (RecordingSentryTelemetrySink)SentryTelemetryService.Sink;

            Assert.Throws<System.InvalidOperationException>(() => SentryTelemetryService.RecordWhackAMoleRandomCrash(9, "mole_hit"));

            Assert.That(sink.Breadcrumbs, Does.Contain("whack_a_mole.random_crash"));
            Assert.That(sink.Metrics, Does.Contain(SentryFeatureNames.Metrics.WhackAMoleRandomCrash));
            Assert.That(sink.Exceptions, Does.Contain("打地鼠随机崩溃：mole_hit"));
            Assert.That(sink.Contexts, Does.Contain(SentryFeatureNames.Contexts.WhackAMole));
        }

        [Test]
        public void DiagnosticsCatalogContainsDestructiveAndNonDestructiveCases()
        {
            var cases = SentryTelemetryService.GetDiagnosticCases().Select(testCase => testCase.Id).ToArray();

            Assert.That(cases, Does.Contain(SentryFeatureNames.Diagnostics.CaptureMessage));
            Assert.That(cases, Does.Contain(SentryFeatureNames.Diagnostics.CaptureException));
            Assert.That(cases, Does.Contain(SentryFeatureNames.Diagnostics.ThrowUnhandledException));
            Assert.That(cases, Does.Contain(SentryFeatureNames.Diagnostics.LogError));
            Assert.That(cases, Does.Contain(SentryFeatureNames.Diagnostics.DuplicateErrors));
            Assert.That(cases, Does.Contain(SentryFeatureNames.Diagnostics.FailedRequest));
            Assert.That(cases, Does.Contain(SentryFeatureNames.Diagnostics.ManualTransaction));
            Assert.That(cases, Does.Contain(SentryFeatureNames.Diagnostics.Metrics));
            Assert.That(cases, Does.Contain(SentryFeatureNames.Diagnostics.StructuredLog));
            Assert.That(cases, Does.Contain(SentryFeatureNames.Diagnostics.Screenshot));
            Assert.That(cases, Does.Contain(SentryFeatureNames.Diagnostics.ViewHierarchy));
            Assert.That(cases, Does.Contain(SentryFeatureNames.Diagnostics.OfflineCache));
            Assert.That(cases, Does.Contain(SentryFeatureNames.Diagnostics.BlockMainThread));
            Assert.That(cases, Does.Contain(SentryFeatureNames.Diagnostics.NativeCrash));
            Assert.That(cases, Does.Contain(SentryFeatureNames.Diagnostics.FilteredEvent));
        }

        private sealed class RecordingSentryTelemetrySink : ISentryTelemetrySink
        {
            public readonly List<string> Breadcrumbs = new List<string>();
            public readonly List<string> Logs = new List<string>();
            public readonly List<string> Metrics = new List<string>();
            public readonly List<string> Distributions = new List<string>();
            public readonly List<string> UserIds = new List<string>();
            public readonly List<string> Tags = new List<string>();
            public readonly List<string> Contexts = new List<string>();
            public readonly List<int> BlockDurations = new List<int>();
            public readonly List<string> Exceptions = new List<string>();

            public void AddBreadcrumb(string name, string category, Dictionary<string, string> data)
            {
                Breadcrumbs.Add(name);
            }

            public void CaptureMessage(string message, Dictionary<string, string> tags, string contextName, object context)
            {
                Logs.Add(message);
            }

            public void SetContext(string name, object value)
            {
                Contexts.Add(name);
            }

            public void SetTag(string key, string value)
            {
                Tags.Add($"{key}={value}");
            }

            public void SetUser(string userId, string userName)
            {
                UserIds.Add(userId);
            }

            public void EmitCounter(string name, double value, Dictionary<string, string> tags)
            {
                Metrics.Add(name);
            }

            public void EmitGauge(string name, double value, Dictionary<string, string> tags)
            {
                Metrics.Add(name);
            }

            public void EmitDistribution(string name, double value, Dictionary<string, string> tags)
            {
                Metrics.Add(name);
                Distributions.Add(name);
            }

            public void RunDiagnosticCase(string id)
            {
                Logs.Add(id);
            }

            public void BlockMainThread(int milliseconds)
            {
                BlockDurations.Add(milliseconds);
            }

            public void CaptureException(System.Exception exception, Dictionary<string, string> tags, string contextName, object context)
            {
                Exceptions.Add(exception.Message);
            }
        }
    }
}
