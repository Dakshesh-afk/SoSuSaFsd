using System.ComponentModel.DataAnnotations.Schema;

namespace SoSuSaFsd.Domain
{
    public class Posts : BaseDomainModel
    {
        public string? Content { get; set; }
        public string? PostStatus { get; set; } // e.g., "Published"

        // --- Relationships ---
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual Users? User { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Categories? Category { get; set; }
    }
}