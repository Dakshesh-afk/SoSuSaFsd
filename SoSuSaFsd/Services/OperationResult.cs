namespace SoSuSaFsd.Services
{
    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        // Optional payloads
        public bool? Liked { get; set; }
        public int? LikeCount { get; set; }
        public bool? IsFollowing { get; set; }
    }
}
