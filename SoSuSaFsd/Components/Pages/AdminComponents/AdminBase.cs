using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SoSuSaFsd.Services;
using SoSuSaFsd.Domain;

namespace SoSuSaFsd.Components.Pages.AdminComponents
{
    [Authorize(Roles = "Admin")]
    public class AdminBase : ComponentBase
    {
        [Inject] protected IAdminService AdminService { get; set; } = default!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

        protected AdminState State { get; set; } = new();

        protected override async Task OnInitializedAsync() => await LoadData();

        protected async Task LoadData()
        {
            State.AllUsers = await AdminService.GetAllUsersAsync();
            State.AllCategories = await AdminService.GetAllCategoriesAsync();
            State.AccessRequests = await AdminService.GetAccessRequestsAsync();
            State.AllReports = await AdminService.GetAllReportsAsync();
        }

        protected async Task DismissReportGroup(int reportId)
        {
            await AdminService.DismissReportGroupAsync(reportId);
            await LoadData();
        }

        protected async Task UndoDismiss(int reportId)
        {
            await AdminService.UndoDismissAsync(reportId);
            await LoadData();
        }

        protected void RequestDelete(int reportId)
        {
            State.PendingDeleteReportId = reportId;
            State.PendingDeleteCategoryId = 0;
            State.ShowDeleteConfirmation = true;
        }

        protected void RequestDeleteCategory(int categoryId)
        {
            State.PendingDeleteCategoryId = categoryId;
            State.PendingDeleteReportId = 0;
            State.ShowDeleteConfirmation = true;
        }

        protected void CancelDelete() => State.CloseDeleteModal();

        protected async Task ConfirmDelete()
        {
            if (State.PendingDeleteReportId != 0)
                await AdminService.DeleteReportedContentAsync(State.PendingDeleteReportId);
            else if (State.PendingDeleteCategoryId != 0)
                await AdminService.DeleteCategoryAsync(State.PendingDeleteCategoryId);

            State.CloseDeleteModal();
            await LoadData();
        }

        protected async Task ToggleVerification(int categoryId)
        {
            try
            {
                await AdminService.ToggleCategoryVerificationAsync(categoryId);
                await LoadData();
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        }

        protected async Task ApproveRequest(int requestId)
        {
            try
            {
                await AdminService.ApproveAccessRequestAsync(requestId);
                await LoadData();
            }
            catch { }
        }

        protected async Task RejectRequest(int requestId)
        {
            try
            {
                await AdminService.RejectAccessRequestAsync(requestId);
                await LoadData();
            }
            catch { }
        }

        protected async Task ToggleUserBan(string userId)
        {
            try
            {
                await AdminService.ToggleUserBanAsync(userId);
                await LoadData();
            }
            catch { }
        }

        protected void ExitAdmin() => NavigationManager.NavigateTo("/", forceLoad: true);

        protected void OpenContentModal(int reportId)
        {
            State.SelectedReport = State.AllReports.FirstOrDefault(r => r.ReportID == reportId);
            if (State.SelectedReport != null) State.ShowContentModal = true;
        }

        protected int GetCurrentIndex(int postId)
        {
            if (!State.CarouselIndices.ContainsKey(postId)) State.CarouselIndices[postId] = 0;
            return State.CarouselIndices[postId];
        }

        protected void NextSlide(int postId, int totalCount)
        {
            var currentIndex = GetCurrentIndex(postId);
            State.CarouselIndices[postId] = (currentIndex + 1) % totalCount;
        }

        protected void PrevSlide(int postId, int totalCount)
        {
            var currentIndex = GetCurrentIndex(postId);
            State.CarouselIndices[postId] = (currentIndex - 1 + totalCount) % totalCount;
        }

        protected string GetStatusBadgeClass(string status) =>
            status switch
            {
                "Resolved" => "badge-resolved",
                "Dismissed" => "badge-dismissed",
                _ => "badge-pending"
            };
    }
}
