using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using SoSuSaFsd.Data;
using SoSuSaFsd.Domain;
using SoSuSaFsd.Services;

namespace SoSuSaFsd.Components.Pages.ProfileComponents
{
    /// <summary>
    /// Base class for Profile page
    /// Extracts all logic from Profile.razor for clean separation of concerns
    /// </summary>
    public abstract class ProfileBase : ComponentBase
    {
        // ========== INJECTED DEPENDENCIES ==========
        [Inject] protected IDbContextFactory<SoSuSaFsdContext> DbFactory { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
        [Inject] protected IServiceScopeFactory ScopeFactory { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] protected IPostService PostService { get; set; } = default!;
        [Inject] protected IProfileService ProfileService { get; set; } = default!;

        // ========== PARAMETERS ==========
        [Parameter] public string? Username { get; set; }

        // ========== STATE ==========
        protected ProfileState State { get; set; } = new();

        // ========== LIFECYCLE ==========
        protected override async Task OnParametersSetAsync()
        {
            await LoadProfileData();
        }

        // ========== DATA LOADING ==========
        protected async Task LoadProfileData()
        {
            State.Reset();
            State.IsLoading = true;

            try
            {
                // Load current authenticated user
                await LoadCurrentUser();

                // Load profile user and their data
                if (!string.IsNullOrEmpty(Username))
                {
                    State.ProfileUser = await ProfileService.GetUserByUsernameAsync(Username);

                    if (State.ProfileUser != null)
                    {
                        State.PageTitle = $"{State.ProfileUser.DisplayName ?? State.ProfileUser.UserName} - Profile";

                        // Load user's posts using PostService
                        State.UserPosts = await PostService.GetUserPostsAsync(State.ProfileUser.Id);

                        // Load follower count
                        State.FollowerCount = await ProfileService.GetFollowerCountAsync(State.ProfileUser.Id);
                    }
                    else
                    {
                        State.ErrorMessage = $"User '{Username}' not found.";
                    }
                }
            }
            catch (Exception ex)
            {
                State.ErrorMessage = "Error loading profile: " + ex.Message;
            }
            finally
            {
                State.IsLoading = false;
            }
        }

        private async Task LoadCurrentUser()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (!string.IsNullOrEmpty(userId))
                {
                    using var scope = ScopeFactory.CreateScope();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
                    State.CurrentUser = await userManager.FindByIdAsync(userId);
                }
            }
        }

        // ========== USER REPORTING ==========
        protected async Task ReportUserAsync()
        {
            if (State.ProfileUser == null || State.CurrentUser == null)
                return;

            try
            {
                var promptText = $"Report @{State.ProfileUser.UserName}. Please enter a brief reason:";
                var reason = await JSRuntime.InvokeAsync<string>("prompt", promptText);

                if (string.IsNullOrWhiteSpace(reason))
                {
                    State.ReportStatusMessage = "Report cancelled.";
                    return;
                }

                var submitted = await ProfileService.SubmitUserReportAsync(
                    State.ProfileUser.Id, 
                    State.CurrentUser.Id, 
                    reason);

                State.ReportStatusMessage = submitted 
                    ? "Report submitted successfully." 
                    : "You have already reported this user.";
            }
            catch (Exception ex)
            {
                State.ReportStatusMessage = "Error: " + ex.Message;
            }
        }

        // ========== POST INTERACTIONS ==========
        protected async Task ToggleLike(int postId)
        {
            if (State.CurrentUser == null)
            {
                Navigation.NavigateTo("/");
                return;
            }

            try
            {
                await PostService.ToggleLikeAsync(postId, State.CurrentUser.Id);

                // Refresh likes for this post
                var post = State.UserPosts.FirstOrDefault(p => p.Id == postId);
                if (post != null)
                {
                    using var context = DbFactory.CreateDbContext();
                    post.Likes = await context.PostLikes
                        .Where(l => l.PostID == postId)
                        .ToListAsync();
                }

                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling like: {ex.Message}");
            }
        }

        protected async Task ToggleComments(int postId)
        {
            if (State.CurrentUser == null)
            {
                Navigation.NavigateTo("/");
                return;
            }

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

        protected void UpdateCommentDraft(int postId, string? content)
        {
            if (content != null)
                State.CommentDrafts[postId] = content;
        }

        protected async Task SubmitComment(int postId)
        {
            if (State.CurrentUser == null)
            {
                Navigation.NavigateTo("/");
                return;
            }

            if (!State.CommentDrafts.ContainsKey(postId) || 
                string.IsNullOrWhiteSpace(State.CommentDrafts[postId]))
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
                    CreatedBy = State.CurrentUser.Id
                };

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

        protected void UpdateReplyDraft(int commentId, string? content)
        {
            if (content != null)
                State.ReplyDrafts[commentId] = content;
        }

        protected async Task SubmitReply(int postId, int parentCommentId)
        {
            if (State.CurrentUser == null)
            {
                Navigation.NavigateTo("/");
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
                    CreatedBy = State.CurrentUser.Id
                };

                await PostService.CreateCommentAsync(newComment);

                State.ReplyDrafts[parentCommentId] = "";
                State.ActiveReplyBoxes[parentCommentId] = false;
                newComment.User = State.CurrentUser;

                EnsureCommentListExists(postId);
                State.PostComments[postId].Add(newComment);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error posting reply: " + ex.Message);
            }
        }

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

        // ========== REPORTS ==========
        protected void OpenReportModal(int postId)
        {
            Console.WriteLine($"Report post {postId}");
            // Could be expanded to show a modal in the future
        }

        // ========== HELPER METHODS ==========
        private void EnsureCommentListExists(int postId)
        {
            if (!State.PostComments.ContainsKey(postId))
                State.PostComments[postId] = new List<Comments>();
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
            var isLiked = State.CurrentUser != null && post.Likes.Any(l => l.UserID == State.CurrentUser.Id);
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
            if (likeCount > 0)
            {
                builder.AddContent(7, likeCount);
            }
            else
            {
                builder.AddContent(7, "Like");
            }
            builder.CloseElement();
        };
    }
}
