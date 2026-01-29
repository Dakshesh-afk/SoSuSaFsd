using SoSuSaFsd.Domain;

namespace SoSuSaFsd.Components.Pages.ProfileComponents
{
    /// <summary>
    /// State container for Profile page
    /// Consolidates all profile-related state in one place
    /// </summary>
    public class ProfileState
    {
        // ========== USER DATA ==========
        /// <summary>
        /// The profile being viewed
        /// </summary>
        public Users? ProfileUser { get; set; }

        /// <summary>
        /// Currently logged-in user
        /// </summary>
        public Users? CurrentUser { get; set; }

        /// <summary>
        /// Posts created by the profile user
        /// </summary>
        public List<Posts> UserPosts { get; set; } = new();

        // ========== STATS ==========
        /// <summary>
        /// Number of followers the profile user has
        /// </summary>
        public int FollowerCount { get; set; } = 0;

        // ========== UI STATE ==========
        /// <summary>
        /// Whether the page is loading
        /// </summary>
        public bool IsLoading { get; set; } = true;

        /// <summary>
        /// Error message to display
        /// </summary>
        public string ErrorMessage { get; set; } = "";

        /// <summary>
        /// Page title for browser
        /// </summary>
        public string PageTitle { get; set; } = "User Profile";

        /// <summary>
        /// Report status message
        /// </summary>
        public string ReportStatusMessage { get; set; } = "";

        // ========== MODAL STATE ==========
        /// <summary>
        /// Whether the report modal is visible
        /// </summary>
        public bool ShowReportModal { get; set; } = false;

        /// <summary>
        /// Whether the notification modal is visible
        /// </summary>
        public bool ShowNotificationModal { get; set; } = false;

        /// <summary>
        /// Notification message to display
        /// </summary>
        public string NotificationMessage { get; set; } = "";

        /// <summary>
        /// Type of notification: "success", "error", "info"
        /// </summary>
        public string NotificationType { get; set; } = "success";

        /// <summary>
        /// ID of the post being reported
        /// </summary>
        public int ReportPostId { get; set; } = 0;

        // ========== POST INTERACTION STATE ==========
        /// <summary>
        /// Tracks which posts have comments visible
        /// </summary>
        public Dictionary<int, bool> ShowComments { get; set; } = new();

        /// <summary>
        /// Stores comments for each post
        /// </summary>
        public Dictionary<int, List<Comments>> PostComments { get; set; } = new();

        /// <summary>
        /// Comment drafts being typed
        /// </summary>
        public Dictionary<int, string> CommentDrafts { get; set; } = new();

        /// <summary>
        /// Current carousel index for each post
        /// </summary>
        public Dictionary<int, int> CarouselIndices { get; set; } = new();

        /// <summary>
        /// Tracks which reply boxes are open
        /// </summary>
        public Dictionary<int, bool> ActiveReplyBoxes { get; set; } = new();

        /// <summary>
        /// Reply drafts being typed
        /// </summary>
        public Dictionary<int, string> ReplyDrafts { get; set; } = new();

        // ========== METHODS ==========
        /// <summary>
        /// Resets all state to initial values
        /// </summary>
        public void Reset()
        {
            ProfileUser = null;
            UserPosts = new();
            IsLoading = true;
            ErrorMessage = "";
            FollowerCount = 0;
            ReportStatusMessage = "";
            ShowReportModal = false;
            ShowNotificationModal = false;
            NotificationMessage = "";
            NotificationType = "success";
            ReportPostId = 0;
            ShowComments.Clear();
            PostComments.Clear();
            CommentDrafts.Clear();
            CarouselIndices.Clear();
            ActiveReplyBoxes.Clear();
            ReplyDrafts.Clear();
        }

        /// <summary>
        /// Shows a notification modal
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

        /// <summary>
        /// Whether the report button should be disabled
        /// </summary>
        public bool IsReportDisabled => ProfileUser == null || CurrentUser == null;
    }
}
