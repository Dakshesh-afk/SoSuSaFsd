using System.ComponentModel.DataAnnotations.Schema;

namespace SoSuSaFsd.Domain
{
    public class Posts : BaseDomainModel
    {
        public string? Content { get; set; }
        public List<PostMedia> Media { get; set; } = new();

        public string? PostStatus { get; set; }

        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public virtual Users? User { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Categories? Category { get; set; }
        public virtual ICollection<PostLikes> Likes { get; set; } = new List<PostLikes>();
    }
}