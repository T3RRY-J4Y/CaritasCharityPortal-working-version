using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Firebase.Database;
using Firebase.Database.Query;
using CaritasCharityPortal.Services;

namespace CaritasCharityPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly FirebaseAuthService _authService;
        private readonly string? _firebaseApiKey;

        public AccountController(IConfiguration configuration)
        {
            var databaseUrl = configuration["Firebase:DatabaseUrl"];
            _firebaseClient = new FirebaseClient(databaseUrl!);
            _authService = new FirebaseAuthService();
            _firebaseApiKey = configuration["Firebase:ApiKey"];
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel? model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { success = false, message = "Email and password required." });

            string selectedRole = model.SelectedRole ?? "User";
            string actualRole = null!;

            // Admin account
            if (model.Email == "admin@caritas.com" && model.Password == "Admin@123")
            {
                actualRole = "Admin";
            }

            else
            {
                try
                {
                    var userRecord = await _authService.GetUserByEmailAsync(model.Email);
                    if (userRecord == null)
                        return Unauthorized(new { success = false, message = "Invalid email or password." });

                    var isValid = await VerifyPasswordWithFirebase(model.Email, model.Password);
                    if (!isValid)
                        return Unauthorized(new { success = false, message = "Invalid email or password." });

                    UserDb? userNode = await _firebaseClient
                        .Child("users")
                        .Child(userRecord.Uid)
                        .OnceSingleAsync<UserDb?>();

                    if (userNode == null)
                        return Unauthorized(new { success = false, message = "User not registered in system." });

                    actualRole = userNode!.Role ?? "User";
                }
                catch (Exception ex)
                {
                    return BadRequest(new { success = false, message = ex.Message });
                }
            }

            // Role validation
            if (selectedRole == "Administrator" && actualRole != "Admin")
            {
                return BadRequest(new { success = false, message = "Incorrect role. You are not authorized as Administrator." });
            }
            if (selectedRole == "User" && actualRole != "User")
            {
                return BadRequest(new { success = false, message = "Incorrect role. You are not authorized as a regular User." });
            }

            await SignInUser(model.Email, actualRole);

            // ✅ CHANGED: Always redirect to Dashboard (instead of Donations or Donor)
            string redirectUrl = Url.Action("Index", "Dashboard");
            return Ok(new { success = true, redirectUrl });
        }

        private async Task<bool> VerifyPasswordWithFirebase(string email, string password)
        {
            if (string.IsNullOrEmpty(_firebaseApiKey)) return false;
            using var client = new HttpClient();
            var response = await client.PostAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_firebaseApiKey}",
                new StringContent($"{{\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}",
                System.Text.Encoding.UTF8, "application/json"));
            return response.IsSuccessStatusCode;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterModel? model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Invalid data." });

            if (string.IsNullOrWhiteSpace(model.Email) || !IsValidEmail(model.Email))
                return BadRequest(new { success = false, message = "Invalid email format." });
            if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 6)
                return BadRequest(new { success = false, message = "Password must be at least 6 characters." });
            if (model.Password != model.ConfirmPassword)
                return BadRequest(new { success = false, message = "Passwords do not match." });

            try
            {
                var userRecord = await _authService.CreateUserAsync(model.Email, model.Password, model.FullName ?? "User");

                var userDb = new UserDb
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Location = model.Location,
                    Role = "User",
                    CreatedAt = DateTime.UtcNow
                };
                await _firebaseClient.Child("users").Child(userRecord.Uid).PutAsync(userDb);

                await SignInUser(model.Email, "User");

                // ✅ CHANGED: Redirect to Dashboard after registration
                return Ok(new { success = true, redirectUrl = Url.Action("Index", "Dashboard") });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();

        [HttpGet] public IActionResult ForgotPassword() => View();
        [HttpPost] public IActionResult ForgotPassword(string email) { ViewBag.Message = "Reset link sent (if email exists)."; return View(); }

        private async Task SignInUser(string email, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }

        private bool IsValidEmail(string email) => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    public class LoginModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? SelectedRole { get; set; }
    }

    public class RegisterModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
    }

    public class UserDb
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}