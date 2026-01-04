using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoSuSaFsd.Domain
{
    public class CategoryFollows : BaseDomainModel
    {
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual Users? User { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Categories? Category { get; set; }
    }
}