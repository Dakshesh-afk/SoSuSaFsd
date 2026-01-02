namespace SoSuSaFsd.Domain
{
    public class Comments : BaseDomainModel
    {
        public string? Content { get; set; }
        public int PostID { get; set; }
        public string UserID { get; set; }
        public int? ParentCommentID { get; set; }
    }
}
