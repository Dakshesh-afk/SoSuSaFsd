using System.ComponentModel.DataAnnotations.Schema;

namespace SoSuSaFsd.Domain
{
    public class Comments : BaseDomainModel
    {
        public string? Content { get; set; }
        public int PostID { get; set; }

        //Navigation properties 
        public string UserID { get; set; } = string.Empty;
        [ForeignKey("UserID")]
        public virtual Users? User { get; set; }
        public int? ParentCommentID { get; set; }
    }
}
