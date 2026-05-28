using Sentry.Unity;

namespace SentryGame.SentryTesting
{
    /// <summary>
    /// Drops all events emitted from the Sentry diagnostics scene so that intentional
    /// test exceptions do not pollute the production issue tracker.
    ///
    /// Register this asset in the Sentry editor window under Options Config → Sentry
    /// Options Configuration.
    /// </summary>
    public class SentryDiagnosticsFilter : ScriptableOptionsConfiguration
    {
        public override void Configure(SentryUnityOptions options)
        {
            options.SetBeforeSend((sentryEvent, _) =>
            {
                if (sentryEvent.Tags.TryGetValue(SentryFeatureNames.Tags.GameMode, out var gameMode)
                    && gameMode == SentryFeatureNames.GameModes.SentryDiagnostics)
                {
                    // Intentional diagnostic test event – drop it so it doesn't create
                    // noise in the production Sentry issue tracker.
                    return null;
                }

                return sentryEvent;
            });
        }
    }
}