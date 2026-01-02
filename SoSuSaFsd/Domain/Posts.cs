namespace SoSuSaFsd.Domain
{
    public class Posts : BaseDomainModel
    {
        public string? Content { get; set; }
        public string? PostStatus { get; set; }
        public string UserId { get; set; }
        public int CategoryId { get; set; }

    }
}
