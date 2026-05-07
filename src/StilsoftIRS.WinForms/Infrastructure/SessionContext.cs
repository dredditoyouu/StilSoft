using StilsoftIRS.Models;

namespace StilsoftIRS.Infrastructure
{
    internal static class SessionContext
    {
        public static User CurrentUser { get; private set; }

        public static bool IsAuthenticated => CurrentUser != null;

        public static void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        public static void Clear()
        {
            CurrentUser = null;
        }
    }
}
