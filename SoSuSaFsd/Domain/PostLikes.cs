using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoSuSaFsd.Domain
{
    public class PostLikes
    {
        [Key]
        public int Id { get; set; }
        public DateTime LikedAt { get; set; } = DateTime.Now;

        // Foreign Key
        public int PostID { get; set; }

        // --- ADD THIS NAVIGATION PROPERTY ---
        [ForeignKey("PostID")]
        public virtual Posts? Post { get; set; }
        // ------------------------------------

        public string UserID { get; set; } = string.Empty;
    }
}