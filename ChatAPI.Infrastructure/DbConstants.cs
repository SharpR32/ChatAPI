namespace ChatAPI.Infrastructure;

/// <summary>
/// Contains constants used in db queries
/// </summary>
/// <remarks>
/// Constants are used for string precompilation
/// </remarks>
public static class DbConstants
{
    public const string USER_TABLE = "public.users";
    public const string LOGIN_TABLE = "public.login_log";

    public static class User
    {
        public const string Id = "id";
        public const string UserName = "username";
        public const string PasswordHash = "password";
        public const string LoginCount = "login_count";
    }

    public static class LoginLog
    {
        public const string UserId = "user_id";
        public const string IP = "ip";
    }
}
