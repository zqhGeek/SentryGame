namespace SentryGame.SentryTesting
{
    public static class SentryFeatureNames
    {
        public static class Tags
        {
            public const string Scene = "scene";
            public const string GameMode = "game_mode";
            public const string TestCase = "test_case";
            public const string FailureReason = "failure_reason";
        }

        public static class Contexts
        {
            public const string Login = "login";
            public const string WhackAMole = "whack_a_mole";
            public const string Snake = "snake";
            public const string Diagnostics = "diagnostics";
        }

        public static class Metrics
        {
            public const string LoginFailure = "login.failure";
            public const string MoleHit = "mole.hit";
            public const string MoleMissed = "mole.missed";
            public const string WhackAMoleEntered = "whack_a_mole.enter";
            public const string WhackAMoleExited = "whack_a_mole.exit";
            public const string WhackAMoleRandomStall = "whack_a_mole.random_stall";
            public const string WhackAMoleStallDuration = "whack_a_mole.stall_duration";
            public const string WhackAMoleRandomCrash = "whack_a_mole.random_crash";
            public const string SnakeFoodEaten = "snake.food_eaten";
            public const string SnakeRandomStall = "snake.random_stall";
            public const string SnakeStallDuration = "snake.stall_duration";
            public const string SnakeRandomCrash = "snake.random_crash";
            public const string GameScore = "game.score";
            public const string DiagnosticsButtonClick = "diagnostics.button_click";
        }

        public static class Transactions
        {
            public const string ManualDiagnostics = "diagnostics.manual_transaction";
            public const string WhackAMoleRandomStall = "whack_a_mole.random_stall";
            public const string SnakeRandomStall = "snake.random_stall";
        }

        public static class Spans
        {
            public const string WhackAMoleRewardAppeared = "whack_a_mole.reward_appeared";
            public const string SnakeRewardAppeared = "snake.reward_appeared";
        }

        public static class Diagnostics
        {
            public const string CaptureMessage = "capture_message";
            public const string CaptureException = "capture_exception";
            public const string ThrowUnhandledException = "throw_unhandled_exception";
            public const string LogError = "debug_log_error";
            public const string DuplicateErrors = "duplicate_errors";
            public const string FailedRequest = "failed_request";
            public const string ManualTransaction = "manual_transaction";
            public const string Metrics = "metrics";
            public const string StructuredLog = "structured_log";
            public const string Screenshot = "screenshot";
            public const string ViewHierarchy = "view_hierarchy";
            public const string OfflineCache = "offline_cache";
            public const string BlockMainThread = "block_main_thread";
            public const string NativeCrash = "native_crash";
            public const string FilteredEvent = "filtered_event";
        }
    }
}
