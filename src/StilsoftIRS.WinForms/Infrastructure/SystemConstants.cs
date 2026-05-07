namespace StilsoftIRS.Infrastructure
{
    internal static class SystemConstants
    {
        public const string AdministratorRole = "Администратор";
        public const string OperatorRole = "Оператор";
        public const string AnalystRole = "Аналитик";

        public const string NewStatus = "Новый";
        public const string InProgressStatus = "В работе";
        public const string EscalatedStatus = "Эскалирован";
        public const string ResolvedStatus = "Решён";
        public const string ClosedStatus = "Закрыт";

        public const string CriticalPriority = "Критический";
        public const string HighPriority = "Высокий";
        public const string MediumPriority = "Средний";
        public const string LowPriority = "Низкий";

        public static readonly string[] Roles =
        {
            AdministratorRole,
            OperatorRole,
            AnalystRole
        };

        public static readonly string[] Priorities =
        {
            CriticalPriority,
            HighPriority,
            MediumPriority,
            LowPriority
        };

        public static readonly string[] Statuses =
        {
            NewStatus,
            InProgressStatus,
            EscalatedStatus,
            ResolvedStatus,
            ClosedStatus
        };
    }
}
