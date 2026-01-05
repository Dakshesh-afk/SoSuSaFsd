using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoSuSaFsd.Domain
{
    // This new table will store individual media files for a post
    public class PostMedia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MediaPath { get; set; } = string.Empty; // e.g., "/uploads/abc.jpg"

        [Required]
        public string MediaType { get; set; } = string.Empty; // "Image" or "Video"

        // Foreign Key back to the Post
        public int PostId { get; set; }
        [ForeignKey("PostId")]
        public Posts? Post { get; set; }
    }
}