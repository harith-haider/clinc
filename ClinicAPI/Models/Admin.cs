namespace ClinicAPI.Models
{
    public class Admin
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "مدير النظام";
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Admin";
    }
}