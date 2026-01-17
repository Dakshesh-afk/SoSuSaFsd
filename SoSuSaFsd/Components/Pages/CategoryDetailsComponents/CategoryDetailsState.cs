using SoSuSaFsd.Domain;

namespace SoSuSaFsd.Components.Pages.CategoryDetailsComponents
{
    /// <summary>
    /// Manages all state for the CategoryDetails page
    /// </summary>
    public class CategoryDetailsState
    {
        // ========== DATA MODELS ==========
        public Categories? CurrentCategory { get; set; }
        public List<Posts> CategoryPosts { get; set; } = new();
        public List<Categories> RecentCategories { get; set; } = new();
        public List<Categories> FollowedCategories { get; set; } = new();
        public List<Categories> VerifiedCategories { get; set; } = new();

        // ========== UI STATE ==========
        public bool IsLoading { get; set; } = true;
        public string Title { get; set; } = "Category";
        public string ErrorMessage { get; set; } = "";
        public bool ShowLoginOverlay { get; set; } = false;

        // ========== USER STATE ==========
        public string? CurrentUserId { get; set; }
        public bool IsAdmin { get; set; } = false;
        public Users? CurrentUser { get; set; }

        // ========== CATEGORY STATE ==========
        public bool IsFollowing { get; set; } = false;
        public bool HasApprovedAccess { get; set; } = false;
        public bool HasPendingRequest { get; set; } = false;
        public CategoryAccessRequests? ExistingAccessRequest { get; set; }

        // ========== MODAL VISIBILITY ==========
        public bool ShowPostModal { get; set; } = false;
        public bool ShowRequestModal { get; set; } = false;
        public bool ShowReportModal { get; set; } = false;
        public bool ShowNotificationModal { get; set; } = false;
        public bool ShowCreateConfirm { get; set; } = false;

        // ========== FORM DATA ==========
        public string SearchTerm { get; set; } = "";
        public string PostSearchTerm { get; set; } = "";
        public string NewPostContent { get; set; } = "";
        public string RequestReason { get; set; } = "";
        public int ReportPostId { get; set; } = 0;
        public string ReportReasonSelection { get; set; } = "Spam";
        public string ReportReasonDetails { get; set; } = "";
        public string NotificationMessage { get; set; } = "";
        public string NotificationType { get; set; } = "success";

        // ========== COMMENTS & MEDIA ==========
        public Dictionary<int, bool> ShowComments { get; set; } = new();
        public Dictionary<int, List<Comments>> PostComments { get; set; } = new();
        public Dictionary<int, string> CommentDrafts { get; set; } = new();
        public Dictionary<int, int> CarouselIndices { get; set; } = new();
        public List<PendingFile> PendingUploads { get; set; } = new();
        public bool IsUploading { get; set; } = false;

        // ========== REPLY MANAGEMENT ==========
        public Dictionary<int, bool> ActiveReplyBoxes { get; set; } = new();
        public Dictionary<int, string> ReplyDrafts { get; set; } = new();

        // ========== HELPER METHODS ==========
        public void ShowNotification(string message, string type = "success")
        {
            NotificationMessage = message;
            NotificationType = type;
            ShowNotificationModal = true;
        }

        public void CloseNotification()
        {
            ShowNotificationModal = false;
            NotificationMessage = "";
        }

        public void ResetPageState()
        {
            IsLoading = true;
            ErrorMessage = "";
            HasApprovedAccess = false;
            HasPendingRequest = false;
        }

        public void ClosePostModal()
        {
            ShowPostModal = false;
            NewPostContent = "";
            PendingUploads.Clear();
            ErrorMessage = "";
        }
    }

    /// <summary>
    /// Helper class for pending file uploads
    /// </summary>
    public class PendingFile
    {
        public string SavedPath { get; set; } = null!;
        public string PhysicalPath { get; set; } = null!;
        public string MediaType { get; set; } = null!;
    }
}
