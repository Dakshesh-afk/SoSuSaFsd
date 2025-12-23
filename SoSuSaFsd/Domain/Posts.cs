namespace SoSuSaFsd.Domain
{
    public class Posts : BaseDomainModel
    {
        public string? Content { get; set; }
        public string? PostStatus { get; set; }

        // Foreign Key to Users
        public int UserId { get; set; }
        // Foreign Key to Categories
        public int CategoryId { get; set; }

    }
}
