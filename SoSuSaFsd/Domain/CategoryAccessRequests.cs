using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoSuSaFsd.Domain
{
    public class CategoryAccessRequests : BaseDomainModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public string? SupportingDocumentPath { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual Users? User { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual Categories? Category { get; set; }
    }
}

