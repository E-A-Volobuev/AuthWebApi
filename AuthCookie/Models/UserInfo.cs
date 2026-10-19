namespace AuthCookie.Models
{
    /// <summary>
    /// Простая "пользовательская база" для демо: username/password/роль/отдел.
    /// В реальном приложении пользователи хранились бы в БД.
    /// </summary>
    public record UserInfo(string UserName, string Password, string Role, string? Department = null);

    public static class Users
    {
        public static IReadOnlyList<UserInfo> All { get; } = new List<UserInfo>
        {
            new UserInfo("admin", "admin123", "Admin", "it"),
            new UserInfo("user", "user123", "User", null),
            new UserInfo("fin", "fin123", "User", "finance")
        };

        public static UserInfo? Find(string userName)
            => All.FirstOrDefault(u => u.UserName == userName);
    }
}
