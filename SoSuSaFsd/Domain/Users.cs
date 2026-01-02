using Microsoft.AspNetCore.Identity;
using System;

namespace SoSuSaFsd.Domain
{
    public class Users : IdentityUser
    {
        public string? Bio { get; set; }
        public string? ProfileImage { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? DisplayName { get; set; }
        public string? Role { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; }
    }
}