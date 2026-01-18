using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoSuSaFsd.Domain
{
    public class Comments : BaseDomainModel
    {

        [Required(ErrorMessage = "Comment content is required")]
        [MinLength(1, ErrorMessage = "Comment cannot be empty")]
        public string Content { get; set; }
        public int PostID { get; set; }

        //Navigation properties 
        public string UserID { get; set; } = string.Empty;
        [ForeignKey("UserID")]
        public virtual Users? User { get; set; }
        public int? ParentCommentID { get; set; }
    }
}
