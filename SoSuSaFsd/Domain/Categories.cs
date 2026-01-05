using System;
using System.ComponentModel.DataAnnotations;

namespace SoSuSaFsd.Domain
{
    public class Categories : BaseDomainModel
    {
        public string? CategoryName { get; set; }
        public string? CategoryDescription { get; set; }
        public bool CategoryIsRestricted { get; set; }
        public bool IsVerified { get; set; } = false;
        public string? CreatedBy { get; set; }
    }
}