using SoSuSaFsd.Domain;

namespace SoSuSaFsd.Components.Pages.HomeComponents
{
    /// <summary>
    /// Manages all state for the Home page
    /// Separated from Home.razor to reduce complexity and improve testability
    /// </summary>
    public class HomePageState
    {
        // ========== SEARCH STATE ==========
        public string SearchTerm { get; set; } = "";
        public string HeaderSearchTerm { get; set; } = "";
        public bool HasSearchedPosts { get; set; } = false;
        public bool HasSearchedUsers { get; set; } = false;
        public string CurrentActiveSection { get; set; } = "home";

        // ========== DATA COLLECTIONS ==========
        public List<Categories> RecentCategories { get; set; } = new();
        public List<Categories> VerifiedCategories { get; set; } = new();
        public List<Categories> VerifiedCategoriesForRequest { get; set; } = new();
        public List<Categories> FollowedCategories { get; set; } = new();
        public List<Posts> FeedPosts { get; set; } = new();
        public List<Posts> UserPosts { get; set; } = new();
        public List<CategoryAccessRequests> UserAccessRequests { get; set; } = new();
        public List<Posts> PostSearchResults { get; set; } = new();
        public List<Users> UserSearchResults { get; set; } = new();

        // ========== USER INFO ==========
        public Users? CurrentUser { get; set; }
        public int SelectedCategoryId { get; set; } = 0;
        public string AccessRequestReason { get; set; } = "";
        public string AccessRequestMessage { get; set; } = "";
        public bool IsAccessRequestSuccess { get; set; } = false;

        // ========== COMMENTS & INTERACTIONS ==========
        public Dictionary<int, bool> ShowComments { get; set; } = new();
        public Dictionary<int, List<Comments>> PostComments { get; set; } = new();
        public Dictionary<int, string> CommentDrafts { get; set; } = new();
        public Dictionary<int, int> CarouselIndices { get; set; } = new();
        public Dictionary<int, bool> ActiveReplyBoxes { get; set; } = new();
        public Dictionary<int, string> ReplyDrafts { get; set; } = new();

        // ========== MODALS ==========
        public bool ShowReportModal { get; set; } = false;
        public int ReportPostId { get; set; } = 0;
        public string ReportReasonSelection { get; set; } = "Spam";
        public string ReportReasonDetails { get; set; } = "";
        public bool ShowNotificationModal { get; set; } = false;
        public string NotificationMessage { get; set; } = "";
        public string NotificationType { get; set; } = "success";
        public bool ShowCreateConfirm { get; set; } = false;
        public bool ShowLoginOverlay { get; set; } = false;
        public string ErrorMessage { get; set; } = "";

        // ========== SETTINGS ==========
        public string UpdateStatusMessage { get; set; } = "";
        public string PasswordMessage { get; set; } = "";
        public bool IsPasswordSuccess { get; set; } = false;

        // ========== DEBUG ==========
        public bool DebugRenderPosts { get; set; } = false;

        /// <summary>
        /// Clears search-related state
        /// </summary>
        public void ClearSearchState()
        {
            HeaderSearchTerm = "";
            PostSearchResults = new List<Posts>();
            UserSearchResults = new List<Users>();
            HasSearchedPosts = false;
            HasSearchedUsers = false;
        }

        /// <summary>
        /// Resets modal state
        /// </summary>
        public void ResetReportModal()
        {
            ShowReportModal = false;
            ReportPostId = 0;
            ReportReasonSelection = "Spam";
            ReportReasonDetails = "";
        }

        /// <summary>
        /// Shows a notification message
        /// </summary>
        public void ShowNotification(string message, string type = "success")
        {
            NotificationMessage = message;
            NotificationType = type;
            ShowNotificationModal = true;
        }

        /// <summary>
        /// Closes the notification modal
        /// </summary>
        public void CloseNotification()
        {
            ShowNotificationModal = false;
            NotificationMessage = "";
        }
    }
}
