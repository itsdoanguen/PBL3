using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3.Models;
using PBL3.ViewModels;
using PBL3.Data;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;


namespace PBL3.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AuthenticationController(ApplicationDbContext context)
        {
            _context = context;
        }
        //GET: Authentication/Index
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Index", "Admin");
                else if (User.IsInRole("Moderator"))
                    return RedirectToAction("Index", "Moderator");
                else if (User.IsInRole("User"))
                    return RedirectToAction("Index", "User");
            }

            return View();
        }
        //GET: Authentication/AccessDenied
        public IActionResult AccessDenied()
        {
            return PartialView("_AccessDeniedPartial");

        }

        //GET: Authentication/Register
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Index", "Admin");
                else if (User.IsInRole("Moderator"))
                    return RedirectToAction("Index", "Moderator");
                else if (User.IsInRole("User"))
                    return RedirectToAction("Index", "User");
            }

            return View();
        }
        //POST: Authentication/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email is already in use");
                    return View(model);
                }
                var user = new UserModel
                {
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = UserModel.UserRole.User
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await SignIn(user);
                return RedirectToAction("Index", "User");
            }
            return View(model);
        }
        //GET: Authentication/Login
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Index", "Admin");
                else if (User.IsInRole("Moderator"))
                    return RedirectToAction("Index", "Moderator");
                else if (User.IsInRole("User"))
                    return RedirectToAction("Index", "User");
            }

            return View();
        }
        //POST: Authentication/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    ModelState.AddModelError("Email", "Invalid email or password");
                    return View(model);
                }

                await SignIn(user);
                if (user.Role == UserModel.UserRole.Admin)
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (user.Role == UserModel.UserRole.Moderator)
                {
                    return RedirectToAction("Index", "Moderator");
                }
                else if (user.Role == UserModel.UserRole.User)
                {
                    return RedirectToAction("Index", "User");
                }
            }
            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Authentication");
        }

        public async Task SignIn(UserModel user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
        }
    }
}
