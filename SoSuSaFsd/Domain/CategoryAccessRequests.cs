using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoSuSaFsd.Domain
{
    public class CategoryAccessRequests : BaseDomainModel
    {
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual Users? User { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Categories? Category { get; set; }

        public string? Reason { get; set; }
        
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    }
}

