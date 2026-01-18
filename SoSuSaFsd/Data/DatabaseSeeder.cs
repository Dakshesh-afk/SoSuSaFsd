using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoSuSaFsd.Domain;

namespace SoSuSaFsd.Data
{
    public class DatabaseSeeder
    {
        private readonly SoSuSaFsdContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            SoSuSaFsdContext context,
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                await SeedRolesAsync();
                await SeedUsersAsync();
                await SeedCategoriesAsync();
                await SeedPostsAsync();
                await SeedPostMediaAsync();
                await SeedCommentsAsync();
                await SeedLikesAsync();
                await SeedCategoryFollowsAsync();
                await SeedReportsAsync(); // NEW: Seed sample reports
                await SeedCategoryAccessRequestsAsync(); // NEW: Seed verification requests

                _logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private async Task SeedRolesAsync()
        {
            string[] roleNames = { "Admin", "User" };
            
            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                    _logger.LogInformation($"Created role: {roleName}");
                }
            }
        }

        private async Task SeedUsersAsync()
        {
            var users = new List<(string Username, string Email, string Password, string Role, string FirstName, string LastName, string? Bio, bool IsVerified)>
            {
                ("SoSuSaAdmin", "admin@sosusa.com", "Admin@123", "Admin", "Admin", "User", "Platform Administrator", true),
                ("Louis", "louis@example.com", "User@123", "User", "Louis", "Tan", "Tech enthusiast | Developer", true),
                ("Dakshesh", "dakshesh@example.com", "User@123", "User", "Dakshesh", "Kumar", "Code wizard | Full-stack dev", true),
                ("gamer_123", "gamer123@example.com", "User@123", "User", "Marcus", "Lee", "Gaming is life | Twitch streamer", false),
                ("troller67", "troller67@example.com", "User@123", "User", "Ryan", "Ng", "Professional meme lord", true),
                ("Mothership", "mothership@example.com", "User@123", "User", "Sarah", "Lim", "Breaking news 24/7 | Journalist", true),
                ("temasekteacher", "temasekteacher@example.com", "User@123", "User", "David", "Tan", "Educator @ Temasek Poly | Teaching tech", true)
            };

            foreach (var (username, email, password, role, firstName, lastName, bio, isVerified) in users)
            {
                var existingUser = await _userManager.FindByEmailAsync(email);
                
                if (existingUser == null)
                {
                    var user = new Users
                    {
                        UserName = username,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        DisplayName = $"{firstName} {lastName}",
                        Bio = bio,
                        DateCreated = DateTime.Now.AddDays(-Random.Shared.Next(30, 365)),
                        DateUpdated = DateTime.Now,
                        DateOfBirth = DateTime.Now.AddYears(-Random.Shared.Next(20, 50)),
                        IsActive = true,
                        IsVerified = isVerified,
                        Role = role,
                        EmailConfirmed = true,
                        ProfileImage = $"https://ui-avatars.com/api/?name={firstName}+{lastName}&background=random"
                    };

                    var result = await _userManager.CreateAsync(user, password);
                    
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, role);
                        _logger.LogInformation($"Created user: {username} with role: {role}");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to create user {username}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private async Task SeedCategoriesAsync()
        {
            if (await _context.Categories.AnyAsync())
            {
                _logger.LogInformation("Categories already exist. Skipping category seeding.");
                return;
            }

            var adminUser = await _userManager.FindByEmailAsync("admin@sosusa.com");
            var mothershipUser = await _userManager.FindByEmailAsync("mothership@example.com");
            var temasekUser = await _userManager.FindByEmailAsync("temasekteacher@example.com");
            var louisUser = await _userManager.FindByEmailAsync("louis@example.com");

            var categories = new List<Categories>
            {
                new Categories
                {
                    CategoryName = "SG News",
                    CategoryDescription = "Latest news and updates from Singapore",
                    CategoryIsRestricted = false,
                    IsVerified = true,
                    CreatedBy = mothershipUser?.Id ?? adminUser?.Id,
                    DateCreated = DateTime.Now.AddDays(-120),
                    DateUpdated = DateTime.Now.AddDays(-120)
                },
                new Categories
                {
                    CategoryName = "Temasek Poly News",
                    CategoryDescription = "News, events, and updates from Temasek Polytechnic",
                    CategoryIsRestricted = false,
                    IsVerified = true,
                    CreatedBy = temasekUser?.Id ?? adminUser?.Id,
                    DateCreated = DateTime.Now.AddDays(-110),
                    DateUpdated = DateTime.Now.AddDays(-110)
                },
                new Categories
                {
                    CategoryName = "Gaming",
                    CategoryDescription = "Game reviews, esports, and gaming discussions",
                    CategoryIsRestricted = false,
                    IsVerified = false,
                    CreatedBy = adminUser?.Id,
                    DateCreated = DateTime.Now.AddDays(-100),
                    DateUpdated = DateTime.Now.AddDays(-100)
                },
                new Categories
                {
                    CategoryName = "Technology",
                    CategoryDescription = "Tech news, gadgets, and innovations",
                    CategoryIsRestricted = false,
                    IsVerified = false,
                    CreatedBy = louisUser?.Id ?? adminUser?.Id,
                    DateCreated = DateTime.Now.AddDays(-95),
                    DateUpdated = DateTime.Now.AddDays(-95)
                },
                new Categories
                {
                    CategoryName = "Programming",
                    CategoryDescription = "Code, tutorials, and developer discussions",
                    CategoryIsRestricted = false,
                    IsVerified = false,
                    CreatedBy = louisUser?.Id ?? adminUser?.Id,
                    DateCreated = DateTime.Now.AddDays(-90),
                    DateUpdated = DateTime.Now.AddDays(-90)
                },
                new Categories
                {
                    CategoryName = "Memes & Humor",
                    CategoryDescription = "Dank memes and funny content",
                    CategoryIsRestricted = false,
                    IsVerified = false,
                    CreatedBy = adminUser?.Id,
                    DateCreated = DateTime.Now.AddDays(-85),
                    DateUpdated = DateTime.Now.AddDays(-85)
                },
                new Categories
                {
                    CategoryName = "Education",
                    CategoryDescription = "Learning resources, study tips, and academic discussions",
                    CategoryIsRestricted = false,
                    IsVerified = false,
                    CreatedBy = temasekUser?.Id ?? adminUser?.Id,
                    DateCreated = DateTime.Now.AddDays(-80),
                    DateUpdated = DateTime.Now.AddDays(-80)
                },
                new Categories
                {
                    CategoryName = "Food & Dining",
                    CategoryDescription = "Hawker recommendations, cafes, and food reviews",
                    CategoryIsRestricted = false,
                    IsVerified = false,
                    CreatedBy = adminUser?.Id,
                    DateCreated = DateTime.Now.AddDays(-75),
                    DateUpdated = DateTime.Now.AddDays(-75)
                },
                new Categories
                {
                    CategoryName = "VIP Lounge",
                    CategoryDescription = "Exclusive category for verified members only",
                    CategoryIsRestricted = true,
                    IsVerified = true,
                    CreatedBy = adminUser?.Id,
                    DateCreated = DateTime.Now.AddDays(-70),
                    DateUpdated = DateTime.Now.AddDays(-70)
                },
                new Categories
                {
                    CategoryName = "General",
                    CategoryDescription = "Random discussions and off-topic conversations",
                    CategoryIsRestricted = false,
                    IsVerified = false,
                    CreatedBy = adminUser?.Id,
                    DateCreated = DateTime.Now.AddDays(-130),
                    DateUpdated = DateTime.Now.AddDays(-130)
                }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Seeded {categories.Count} categories.");
        }

        private async Task SeedPostsAsync()
        {
            if (await _context.Posts.AnyAsync())
            {
                _logger.LogInformation("Posts already exist. Skipping post seeding.");
                return;
            }

            var users = await _context.Users.ToListAsync();
            var categories = await _context.Categories.ToListAsync();

            if (!users.Any() || !categories.Any())
            {
                _logger.LogWarning("No users or categories found. Skipping post seeding.");
                return;
            }

            var sgNewsCategory = categories.FirstOrDefault(c => c.CategoryName == "SG News");
            var temasekCategory = categories.FirstOrDefault(c => c.CategoryName == "Temasek Poly News");
            var gamingCategory = categories.FirstOrDefault(c => c.CategoryName == "Gaming");
            var techCategory = categories.FirstOrDefault(c => c.CategoryName == "Technology");
            var programmingCategory = categories.FirstOrDefault(c => c.CategoryName == "Programming");
            var memesCategory = categories.FirstOrDefault(c => c.CategoryName == "Memes & Humor");
            var educationCategory = categories.FirstOrDefault(c => c.CategoryName == "Education");
            var generalCategory = categories.FirstOrDefault(c => c.CategoryName == "General");

            // Get specific users by username
            var adminUser = users.FirstOrDefault(u => u.UserName == "SoSuSaAdmin");
            var mothership = users.FirstOrDefault(u => u.UserName == "Mothership");
            var temasekteacher = users.FirstOrDefault(u => u.UserName == "temasekteacher");
            var gamer = users.FirstOrDefault(u => u.UserName == "gamer_123");
            var troller = users.FirstOrDefault(u => u.UserName == "troller67");
            var louis = users.FirstOrDefault(u => u.UserName == "Louis");
            var dakshesh = users.FirstOrDefault(u => u.UserName == "Dakshesh");

            var posts = new List<Posts>
            {
                new Posts
                {
                    Content = "BREAKING: New MRT line announced! The Thomson-East Coast Line extension will connect to more neighborhoods. What do you think about the new route?",
                    PostStatus = "Published",
                    UserId = mothership?.Id ?? adminUser?.Id ?? users.First().Id,
                    CategoryId = sgNewsCategory?.Id ?? categories.First().Id,
                    DateCreated = DateTime.Now.AddHours(-6),
                    DateUpdated = DateTime.Now.AddHours(-6),
                    CreatedBy = mothership?.Id ?? adminUser?.Id ?? users.First().Id
                },
                new Posts
                {
                    Content = "Temasek Poly's Open House this weekend! Come check out the IIT, ENG, and BUS schools. There'll be booths, demos, and free snacks",
                    PostStatus = "Published",
                    UserId = temasekteacher?.Id ?? adminUser?.Id ?? users.First().Id,
                    CategoryId = temasekCategory?.Id ?? categories.First().Id,
                    DateCreated = DateTime.Now.AddDays(-1),
                    DateUpdated = DateTime.Now.AddDays(-1),
                    CreatedBy = temasekteacher?.Id ?? adminUser?.Id ?? users.First().Id
                },
                new Posts
                {
                    Content = "Just hit Diamond rank in Valorant! Took me 3 months but finally made it. Any Immortal players got tips for climbing higher?",
                    PostStatus = "Published",
                    UserId = gamer?.Id ?? adminUser?.Id ?? users.First().Id,
                    CategoryId = gamingCategory?.Id ?? categories.First().Id,
                    DateCreated = DateTime.Now.AddHours(-12),
                    DateUpdated = DateTime.Now.AddHours(-12),
                    CreatedBy = gamer?.Id ?? adminUser?.Id ?? users.First().Id
                },
                new Posts
                {
                    Content = "When your code works but you don't know why (Meanwhile: When your code doesn't work and you don't know why)",
                    PostStatus = "Published",
                    UserId = troller?.Id ?? adminUser?.Id ?? users.First().Id,
                    CategoryId = memesCategory?.Id ?? categories.First().Id,
                    DateCreated = DateTime.Now.AddHours(-8),
                    DateUpdated = DateTime.Now.AddHours(-8),
                    CreatedBy = troller?.Id ?? adminUser?.Id ?? users.First().Id
                },
                new Posts
                {
                    Content = "Just finished building a Blazor app with .NET 8 for my FYP! Clean architecture, real-time updates with SignalR. Anyone else working on cool projects?",
                    PostStatus = "Published",
                    UserId = louis?.Id ?? adminUser?.Id ?? users.First().Id,
                    CategoryId = programmingCategory?.Id ?? categories.First().Id,
                    DateCreated = DateTime.Now.AddDays(-2),
                    DateUpdated = DateTime.Now.AddDays(-2),
                    CreatedBy = louis?.Id ?? adminUser?.Id ?? users.First().Id
                },
                new Posts
                {
                    Content = "Hot take: AI won't replace developers, but developers who use AI will replace those who don't. Thoughts?",
                    PostStatus = "Published",
                    UserId = dakshesh?.Id ?? adminUser?.Id ?? users.First().Id,
                    CategoryId = techCategory?.Id ?? categories.First().Id,
                    DateCreated = DateTime.Now.AddDays(-3),
                    DateUpdated = DateTime.Now.AddDays(-3),
                    CreatedBy = dakshesh?.Id ?? adminUser?.Id ?? users.First().Id
                },
                new Posts
                {
                    Content = "Reminder: Assignment deadline next week! Don't forget to check the submission guidelines on Brightspace",
                    PostStatus = "Published",
                    UserId = temasekteacher?.Id ?? adminUser?.Id ?? users.First().Id,
                    CategoryId = temasekCategory?.Id ?? categories.First().Id,
                    DateCreated = DateTime.Now.AddDays(-4),
                    DateUpdated = DateTime.Now.AddDays(-4),
                    CreatedBy = temasekteacher?.Id ?? adminUser?.Id ?? users.First().Id
                },
                new Posts
                {
                    Content = "New COD update is FIRE! The new maps are insane! Who wants to squad up later?",
                    PostStatus = "Published",
                    UserId = gamer?.Id ?? adminUser?.Id ?? users.First().Id,
                    CategoryId = gamingCategory?.Id ?? categories.First().Id,
                    DateCreated = DateTime.Now.AddDays(-5),
                    DateUpdated = DateTime.Now.AddDays(-5),
                    CreatedBy = gamer?.Id ?? adminUser?.Id ?? users.First().Id
                },
                new Posts
                {
                    Content = "Study tip: Pomodoro technique actually works! 25 min focus + 5 min break. Been using it for a week and my productivity is through the roof",
                    PostStatus = "Published",
                    UserId = temasekteacher?.Id ?? adminUser?.Id ?? users.First().Id,
                    CategoryId = educationCategory?.Id ?? categories.First().Id,
                    DateCreated = DateTime.Now.AddDays(-6),
                    DateUpdated = DateTime.Now.AddDays(-6),
                    CreatedBy = temasekteacher?.Id ?? adminUser?.Id ?? users.First().Id
                },
                new Posts
                {
                    Content = "Anyone else excited for the long weekend? Finally can catch up on sleep",
                    PostStatus = "Published",
                    UserId = troller?.Id ?? adminUser?.Id ?? users.First().Id,
                    CategoryId = generalCategory?.Id ?? categories.First().Id,
                    DateCreated = DateTime.Now.AddDays(-7),
                    DateUpdated = DateTime.Now.AddDays(-7),
                    CreatedBy = troller?.Id ?? adminUser?.Id ?? users.First().Id
                }
            };

            await _context.Posts.AddRangeAsync(posts);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Seeded {posts.Count} posts.");
        }

        private async Task SeedPostMediaAsync()
        {
            if (await _context.PostMedia.AnyAsync())
            {
                _logger.LogInformation("Post media already exists. Skipping media seeding.");
                return;
            }

            var posts = await _context.Posts.ToListAsync();

            if (!posts.Any())
            {
                _logger.LogWarning("No posts found. Skipping media seeding.");
                return;
            }

            var mediaList = new List<PostMedia>();

            // Sample placeholder images from Lorem Picsum and videos
            var sampleImages = new[]
            {
                "https://picsum.photos/800/600?random=1",
                "https://picsum.photos/800/600?random=2",
                "https://picsum.photos/800/600?random=3",
                "https://picsum.photos/800/600?random=4",
                "https://picsum.photos/800/600?random=5",
                "https://picsum.photos/800/600?random=6",
                "https://picsum.photos/800/600?random=7",
                "https://picsum.photos/800/600?random=8"
            };

            var sampleVideos = new[]
            {
                "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4",
                "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4",
                "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4"
            };

            // Add media to specific posts (3-5 posts with media)
            var postsToAddMedia = posts.Take(5).ToList();

            foreach (var post in postsToAddMedia)
            {
                var randomMediaCount = Random.Shared.Next(1, 4); // 1-3 media items per post
                var isVideo = Random.Shared.Next(0, 5) == 0; // 20% chance of video

                if (isVideo)
                {
                    // Add a video
                    mediaList.Add(new PostMedia
                    {
                        PostId = post.Id,
                        MediaPath = sampleVideos[Random.Shared.Next(sampleVideos.Length)],
                        MediaType = "Video"
                    });
                }
                else
                {
                    // Add images
                    for (int i = 0; i < randomMediaCount; i++)
                    {
                        mediaList.Add(new PostMedia
                        {
                            PostId = post.Id,
                            MediaPath = sampleImages[Random.Shared.Next(sampleImages.Length)],
                            MediaType = "Image"
                        });
                    }
                }
            }

            await _context.PostMedia.AddRangeAsync(mediaList);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Seeded {mediaList.Count} media items across {postsToAddMedia.Count} posts.");
        }

        private async Task SeedCommentsAsync()
        {
            if (await _context.Comments.AnyAsync())
            {
                _logger.LogInformation("Comments already exist. Skipping comment seeding.");
                return;
            }

            var users = await _context.Users.ToListAsync();
            var posts = await _context.Posts.ToListAsync();

            if (!users.Any() || !posts.Any())
            {
                _logger.LogWarning("No users or posts found. Skipping comment seeding.");
                return;
            }

            // Get specific users by username
            var louis = users.FirstOrDefault(u => u.UserName == "Louis");
            var dakshesh = users.FirstOrDefault(u => u.UserName == "Dakshesh");
            var gamer = users.FirstOrDefault(u => u.UserName == "gamer_123");
            var troller = users.FirstOrDefault(u => u.UserName == "troller67");
            var mothership = users.FirstOrDefault(u => u.UserName == "Mothership");
            var temasekteacher = users.FirstOrDefault(u => u.UserName == "temasekteacher");
            var adminUser = users.FirstOrDefault(u => u.UserName == "SoSuSaAdmin");

            var comments = new List<Comments>
            {
                new Comments
                {
                    Content = "Finally! The current MRT is so crowded during peak hours",
                    PostID = posts[0].Id,
                    UserID = dakshesh?.Id ?? adminUser?.Id ?? users.First().Id,
                    DateCreated = DateTime.Now.AddHours(-5),
                    DateUpdated = DateTime.Now.AddHours(-5),
                    CreatedBy = dakshesh?.Id ?? adminUser?.Id ?? users.First().Id
                },
                new Comments
                {
                    Content = "Will be there! Looking forward to checking out the IT labs",
                    PostID = posts[1].Id,
                    UserID = louis?.Id ?? adminUser?.Id ?? users.First().Id,
                    DateCreated = DateTime.Now.AddHours(-20),
                    DateUpdated = DateTime.Now.AddHours(-20),
                    CreatedBy = louis?.Id ?? adminUser?.Id ?? users.First().Id
                },
                new Comments
                {
                    Content = "Congrats bro! Diamond is already top 5% of players",
                    PostID = posts[2].Id,
                    UserID = troller?.Id ?? adminUser?.Id ?? users.First().Id,
                    DateCreated = DateTime.Now.AddHours(-10),
                    DateUpdated = DateTime.Now.AddHours(-10),
                    CreatedBy = troller?.Id ?? adminUser?.Id ?? users.First().Id
                },
                new Comments
                {
                    Content = "This is too real",
                    PostID = posts[3].Id,
                    UserID = louis?.Id ?? adminUser?.Id ?? users.First().Id,
                    DateCreated = DateTime.Now.AddHours(-7),
                    DateUpdated = DateTime.Now.AddHours(-7),
                    CreatedBy = louis?.Id ?? adminUser?.Id ?? users.First().Id
                },
                new Comments
                {
                    Content = "Nice! Would love to see the code. Is it on GitHub?",
                    PostID = posts[4].Id,
                    UserID = dakshesh?.Id ?? adminUser?.Id ?? users.First().Id,
                    DateCreated = DateTime.Now.AddDays(-1).AddHours(-5),
                    DateUpdated = DateTime.Now.AddDays(-1).AddHours(-5),
                    CreatedBy = dakshesh?.Id ?? adminUser?.Id ?? users.First().Id
                },
                new Comments
                {
                    Content = "100% agree! AI is a tool, not a replacement",
                    PostID = posts[5].Id,
                    UserID = louis?.Id ?? adminUser?.Id ?? users.First().Id,
                    DateCreated = DateTime.Now.AddDays(-2).AddHours(-8),
                    DateUpdated = DateTime.Now.AddDays(-2).AddHours(-8),
                    CreatedBy = louis?.Id ?? adminUser?.Id ?? users.First().Id
                },
                new Comments
                {
                    Content = "Thanks for the reminder! Almost forgot",
                    PostID = posts[6].Id,
                    UserID = gamer?.Id ?? adminUser?.Id ?? users.First().Id,
                    DateCreated = DateTime.Now.AddDays(-3).AddHours(-12),
                    DateUpdated = DateTime.Now.AddDays(-3).AddHours(-12),
                    CreatedBy = gamer?.Id ?? adminUser?.Id ?? users.First().Id
                },
                new Comments
                {
                    Content = "Let's goooo! Add me: gamer_123#6969",
                    PostID = posts[7].Id,
                    UserID = troller?.Id ?? adminUser?.Id ?? users.First().Id,
                    DateCreated = DateTime.Now.AddDays(-4).AddHours(-6),
                    DateUpdated = DateTime.Now.AddDays(-4).AddHours(-6),
                    CreatedBy = troller?.Id ?? adminUser?.Id ?? users.First().Id
                }
            };

            await _context.Comments.AddRangeAsync(comments);
            await _context.SaveChangesAsync();

            // Add a reply to the first comment
            var firstComment = comments[0];
            var replyComment = new Comments
            {
                Content = "Yeah, hopefully this helps with the congestion!",
                PostID = posts[0].Id,
                UserID = mothership?.Id ?? adminUser?.Id ?? users.First().Id,
                ParentCommentID = firstComment.Id,
                DateCreated = DateTime.Now.AddHours(-4),
                DateUpdated = DateTime.Now.AddHours(-4),
                CreatedBy = mothership?.Id ?? adminUser?.Id ?? users.First().Id
            };

            await _context.Comments.AddAsync(replyComment);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Seeded {comments.Count + 1} comments (including replies).");
        }

        private async Task SeedLikesAsync()
        {
            if (await _context.PostLikes.AnyAsync())
            {
                _logger.LogInformation("Likes already exist. Skipping like seeding.");
                return;
            }

            var users = await _context.Users.ToListAsync();
            var posts = await _context.Posts.ToListAsync();

            if (!users.Any() || !posts.Any())
            {
                _logger.LogWarning("No users or posts found. Skipping like seeding.");
                return;
            }

            var likes = new List<PostLikes>();

            // Give each post 2-4 likes from different users
            foreach (var post in posts.Take(8))
            {
                var randomUserCount = Random.Shared.Next(2, Math.Min(users.Count, 5));
                var randomUsers = users.OrderBy(x => Random.Shared.Next()).Take(randomUserCount);

                foreach (var user in randomUsers)
                {
                    likes.Add(new PostLikes
                    {
                        PostID = post.Id,
                        UserID = user.Id,
                        LikedAt = DateTime.Now.AddDays(-Random.Shared.Next(1, 10))
                    });
                }
            }

            await _context.PostLikes.AddRangeAsync(likes);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Seeded {likes.Count} likes.");
        }

        private async Task SeedCategoryFollowsAsync()
        {
            if (await _context.CategoryFollows.AnyAsync())
            {
                _logger.LogInformation("Category follows already exist. Skipping category follow seeding.");
                return;
            }

            var users = await _context.Users.ToListAsync();
            var categories = await _context.Categories.ToListAsync();

            if (!users.Any() || !categories.Any())
            {
                _logger.LogWarning("No users or categories found. Skipping category follow seeding.");
                return;
            }

            var follows = new List<CategoryFollows>();

            // Each user follows 3-5 categories
            foreach (var user in users)
            {
                var randomCategoryCount = Random.Shared.Next(3, Math.Min(categories.Count, 6));
                var randomCategories = categories.OrderBy(x => Random.Shared.Next()).Take(randomCategoryCount);

                foreach (var category in randomCategories)
                {
                    follows.Add(new CategoryFollows
                    {
                        UserId = user.Id,
                        CategoryId = category.Id,
                        DateCreated = DateTime.Now.AddDays(-Random.Shared.Next(5, 30)),
                        DateUpdated = DateTime.Now.AddDays(-Random.Shared.Next(5, 30)),
                        CreatedBy = user.Id
                    });
                }
            }

            await _context.CategoryFollows.AddRangeAsync(follows);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Seeded {follows.Count} category follows.");
        }

        private async Task SeedReportsAsync()
        {
            if (await _context.Reports.AnyAsync())
            {
                _logger.LogInformation("Reports already exist. Skipping report seeding.");
                return;
            }

            var users = await _context.Users.ToListAsync();
            var posts = await _context.Posts.ToListAsync();

            if (!users.Any() || !posts.Any())
            {
                _logger.LogWarning("No users or posts found. Skipping report seeding.");
                return;
            }

            // Get specific users
            var adminUser = users.FirstOrDefault(u => u.UserName == "SoSuSaAdmin");
            var louis = users.FirstOrDefault(u => u.UserName == "Louis");
            var dakshesh = users.FirstOrDefault(u => u.UserName == "Dakshesh");
            var gamer = users.FirstOrDefault(u => u.UserName == "gamer_123");
            var troller = users.FirstOrDefault(u => u.UserName == "troller67");

            var reports = new List<Reports>
            {
                new Reports
                {
                    PostID = posts[3].Id, // The meme post from troller67
                    ReporterID = louis?.Id ?? adminUser?.Id ?? users.First().Id,
                    Reason = "Spam: This appears to be low-effort spam content that doesn't contribute to the discussion.",
                    Status = "Pending",
                    DateCreated = DateTime.Now.AddDays(-2)
                },
                new Reports
                {
                    PostID = posts[7].Id, // The COD gaming post
                    ReporterID = dakshesh?.Id ?? adminUser?.Id ?? users.First().Id,
                    Reason = "Inappropriate: Contains inappropriate language and behavior.",
                    Status = "Pending",
                    DateCreated = DateTime.Now.AddDays(-3)
                },
                new Reports
                {
                    PostID = posts[0].Id, // The MRT news post
                    ReporterID = gamer?.Id ?? adminUser?.Id ?? users.First().Id,
                    Reason = "Misinformation: The information about the MRT line seems inaccurate.",
                    Status = "Resolved",
                    DateCreated = DateTime.Now.AddDays(-5)
                },
                new Reports
                {
                    PostID = posts[2].Id, // The Valorant rank post
                    ReporterID = troller?.Id ?? adminUser?.Id ?? users.First().Id,
                    Reason = "Harassment: User is bragging and making others feel bad about their rank.",
                    Status = "Dismissed",
                    DateCreated = DateTime.Now.AddDays(-7)
                },
                new Reports
                {
                    PostID = posts[4].Id, // Louis's Blazor post
                    ReporterID = gamer?.Id ?? adminUser?.Id ?? users.First().Id,
                    Reason = "Spam: Self-promotion without meaningful content.",
                    Status = "Pending",
                    DateCreated = DateTime.Now.AddDays(-1)
                }
            };

            await _context.Reports.AddRangeAsync(reports);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Seeded {reports.Count} reports (Pending: {reports.Count(r => r.Status == "Pending")}, Resolved: {reports.Count(r => r.Status == "Resolved")}, Dismissed: {reports.Count(r => r.Status == "Dismissed")}).");
        }

        private async Task SeedCategoryAccessRequestsAsync()
        {
            if (await _context.CategoryAccessRequests.AnyAsync())
            {
                _logger.LogInformation("Category access requests already exist. Skipping access request seeding.");
                return;
            }

            var users = await _context.Users.ToListAsync();
            var categories = await _context.Categories.ToListAsync();

            if (!users.Any() || !categories.Any())
            {
                _logger.LogWarning("No users or categories found. Skipping access request seeding.");
                return;
            }

            // Get specific users
            var adminUser = users.FirstOrDefault(u => u.UserName == "SoSuSaAdmin");
            var gamer = users.FirstOrDefault(u => u.UserName == "gamer_123");
            var louis = users.FirstOrDefault(u => u.UserName == "Louis");
            var dakshesh = users.FirstOrDefault(u => u.UserName == "Dakshesh");
            var troller = users.FirstOrDefault(u => u.UserName == "troller67");

            // Get VIP Lounge category (restricted)
            var vipCategory = categories.FirstOrDefault(c => c.CategoryName == "VIP Lounge");

            if (vipCategory == null)
            {
                _logger.LogWarning("VIP Lounge category not found. Skipping access request seeding.");
                return;
            }

            var accessRequests = new List<CategoryAccessRequests>
            {
                new CategoryAccessRequests
                {
                    UserId = gamer?.Id ?? adminUser?.Id ?? users.First().Id,
                    CategoryId = vipCategory.Id,
                    Reason = "I'm an active member of the gaming community and would love to participate in exclusive discussions. I've been a member for 3 months and contribute quality content regularly.",
                    Status = "Pending",
                    DateCreated = DateTime.Now.AddDays(-3),
                    DateUpdated = DateTime.Now.AddDays(-3),
                    CreatedBy = gamer?.Id ?? adminUser?.Id ?? users.First().Id
                },
                new CategoryAccessRequests
                {
                    UserId = louis?.Id ?? adminUser?.Id ?? users.First().Id,
                    CategoryId = vipCategory.Id,
                    Reason = "As a tech developer and active contributor to the Programming and Technology categories, I believe I can add value to VIP discussions. I'd like to network with other verified members.",
                    Status = "Approved",
                    DateCreated = DateTime.Now.AddDays(-10),
                    DateUpdated = DateTime.Now.AddDays(-7),
                    CreatedBy = louis?.Id ?? adminUser?.Id ?? users.First().Id,
                    UpdatedBy = adminUser?.Id
                },
                new CategoryAccessRequests
                {
                    UserId = dakshesh?.Id ?? adminUser?.Id ?? users.First().Id,
                    CategoryId = vipCategory.Id,
                    Reason = "I'm a full-stack developer interested in exclusive tech discussions. Looking to collaborate with other professionals.",
                    Status = "Approved",
                    DateCreated = DateTime.Now.AddDays(-12),
                    DateUpdated = DateTime.Now.AddDays(-8),
                    CreatedBy = dakshesh?.Id ?? adminUser?.Id ?? users.First().Id,
                    UpdatedBy = adminUser?.Id
                },
                new CategoryAccessRequests
                {
                    UserId = troller?.Id ?? adminUser?.Id ?? users.First().Id,
                    CategoryId = vipCategory.Id,
                    Reason = "pls give access i want to see whats inside lol",
                    Status = "Rejected",
                    DateCreated = DateTime.Now.AddDays(-15),
                    DateUpdated = DateTime.Now.AddDays(-13),
                    CreatedBy = troller?.Id ?? adminUser?.Id ?? users.First().Id,
                    UpdatedBy = adminUser?.Id
                }
            };

            await _context.CategoryAccessRequests.AddRangeAsync(accessRequests);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Seeded {accessRequests.Count} category access requests (Pending: {accessRequests.Count(r => r.Status == "Pending")}, Approved: {accessRequests.Count(r => r.Status == "Approved")}, Rejected: {accessRequests.Count(r => r.Status == "Rejected")}).");
        }
    }
}
