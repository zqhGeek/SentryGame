using System;
using System.Collections.Generic;

namespace SentryGame.SentryTesting
{
    public interface ISentryTelemetrySink
    {
        void AddBreadcrumb(string name, string category, Dictionary<string, string> data);

        void CaptureMessage(string message, Dictionary<string, string> tags, string contextName, object context);

        void CaptureException(Exception exception, Dictionary<string, string> tags, string contextName, object context);

        void SetContext(string name, object value);

        void SetTag(string key, string value);

        void SetUser(string userId, string userName);

        void EmitCounter(string name, double value, Dictionary<string, string> tags);

        void EmitGauge(string name, double value, Dictionary<string, string> tags);

        void EmitDistribution(string name, double value, Dictionary<string, string> tags);

        void RunCpuStallTrace(int milliseconds, string transactionName, string spanName, string spanDescription, Dictionary<string, string> tags, string contextName, object context);

        void RunDiagnosticCase(string id);
    }
}
