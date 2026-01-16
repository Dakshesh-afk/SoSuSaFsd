using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SoSuSaFsd.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SoSuSaFsd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<Users> _signInManager;
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            SignInManager<Users> signInManager,
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password)
        {
            // 1. Find the user first so we can check their status
            var user = await _userManager.FindByNameAsync(username);

            // 2. CHECK IF BANNED
            // If the user exists AND IsActive is false, we kick them out.
            if (user != null && user.IsActive == false)
            {
                // Return to home page with the specific error message
                return Redirect("/?error=" + Uri.EscapeDataString("Your account has been suspended."));
            }

            // 3. If they passed the ban check, proceed with normal password check
            var result = await _signInManager.PasswordSignInAsync(username, password, isPersistent: true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return Redirect("/");
            }

            return Redirect("/?error=InvalidLogin");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] string email, [FromForm] string username, [FromForm] string password)
        {
            var newUser = new Users
            {
                UserName = username,
                Email = email,
                DateCreated = DateTime.Now,
                IsActive = true // New users are active by default
            };

            var result = await _userManager.CreateAsync(newUser, password);

            if (result.Succeeded)
            {
                // Assign Admin role if applicable
                if (!await _roleManager.RoleExistsAsync("Admin"))
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));

                if (username.ToLower() == "admin")
                    await _userManager.AddToRoleAsync(newUser, "Admin");

                // Sign them in (Set Cookie)
                await _signInManager.SignInAsync(newUser, isPersistent: true);

                return Redirect("/");
            }

            string errorMsg = result.Errors.FirstOrDefault()?.Description ?? "Registration failed";
            return Redirect($"/?error={Uri.EscapeDataString(errorMsg)}");
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/");
        }
    }
}