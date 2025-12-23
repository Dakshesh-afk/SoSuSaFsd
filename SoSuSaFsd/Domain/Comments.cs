namespace SoSuSaFsd.Domain
{
    public class Comments : BaseDomainModel
    {
        // Content of the comment
        public string? Content { get; set; }

        // Foreign Key to Posts
        public int PostID { get; set; }

        // Foreign Key to Users
        public int UserID { get; set; }

        // Foreign Key to parent Comments for nested comments
        public int? ParentCommentID { get; set; }
    }
}
