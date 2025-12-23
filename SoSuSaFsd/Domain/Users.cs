namespace SoSuSaFsd.Domain
{
    public class Users : BaseDomainModel
    {
        public string? Username { get; set; } 
        public string? Email { get; set; } 
        public string? PasswordHash { get; set; }
        public string? Role { get; set; }
        public string? Bio { get; set; }
        public string? ProfileImage { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? DisplayName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
