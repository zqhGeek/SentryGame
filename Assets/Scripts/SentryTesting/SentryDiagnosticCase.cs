namespace SentryGame.SentryTesting
{
    public readonly struct SentryDiagnosticCase
    {
        public SentryDiagnosticCase(string id, string title)
        {
            Id = id;
            Title = title;
        }

        public string Id { get; }

        public string Title { get; }
    }
}
