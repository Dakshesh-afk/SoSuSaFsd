using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using SoSuSaFsd.Data;
using SoSuSaFsd.Domain;
using SoSuSaFsd.Services;
using SoSuSaFsd.Components.Common;
using SoSuSaFsd.Components.Common.PostCardComponents;
using SoSuSaFsd.Components.Pages.HomeComponents.SettingsPanelComponents;

namespace SoSuSaFsd.Components.Pages.HomeComponents
{
    public abstract class HomePageBase : ComponentBase
    {
        [Inject] protected IDbContextFactory<SoSuSaFsdContext> DbFactory { get; set; } = default!;
        [Inject] protected IServiceScopeFactory ScopeFactory { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
        [Inject] protected IPostService PostService { get; set; } = default!;
        [Inject] protected ICategoryService CategoryService { get; set; } = default!;
        [Inject] protected IHomePageService HomePageService { get; set; } = default!;

        [SupplyParameterFromQuery] public string? Error { get; set; }

        protected HomePageState State { get; set; } = new();
        protected ChangePasswordForm.PasswordChangeModel PasswordModel { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            if (!string.IsNullOrEmpty(Error))
            {
                State.ErrorMessage = Error;
                State.ShowLoginOverlay = true;
            }
            await LoadUserAndCategories();
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            await LoadUserAndCategories();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await JSRuntime.InvokeVoidAsync("window.addEventListener", "visibilitychange",
                        DotNetObjectReference.Create(this));
                }
                catch { }
            }

            await TrackSectionChanges();
            await base.OnAfterRenderAsync(firstRender);
        }

        protected async Task LoadUserAndCategories()
        {
            try
            {
                var authState = await AuthStateProvider.GetAuthenticationStateAsync();
                var userPrincipal = authState.User;

                if (userPrincipal.Identity?.IsAuthenticated == true)
                {
                    var userId = await GetUserIdAsync(userPrincipal);
                    
                    if (string.IsNullOrEmpty(userId))
                    {
                        ForceLogout();
                        return;
                    }

                    State.CurrentUser = await HomePageService.LoadCurrentUserAsync(userId);
                    
                    if (State.CurrentUser == null)
                    {
                        ForceLogout();
                        return;
                    }

                    await HomePageService.LoadUserDataAsync(State, userId);
                }
                else
                {
                    State.CurrentUser = null;
                    await HomePageService.LoadUserDataAsync(State, string.Empty);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        protected async Task RefreshFeed()
        {
            try
            {
                if (string.IsNullOrEmpty(State.CurrentUser?.Id)) return;

                State.FollowedCategories = await CategoryService.GetFollowedCategoriesAsync(State.CurrentUser.Id);
                
                var followedCategoryIds = State.FollowedCategories.Select(c => c.Id).ToList();
                State.FeedPosts = followedCategoryIds.Any()
                    ? await PostService.GetUserFeedPostsAsync(followedCategoryIds)
                    : new List<Posts>();

                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing feed: {ex.Message}");
            }
        }

        protected async Task HandleHeaderSearch()
        {
            var section = await GetCurrentSectionAsync();

            if (section == "profile")
                await HandleUserSearch();
            else if (section != "settings")
                await HandlePostSearch();
        }

        protected async Task HandlePostSearch()
        {
            if (string.IsNullOrWhiteSpace(State.HeaderSearchTerm))
            {
                State.PostSearchResults = new();
                State.HasSearchedPosts = false;
                StateHasChanged();
                return;
            }

            State.HasSearchedPosts = true;
            State.PostSearchResults = await HomePageService.SearchPostsAsync(State.HeaderSearchTerm);
            StateHasChanged();
        }

        protected async Task HandleUserSearch()
        {
            if (string.IsNullOrWhiteSpace(State.HeaderSearchTerm))
            {
                State.UserSearchResults = new();
                State.HasSearchedUsers = false;
                StateHasChanged();
                return;
            }

            State.HasSearchedUsers = true;
            State.UserSearchResults = await HomePageService.SearchUsersAsync(State.HeaderSearchTerm);
            StateHasChanged();
        }

        protected async Task HandleCategorySearch()
        {
            if (string.IsNullOrWhiteSpace(State.SearchTerm)) return;

            var cat = await CategoryService.GetCategoryByNameAsync(State.SearchTerm.Trim());

            if (cat != null)
                Navigation.NavigateTo($"/category/{cat.Id}", true);
            else
                State.ShowCreateConfirm = true;
        }

        protected async Task ToggleLike(int postId)
        {
            if (!EnsureAuthenticated()) return;

            try
            {
                await PostService.ToggleLikeAsync(postId, State.CurrentUser!.Id);
                await HomePageService.RefreshPostLikesAsync(State.FeedPosts, postId);
                await HomePageService.RefreshPostLikesAsync(State.UserPosts, postId);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling like: {ex.Message}");
            }
        }

        protected async Task ToggleComments(int postId)
        {
            if (!State.ShowComments.ContainsKey(postId))
                State.ShowComments[postId] = false;
                
            State.ShowComments[postId] = !State.ShowComments[postId];

            if (State.ShowComments[postId] && !State.PostComments.ContainsKey(postId))
            {
                try
                {
                    State.PostComments[postId] = await PostService.GetPostCommentsAsync(postId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading comments: " + ex.Message);
                }
            }
            StateHasChanged();
        }

        protected async Task SubmitComment(int postId)
        {
            if (!EnsureAuthenticated()) return;
            if (!State.CommentDrafts.ContainsKey(postId) || string.IsNullOrWhiteSpace(State.CommentDrafts[postId]))
                return;

            try
            {
                var newComment = CreateComment(postId, State.CommentDrafts[postId]);
                await PostService.CreateCommentAsync(newComment);

                State.CommentDrafts[postId] = "";
                newComment.User = State.CurrentUser;
                
                EnsureCommentListExists(postId);
                State.PostComments[postId].Add(newComment);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error posting comment: " + ex.Message);
            }
        }

        protected void ToggleReplyBox(int commentId)
        {
            if (!State.ActiveReplyBoxes.ContainsKey(commentId))
                State.ActiveReplyBoxes[commentId] = false;

            State.ActiveReplyBoxes[commentId] = !State.ActiveReplyBoxes[commentId];

            if (!State.ReplyDrafts.ContainsKey(commentId))
                State.ReplyDrafts[commentId] = "";

            StateHasChanged();
        }

        protected async Task SubmitReply(PostCardComments.SubmitReplyEventArgs args)
        {
            if (!EnsureAuthenticated()) return;
            if (!State.ReplyDrafts.ContainsKey(args.ParentCommentId) || 
                string.IsNullOrWhiteSpace(State.ReplyDrafts[args.ParentCommentId]))
                return;

            try
            {
                var newComment = CreateReply(args.PostId, args.ParentCommentId, State.ReplyDrafts[args.ParentCommentId]);
                await PostService.CreateCommentAsync(newComment);

                State.ReplyDrafts[args.ParentCommentId] = "";
                State.ActiveReplyBoxes[args.ParentCommentId] = false;
                newComment.User = State.CurrentUser;

                EnsureCommentListExists(args.PostId);
                State.PostComments[args.PostId].Add(newComment);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error posting reply: " + ex.Message);
            }
        }

        protected void OpenReportModal(int postId)
        {
            if (!EnsureAuthenticated()) return;

            State.ReportPostId = postId;
            State.ShowReportModal = true;
        }

        protected async Task SubmitReport((string Reason, string? Details) args)
        {
            if (State.ReportPostId == 0 || !EnsureAuthenticated())
            {
                State.ShowNotification("Session error. Please refresh and try again.", "error");
                return;
            }

            try
            {
                var submitted = await HomePageService.SubmitReportAsync(
                    State.ReportPostId,
                    State.CurrentUser!.Id,
                    args.Reason,
                    args.Details);

                State.ResetReportModal();
                State.ShowNotification(
                    submitted ? "Report submitted successfully." : "You have already reported this post.",
                    submitted ? "success" : "error");
            }
            catch (Exception ex)
            {
                State.ShowNotification("Error sending report: " + ex.Message, "error");
            }
        }

        protected async Task UpdateProfile()
        {
            if (State.CurrentUser == null) return;

            try
            {
                var success = await HomePageService.UpdateUserProfileAsync(
                    State.CurrentUser,
                    State.CurrentUser.DisplayName ?? "",
                    State.CurrentUser.Bio ?? "",
                    State.CurrentUser.ProfileImage);

                State.UpdateStatusMessage = success 
                    ? "Profile updated successfully!" 
                    : "Error updating profile.";

                if (success)
                {
                    UpdateUserInPosts();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                State.UpdateStatusMessage = "Error updating: " + ex.Message;
            }
        }

        protected async Task HandleProfileImageSelected(InputFileChangeEventArgs e)
        {
            if (!EnsureAuthenticated()) return;

            try
            {
                var file = e.File;
                var resizedFile = await file.RequestImageFileAsync(file.ContentType, 512, 512);

                var buffer = new byte[resizedFile.Size];
                await resizedFile.OpenReadStream(1024 * 1024 * 5).ReadAsync(buffer);
                var base64 = Convert.ToBase64String(buffer);

                State.CurrentUser!.ProfileImage = $"data:{file.ContentType};base64,{base64}";
                State.UpdateStatusMessage = "Photo selected. Click 'Save Changes' to apply.";
                StateHasChanged();
            }
            catch (Exception ex)
            {
                State.UpdateStatusMessage = "Error: " + ex.Message;
            }
        }

        protected async Task HandleVerificationDocumentSelected(IBrowserFile file)
        {
            if (!EnsureAuthenticated()) return;

            try
            {
                if (file.Size > 5 * 1024 * 1024)
                {
                    State.AccessRequestMessage = "File size must be less than 5MB";
                    State.IsAccessRequestSuccess = false;
                    return;
                }

                var uploadsPath = Path.Combine("wwwroot", "uploads", "verification");
                Directory.CreateDirectory(uploadsPath);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.Name)}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.OpenReadStream(5 * 1024 * 1024).CopyToAsync(stream);
                }

                State.VerificationDocumentPath = $"/uploads/verification/{fileName}";
            }
            catch (Exception ex)
            {
                State.AccessRequestMessage = "Error uploading file: " + ex.Message;
                State.IsAccessRequestSuccess = false;
            }
        }

        protected async Task UnfollowCategory(int categoryId)
        {
            if (string.IsNullOrEmpty(State.CurrentUser?.Id)) return;

            try
            {
                await CategoryService.ToggleFollowAsync(State.CurrentUser.Id, categoryId);
                await RefreshFeed();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected async Task HandleAccessRequest()
        {
            if (!EnsureAuthenticated()) return;

            if (State.SelectedCategoryId == 0)
            {
                State.AccessRequestMessage = "Please select a category.";
                State.IsAccessRequestSuccess = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(State.AccessRequestReason))
            {
                State.AccessRequestMessage = "Please provide a reason for the request.";
                State.IsAccessRequestSuccess = false;
                return;
            }

            try
            {
                using var context = DbFactory.CreateDbContext();
                
                var existingRequest = await context.CategoryAccessRequests
                    .FirstOrDefaultAsync(r => r.UserId == State.CurrentUser!.Id && 
                        r.CategoryId == State.SelectedCategoryId);

                if (existingRequest != null)
                {
                    if (existingRequest.Status == "Pending")
                    {
                        State.AccessRequestMessage = "You already have a pending request for this category.";
                        State.IsAccessRequestSuccess = false;
                        return;
                    }
                    else if (existingRequest.Status == "Approved")
                    {
                        State.AccessRequestMessage = "You already have access to this category.";
                        State.IsAccessRequestSuccess = false;
                        return;
                    }
                    else if (existingRequest.Status == "Rejected")
                    {
                        var canResubmitDate = existingRequest.DateUpdated.AddDays(7);
                        var daysRemaining = (canResubmitDate - DateTime.Now).TotalDays;
                        
                        if (daysRemaining > 0)
                        {
                            State.AccessRequestMessage = $"Your previous request was rejected on {existingRequest.DateUpdated:MMM dd, yyyy 'at' h:mm tt}. You can resubmit on {canResubmitDate:MMM dd, yyyy 'at' h:mm tt} ({Math.Ceiling(daysRemaining)} day(s) remaining).";
                            State.IsAccessRequestSuccess = false;
                            return;
                        }
                        
                        existingRequest.Reason = State.AccessRequestReason;
                        existingRequest.Status = "Pending";
                        existingRequest.DateUpdated = DateTime.Now;
                        existingRequest.UpdatedBy = null;
                        existingRequest.SupportingDocumentPath = State.VerificationDocumentPath;
                        
                        await context.SaveChangesAsync();
                        
                        State.AccessRequestMessage = "Request resubmitted successfully!";
                        State.IsAccessRequestSuccess = true;
                        State.SelectedCategoryId = 0;
                        State.AccessRequestReason = "";
                        State.VerificationDocumentPath = null;
                        StateHasChanged();

                        await LoadUserAndCategories();
                        return;
                    }
                }

                var newRequest = new CategoryAccessRequests
                {
                    UserId = State.CurrentUser!.Id,
                    CategoryId = State.SelectedCategoryId,
                    Reason = State.AccessRequestReason,
                    Status = "Pending",
                    SupportingDocumentPath = State.VerificationDocumentPath,
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    CreatedBy = State.CurrentUser.Id
                };

                context.CategoryAccessRequests.Add(newRequest);
                await context.SaveChangesAsync();

                State.AccessRequestMessage = "Request submitted successfully!";
                State.IsAccessRequestSuccess = true;
                State.SelectedCategoryId = 0;
                State.AccessRequestReason = "";
                State.VerificationDocumentPath = null;
                StateHasChanged();

                await LoadUserAndCategories();
            }
            catch (Exception ex)
            {
                State.AccessRequestMessage = "Error submitting request: " + ex.Message;
                State.IsAccessRequestSuccess = false;
            }
        }

        protected void OnSearchPostClicked((int CategoryId, int PostId) data)
        {
            State.ClearSearchState();
            Navigation.NavigateTo($"/category/{data.CategoryId}?focusPostId={data.PostId}", true);
        }

        protected void OnSearchUserClicked(string? username)
        {
            if (string.IsNullOrEmpty(username)) return;
            State.ClearSearchState();
            Navigation.NavigateTo($"/profile/{username}", true);
        }

        protected void NavigateToCategory(int id) => Navigation.NavigateTo($"/category/{id}");
        protected void NavigateToAdmin() => Navigation.NavigateTo("admin", forceLoad: true);
        protected void NavigateToCreate() => Navigation.NavigateTo($"/categories/create?name={State.SearchTerm}");
        protected void RefreshPage() => Navigation.NavigateTo(Navigation.Uri, forceLoad: true);
        protected void ForceLogout() => Navigation.NavigateTo("/api/Account/logout", true);

        protected void ShowLoginOverlay()
        {
            State.ShowLoginOverlay = true;
            StateHasChanged();
        }

        protected void UpdateCommentDraft(PostCardComments.CommentDraftEventArgs args)
        {
            if (args.Content != null)
                State.CommentDrafts[args.PostId] = args.Content;
        }

        protected void UpdateReplyDraft(PostCardComments.ReplyDraftEventArgs args)
        {
            if (args.Content != null)
                State.ReplyDrafts[args.ParentCommentId] = args.Content;
        }

        protected void UpdateCarouselIndex(PostCardMedia.CarouselEventArgs args)
        {
            State.CarouselIndices[args.PostId] = args.Index;
            StateHasChanged();
        }

        protected async Task HandlePasswordChange()
        {
            if (!EnsureAuthenticated()) return;

            try
            {
                using var scope = ScopeFactory.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();

                var user = await userManager.FindByIdAsync(State.CurrentUser!.Id);
                if (user == null)
                {
                    State.PasswordMessage = "User not found.";
                    State.IsPasswordSuccess = false;
                    return;
                }

                var isCorrectPassword = await userManager.CheckPasswordAsync(user, PasswordModel.OldPassword);
                if (!isCorrectPassword)
                {
                    State.PasswordMessage = "Current password is incorrect.";
                    State.IsPasswordSuccess = false;
                    return;
                }

                var result = await userManager.ChangePasswordAsync(user, PasswordModel.OldPassword, PasswordModel.NewPassword);

                if (result.Succeeded)
                {
                    State.PasswordMessage = "Password changed successfully!";
                    State.IsPasswordSuccess = true;
                    
                    PasswordModel = new ChangePasswordForm.PasswordChangeModel();
                    StateHasChanged();
                }
                else
                {
                    State.PasswordMessage = "Error: " + string.Join(", ", result.Errors.Select(e => e.Description));
                    State.IsPasswordSuccess = false;
                }
            }
            catch (Exception ex)
            {
                State.PasswordMessage = "Error changing password: " + ex.Message;
                State.IsPasswordSuccess = false;
            }
        }

        private async Task<string?> GetUserIdAsync(System.Security.Claims.ClaimsPrincipal userPrincipal)
        {
            var userId = userPrincipal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                using var scope = ScopeFactory.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
                var dbUser = await userManager.GetUserAsync(userPrincipal);
                return dbUser?.Id;
            }

            return userId;
        }

        private async Task<string> GetCurrentSectionAsync()
        {
            try
            {
                return await JSRuntime.InvokeAsync<string>("eval", "window.currentSection || 'home'");
            }
            catch
            {
                return State.CurrentActiveSection;
            }
        }

        private async Task TrackSectionChanges()
        {
            try
            {
                var newSection = await GetCurrentSectionAsync();
                if (newSection != State.CurrentActiveSection)
                {
                    State.CurrentActiveSection = newSection;
                    State.ClearSearchState();
                    StateHasChanged();
                }
            }
            catch { }
        }

        private bool EnsureAuthenticated()
        {
            if (State.CurrentUser == null)
            {
                State.ShowLoginOverlay = true;
                return false;
            }
            return true;
        }

        private Comments CreateComment(int postId, string content)
        {
            return new Comments
            {
                PostID = postId,
                UserID = State.CurrentUser!.Id,
                Content = content,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                CreatedBy = State.CurrentUser.Id
            };
        }

        private Comments CreateReply(int postId, int parentCommentId, string content)
        {
            return new Comments
            {
                PostID = postId,
                UserID = State.CurrentUser!.Id,
                Content = content,
                ParentCommentID = parentCommentId,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                CreatedBy = State.CurrentUser.Id
            };
        }

        private void EnsureCommentListExists(int postId)
        {
            if (!State.PostComments.ContainsKey(postId))
                State.PostComments[postId] = new List<Comments>();
        }

        private void UpdateUserInPosts()
        {
            foreach (var post in State.UserPosts.Concat(State.FeedPosts))
            {
                if (post.UserId == State.CurrentUser!.Id && post.User != null)
                {
                    post.User.ProfileImage = State.CurrentUser.ProfileImage;
                    post.User.DisplayName = State.CurrentUser.DisplayName;
                }
            }
        }
    }
}
