using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoSuSaFsd.Domain
{
    public class Reports
    {
        [Key]
        public int ReportID { get; set; }
        [Required]
        public string Reason { get; set; } = "";
        public string Status { get; set; } = "Pending";
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public string ReporterID { get; set; } = "";
        [ForeignKey("ReporterID")]
        public Users? Reporter { get; set; }
        public int? PostID { get; set; }
        [ForeignKey("PostID")]
        public Posts? Post { get; set; }
        public int? CommentID { get; set; }
        [ForeignKey("CommentID")]
        public Comments? Comment { get; set; }
        public int? CategoryID { get; set; }
        [ForeignKey("CategoryID")]
        public Categories? Category { get; set; }
        public string? TargetUserID { get; set; }
        [ForeignKey("TargetUserID")]
        public Users? TargetUser { get; set; }
    }
}