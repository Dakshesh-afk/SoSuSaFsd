using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoSuSaFsd.Data;
using SoSuSaFsd.Domain;

namespace SoSuSaFsd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly SoSuSaFsdContext _context;

        public PostsController(SoSuSaFsdContext context)
        {
            _context = context;
        }

        // GET: api/Posts
        // Returns ALL posts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Posts>>> GetPosts()
        {
            return await _context.Posts.ToListAsync();
        }

        // GET: api/Posts/5
        // Returns ONE post by ID.
        // FIX: The previous issue was likely returning the first item regardless of ID. 
        // This code uses .FindAsync(id) to get the specific one.
        [HttpGet("{id}")]
        public async Task<ActionResult<Posts>> GetPost(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            return post;
        }

        // POST: api/Posts
        // Creates a NEW post
        [HttpPost]
        [Authorize] // This ensures the person MUST be logged in
        public async Task<ActionResult<Posts>> CreatePost(Posts post)
        {
            // 1. Get the ID of the person actually logged in
            var loggedInUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // 2. Mastery Check: Ensure the person isn't trying to post for someone else
            if (post.UserId != loggedInUserId)
            {
                // Use BadRequest or return a custom object to avoid the 'Scheme' error
                return BadRequest(new { message = "Security Violation: You cannot create a post for a different user account." });
            }

            post.Id = 0;
            post.DateCreated = DateTime.Now;

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPost", new { id = post.Id }, post);
        }

        // PUT: api/Posts/5
        // Updates an EXISTING post
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPost(int id, Posts post)
        {
            if (id != post.Id)
            {
                return BadRequest();
            }

            _context.Entry(post).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Posts/5
        // Deletes a post
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PostsExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}