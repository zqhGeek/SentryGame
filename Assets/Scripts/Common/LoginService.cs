namespace SentryGame.Common
{
    public static class LoginService
    {
        public const string FixedUserName = "admin";
        public const string FixedPassword = "123456";

        public static bool Validate(string userName, string password)
        {
            return userName == FixedUserName && password == FixedPassword;
        }
    }
}
