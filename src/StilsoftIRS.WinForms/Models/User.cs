namespace StilsoftIRS.Models
{
    internal sealed class User
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Login { get; set; }

        public string PasswordHash { get; set; }

        public string Role { get; set; }

        public bool IsActive { get; set; }

        public string FullName => (FirstName + " " + LastName).Trim();
    }
}
