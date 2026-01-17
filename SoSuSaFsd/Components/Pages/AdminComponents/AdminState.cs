using SoSuSaFsd.Domain;

namespace SoSuSaFsd.Components.Pages.AdminComponents
{
    public class AdminState
    {
        // Tab & Filter State
        public string ActiveTab { get; set; } = "dashboard";
        public string ReportFilter { get; set; } = "recent_all";

        // Modal State
        public bool ShowContentModal { get; set; } = false;
        public bool ShowDeleteConfirmation { get; set; } = false;
        public int PendingDeleteReportId { get; set; } = 0;
        public int PendingDeleteCategoryId { get; set; } = 0;
        public Reports? SelectedReport { get; set; }
        public Dictionary<int, int> CarouselIndices { get; set; } = new();

        // Data
        public List<Users> AllUsers { get; set; } = new();
        public List<Categories> AllCategories { get; set; } = new();
        public List<CategoryAccessRequests> AccessRequests { get; set; } = new();
        public List<Reports> AllReports { get; set; } = new();

        // Computed Properties
        public int PendingNotificationCount =>
            AllReports?.GroupBy(r => new { r.PostID, r.CommentID, r.CategoryID, r.TargetUserID })
                .Where(g => !g.Any(r => r.Status == "Resolved" || r.Status == "Dismissed"))
                .Sum(g => g.Count(r => r.Status == "Pending")) ?? 0;

        // Methods
        public void CloseContentModal() => ShowContentModal = false;
        public void CloseDeleteModal()
        {
            ShowDeleteConfirmation = false;
            PendingDeleteReportId = 0;
            PendingDeleteCategoryId = 0;
        }
    }
}
