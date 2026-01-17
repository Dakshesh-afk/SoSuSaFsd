using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoSuSaFsd.Data;
using SoSuSaFsd.Domain;
using SoSuSaFsd.Services;
using System.IO;

namespace SoSuSaFsd.Components.Pages.CategoryDetailsComponents
{
    public abstract class CategoryDetailsBase : ComponentBase
    {
        // ========== INJECTED DEPENDENCIES ==========
        [Inject] protected IDbContextFactory<SoSuSaFsdContext> DbFactory { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
        [Inject] protected IServiceScopeFactory ScopeFactory { get; set; } = default!;
        [Inject] protected IWebHostEnvironment Environment { get; set; } = default!;
        [Inject] protected IPostService PostService { get; set; } = default!;
        [Inject] protected ICategoryService CategoryService { get; set; } = default!;
        [Inject] protected ICategoryDetailsService CategoryDetailsService { get; set; } = default!;

        // ========== PARAMETERS ==========
        [Parameter] public int Id { get; set; }
        [SupplyParameterFromQuery] public int? FocusPostId { get; set; }

        // ========== STATE ==========
        protected CategoryDetailsState State { get; set; } = new();

        // ========== CONSTANTS ==========
        protected const long MaxFileSize = 1024 * 1024 * 200;
        protected const int MaxAllowedFiles = 10;

        // ========== LIFECYCLE ==========
        protected override async Task OnParametersSetAsync()
        {
            await InitializePageData();
        }

        // ========== INITIALIZATION ==========
        protected async Task InitializePageData()
        {
            State.ResetPageState();
            try
            {
                var authState = await AuthStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                await LoadUserData(user);
                await CategoryDetailsService.LoadCategoryDataAsync(State, Id, State.CurrentUserId);
            }
            catch (Exception ex)
            {
                State.ErrorMessage = "Error loading: " + ex.Message;
            }
            finally
            {
                State.IsLoading = false;
            }
        }

        private async Task LoadUserData(System.Security.Claims.ClaimsPrincipal user)
        {
            State.CurrentUserId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (State.CurrentUserId == null && user.Identity?.IsAuthenticated == true)
            {
                using var scope = ScopeFactory.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
                var dbUser = await userManager.GetUserAsync(user);
                State.CurrentUserId = dbUser?.Id;
                State.CurrentUser = dbUser;
            }
            else if (State.CurrentUserId != null)
            {
                State.CurrentUser = await CategoryDetailsService.LoadCurrentUserAsync(State.CurrentUserId);
            }

            if (user.Identity?.IsAuthenticated == true)
            {
                State.IsAdmin = user.IsInRole("Admin");
            }
        }

        // ========== NAVIGATION ==========
        protected void ClearSearchAndNavigate()
        {
            FocusPostId = null;
            Navigation.NavigateTo($"/category/{Id}");
        }

        protected void RefreshPage() => Navigation.NavigateTo(Navigation.Uri, forceLoad: true);
        protected void NavigateToCategory(int id) => Navigation.NavigateTo($"/category/{id}", forceLoad: true);
        protected void NavigateToCreate() => Navigation.NavigateTo($"/categories/create?name={State.SearchTerm}");

        // ========== CAROUSEL ==========
        protected int GetCurrentIndex(int postId)
        {
            if (!State.CarouselIndices.ContainsKey(postId))
                State.CarouselIndices[postId] = 0;
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

        // ========== COMMENTS ==========
        protected bool IsCommentSectionVisible(int postId) =>
            State.ShowComments.ContainsKey(postId) && State.ShowComments[postId];

        protected async Task ToggleComments(int postId)
        {
            if (string.IsNullOrEmpty(State.CurrentUserId))
            {
                State.ShowLoginOverlay = true;
                return;
            }

            if (!State.ShowComments.ContainsKey(postId))
                State.ShowComments[postId] = false;

            State.ShowComments[postId] = !State.ShowComments[postId];

            if (State.ShowComments[postId])
                await LoadComments(postId);

            StateHasChanged();
        }

        private async Task LoadComments(int postId)
        {
            if (!State.PostComments.ContainsKey(postId) || State.PostComments[postId] == null)
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
        }

        protected void UpdateCommentDraft(int postId, string? value)
        {
            if (value != null)
                State.CommentDrafts[postId] = value;
        }

        protected async Task SubmitComment(int postId)
        {
            if (State.CurrentUser == null)
            {
                State.ShowLoginOverlay = true;
                return;
            }

            if (!State.CommentDrafts.ContainsKey(postId) || string.IsNullOrWhiteSpace(State.CommentDrafts[postId]))
                return;

            try
            {
                var newComment = new Comments
                {
                    PostID = postId,
                    UserID = State.CurrentUser.Id,
                    Content = State.CommentDrafts[postId],
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    CreatedBy = State.CurrentUser.Id,
                    UpdatedBy = State.CurrentUser.Id
                };

                await PostService.CreateCommentAsync(newComment);

                State.CommentDrafts[postId] = "";
                newComment.User = State.CurrentUser;

                if (!State.PostComments.ContainsKey(postId))
                    State.PostComments[postId] = new List<Comments>();

                State.PostComments[postId].Add(newComment);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error posting comment: " + ex.Message);
            }
        }

        // ========== REPLIES ==========
        protected void ToggleReplyBox(int commentId)
        {
            if (!State.ActiveReplyBoxes.ContainsKey(commentId))
                State.ActiveReplyBoxes[commentId] = false;

            State.ActiveReplyBoxes[commentId] = !State.ActiveReplyBoxes[commentId];

            if (!State.ReplyDrafts.ContainsKey(commentId))
                State.ReplyDrafts[commentId] = "";

            StateHasChanged();
        }

        protected void UpdateReplyDraft(int commentId, string? value)
        {
            if (value != null)
                State.ReplyDrafts[commentId] = value;
        }

        protected async Task SubmitReply(int postId, int parentCommentId)
        {
            if (State.CurrentUser == null)
            {
                State.ShowLoginOverlay = true;
                return;
            }

            if (!State.ReplyDrafts.ContainsKey(parentCommentId) ||
                string.IsNullOrWhiteSpace(State.ReplyDrafts[parentCommentId]))
                return;

            try
            {
                var newComment = new Comments
                {
                    PostID = postId,
                    UserID = State.CurrentUser.Id,
                    Content = State.ReplyDrafts[parentCommentId],
                    ParentCommentID = parentCommentId,
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    CreatedBy = State.CurrentUser.Id,
                    UpdatedBy = State.CurrentUser.Id
                };

                await PostService.CreateCommentAsync(newComment);

                State.ReplyDrafts[parentCommentId] = "";
                State.ActiveReplyBoxes[parentCommentId] = false;
                newComment.User = State.CurrentUser;

                if (!State.PostComments.ContainsKey(postId))
                    State.PostComments[postId] = new List<Comments>();

                State.PostComments[postId].Add(newComment);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error posting reply: " + ex.Message);
            }
        }

        // ========== LIKES ==========
        protected async Task ToggleLike(int postId)
        {
            if (string.IsNullOrEmpty(State.CurrentUserId))
            {
                State.ShowLoginOverlay = true;
                return;
            }

            try
            {
                await PostService.ToggleLikeAsync(postId, State.CurrentUserId);

                var post = State.CategoryPosts.FirstOrDefault(p => p.Id == postId);
                if (post != null)
                {
                    using var context = DbFactory.CreateDbContext();
                    post.Likes = await context.PostLikes.Where(l => l.PostID == postId).ToListAsync();
                }

                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling like: {ex.Message}");
            }
        }

        // ========== POSTS ==========
        protected bool CanUserPost()
        {
            if (State.CurrentCategory?.IsVerified == true)
                return State.IsAdmin || State.HasApprovedAccess;
            return !string.IsNullOrEmpty(State.CurrentUserId);
        }

        protected async Task HandleCreatePost()
        {
            if (string.IsNullOrWhiteSpace(State.NewPostContent) && State.PendingUploads.Count == 0)
                return;

            if (string.IsNullOrEmpty(State.CurrentUserId))
            {
                State.ShowLoginOverlay = true;
                return;
            }

            if (State.CurrentCategory?.IsVerified == true && !State.IsAdmin && !State.HasApprovedAccess)
            {
                State.ErrorMessage = "This category is verified. You must request access to post here.";
                return;
            }

            try
            {
                using var context = DbFactory.CreateDbContext();
                bool dbHasAccess = await context.CategoryAccessRequests
                    .AnyAsync(r => r.UserId == State.CurrentUserId && r.CategoryId == Id && r.Status == "Approved");

                if (State.CurrentCategory?.IsVerified == true && !State.IsAdmin && !dbHasAccess)
                {
                    State.ErrorMessage = "This category is verified. You must request access to post here.";
                    return;
                }

                var newPost = new Posts
                {
                    Content = State.NewPostContent,
                    CategoryId = Id,
                    UserId = State.CurrentUserId,
                    PostStatus = "Published",
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    CreatedBy = State.CurrentUserId,
                    UpdatedBy = State.CurrentUserId,
                    Media = new List<PostMedia>()
                };

                foreach (var file in State.PendingUploads)
                {
                    newPost.Media.Add(new PostMedia
                    {
                        MediaPath = file.SavedPath,
                        MediaType = file.MediaType
                    });
                }

                await CategoryDetailsService.CreatePostAsync(newPost);
                State.ClosePostModal();
                State.CategoryPosts = await PostService.GetCategoryPostsAsync(Id);
            }
            catch (Exception ex)
            {
                State.ErrorMessage = "Error posting: " + ex.Message;
            }
        }

        // ========== FILE UPLOADS ==========
        protected async Task HandleFilesSelected(InputFileChangeEventArgs e)
        {
            State.ErrorMessage = "";
            State.IsUploading = true;

            try
            {
                var newFiles = e.GetMultipleFiles(MaxAllowedFiles);
                var uploadsFolder = Path.Combine(Environment.WebRootPath, "uploads");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                foreach (var file in newFiles)
                {
                    if (State.PendingUploads.Count >= MaxAllowedFiles)
                        break;

                    if (file.Size <= MaxFileSize)
                    {
                        try
                        {
                            var extension = Path.GetExtension(file.Name).ToLower();
                            bool isVideo = extension is ".mp4" or ".webm" or ".mov";
                            var fileName = $"{Guid.NewGuid()}{extension}";
                            var physicalPath = Path.Combine(uploadsFolder, fileName);
                            var webPath = $"/uploads/{fileName}";

                            await using var fileStream = new FileStream(physicalPath, FileMode.Create);
                            await file.OpenReadStream(MaxFileSize).CopyToAsync(fileStream);

                            State.PendingUploads.Add(new PendingFile
                            {
                                SavedPath = webPath,
                                PhysicalPath = physicalPath,
                                MediaType = isVideo ? "Video" : "Image"
                            });
                        }
                        catch (Exception ex)
                        {
                            State.ErrorMessage = $"Error reading {file.Name}: {ex.Message}";
                        }
                    }
                    else
                    {
                        State.ErrorMessage = $"File {file.Name} is too large.";
                    }
                }
            }
            finally
            {
                State.IsUploading = false;
            }
        }

        protected void RemoveFile(int index)
        {
            if (index >= 0 && index < State.PendingUploads.Count)
            {
                var file = State.PendingUploads[index];
                try
                {
                    if (File.Exists(file.PhysicalPath))
                        File.Delete(file.PhysicalPath);
                }
                catch { }
                State.PendingUploads.RemoveAt(index);
            }
        }

        // ========== FOLLOW ==========
        protected async Task ToggleFollow()
        {
            if (string.IsNullOrEmpty(State.CurrentUserId))
            {
                State.ShowLoginOverlay = true;
                return;
            }

            try
            {
                await CategoryService.ToggleFollowAsync(State.CurrentUserId, Id);
                State.IsFollowing = !State.IsFollowing;
                State.FollowedCategories = await CategoryService.GetFollowedCategoriesAsync(State.CurrentUserId);
            }
            catch (Exception ex)
            {
                State.ErrorMessage = ex.Message;
            }
        }

        // ========== ACCESS REQUEST ==========
        protected async Task HandleSubmitRequest()
        {
            if (string.IsNullOrEmpty(State.CurrentUserId))
            {
                State.ShowLoginOverlay = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(State.RequestReason))
            {
                State.ErrorMessage = "Please provide a reason for your request.";
                return;
            }

            try
            {
                using var context = DbFactory.CreateDbContext();
                
                // Check for existing request
                var existingRequest = await context.CategoryAccessRequests
                    .FirstOrDefaultAsync(r => r.UserId == State.CurrentUserId && r.CategoryId == Id);

                if (existingRequest != null)
                {
                    if (existingRequest.Status == "Pending")
                    {
                        State.ErrorMessage = "You already have a pending request for this category.";
                        State.ShowRequestModal = false;
                        return;
                    }
                    else if (existingRequest.Status == "Approved")
                    {
                        State.ErrorMessage = "You already have access to this category.";
                        State.ShowRequestModal = false;
                        return;
                    }
                    else if (existingRequest.Status == "Rejected")
                    {
                        // Check 7-day cooldown
                        var canResubmitDate = existingRequest.DateUpdated.AddDays(7);
                        var daysRemaining = (canResubmitDate - DateTime.Now).TotalDays;
                        
                        if (daysRemaining > 0)
                        {
                            State.ErrorMessage = $"Your previous request was rejected on {existingRequest.DateUpdated:MMM dd, yyyy 'at' h:mm tt}. You can resubmit on {canResubmitDate:MMM dd, yyyy 'at' h:mm tt} ({Math.Ceiling(daysRemaining)} day(s) remaining).";
                            State.ShowRequestModal = false;
                            return;
                        }
                        
                        // Update existing rejected request
                        existingRequest.Reason = State.RequestReason;
                        existingRequest.Status = "Pending";
                        existingRequest.DateUpdated = DateTime.Now;
                        existingRequest.UpdatedBy = null;
                        
                        await context.SaveChangesAsync();
                        
                        State.ShowRequestModal = false;
                        State.RequestReason = "";
                        State.HasPendingRequest = true;
                        State.ShowNotification("Request resubmitted successfully!", "success");
                        StateHasChanged();
                        return;
                    }
                }

                // Create new request
                var success = await CategoryDetailsService.SubmitAccessRequestAsync(
                    State.CurrentUserId, Id, State.RequestReason);

                if (success)
                {
                    State.ShowRequestModal = false;
                    State.RequestReason = "";
                    State.HasPendingRequest = true;
                    State.ShowNotification("Request submitted successfully!", "success");
                    StateHasChanged();
                }
                else
                {
                    State.ErrorMessage = "You already have a pending request.";
                    State.ShowRequestModal = false;
                    State.HasPendingRequest = true;
                }
            }
            catch (Exception ex)
            {
                State.ErrorMessage = "Error sending request: " + ex.Message;
            }
        }

        // ========== REPORTS ==========
        protected void OpenReportModal(int postId)
        {
            if (string.IsNullOrEmpty(State.CurrentUserId))
            {
                State.ShowLoginOverlay = true;
                return;
            }

            State.ReportPostId = postId;
            State.ReportReasonSelection = "Spam";
            State.ReportReasonDetails = "";
            State.ShowReportModal = true;
        }

        protected async Task SubmitReport()
        {
            if (State.ReportPostId == 0 || string.IsNullOrEmpty(State.CurrentUserId))
            {
                State.ShowNotification("Session error. Please refresh and try again.", "error");
                return;
            }

            try
            {
                var submitted = await CategoryDetailsService.SubmitReportAsync(
                    State.ReportPostId, State.CurrentUserId,
                    State.ReportReasonSelection, State.ReportReasonDetails);

                State.ShowReportModal = false;

                if (submitted)
                {
                    State.ShowNotification("Report submitted successfully.", "success");
                }
                else
                {
                    State.ShowNotification("You have already reported this post.", "error");
                }
            }
            catch (Exception ex)
            {
                State.ShowNotification("Error sending report: " + ex.Message, "error");
            }
        }

        // ========== CATEGORY SEARCH ==========
        protected async Task HandleCategorySearch()
        {
            if (string.IsNullOrWhiteSpace(State.SearchTerm))
                return;

            var cat = await CategoryService.GetCategoryByNameAsync(State.SearchTerm.Trim());

            if (cat != null)
                Navigation.NavigateTo($"/category/{cat.Id}", true);
            else
                State.ShowCreateConfirm = true;
        }

        // ========== RENDER FRAGMENTS ==========
        protected RenderFragment RenderCarousel(Posts post) => builder =>
        {
            var mediaList = post.Media.ToList();
            var totalMedia = mediaList.Count;
            var currentIndex = GetCurrentIndex(post.Id);
            var activeMedia = mediaList[currentIndex];

            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "carousel-container");

            if (totalMedia > 1)
            {
                builder.OpenElement(2, "button");
                builder.AddAttribute(3, "class", "carousel-btn prev");
                builder.AddAttribute(4, "onclick", EventCallback.Factory.Create(this, () => PrevSlide(post.Id, totalMedia)));
                builder.AddContent(5, "?");
                builder.CloseElement();
            }

            if (activeMedia.MediaType == "Video")
            {
                builder.OpenElement(6, "video");
                builder.AddAttribute(7, "class", "carousel-media");
                builder.AddAttribute(8, "controls", true);
                builder.AddAttribute(9, "src", activeMedia.MediaPath);
                builder.CloseElement();
            }
            else
            {
                builder.OpenElement(11, "img");
                builder.AddAttribute(12, "class", "carousel-media");
                builder.AddAttribute(13, "src", activeMedia.MediaPath);
                builder.AddAttribute(14, "alt", "Post Media");
                builder.CloseElement();
            }

            if (totalMedia > 1)
            {
                builder.OpenElement(16, "button");
                builder.AddAttribute(17, "class", "carousel-btn next");
                builder.AddAttribute(18, "onclick", EventCallback.Factory.Create(this, () => NextSlide(post.Id, totalMedia)));
                builder.AddContent(19, "?");
                builder.CloseElement();

                builder.OpenElement(20, "div");
                builder.AddAttribute(21, "class", "carousel-counter");
                builder.AddContent(22, $"{currentIndex + 1} / {totalMedia}");
                builder.CloseElement();
            }

            builder.CloseElement();
        };

        protected RenderFragment RenderLikeButton(Posts post) => builder =>
        {
            var isLiked = post.Likes.Any(l => l.UserID == State.CurrentUserId);
            var likeCount = post.Likes.Count;

            if (isLiked)
            {
                builder.OpenElement(0, "i");
                builder.AddAttribute(1, "class", "fas fa-heart");
                builder.AddAttribute(2, "style", "color: #e0245e; margin-right: 5px;");
                builder.CloseElement();
            }
            else
            {
                builder.OpenElement(3, "i");
                builder.AddAttribute(4, "class", "far fa-heart");
                builder.AddAttribute(5, "style", "margin-right: 5px;");
                builder.CloseElement();
            }

            builder.OpenElement(6, "span");
            builder.AddContent(7, likeCount > 0 ? likeCount.ToString() : "Like");
            builder.CloseElement();
        };
    }
}
