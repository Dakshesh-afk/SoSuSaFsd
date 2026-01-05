using System.ComponentModel.DataAnnotations.Schema;

namespace SoSuSaFsd.Domain
{
    public class Posts : BaseDomainModel
    {
        public string? Content { get; set; }

        // NEW: Property to store the image file path/URL
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }

        public string? PostStatus { get; set; }

        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public virtual Users? User { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Categories? Category { get; set; }
    }
}