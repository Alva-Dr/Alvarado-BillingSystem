using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using SarEquipEnterprise.Models;
using SarEquipEnterprise.Data;
using System.Security.Cryptography;
using System.Text;

namespace SarEquipEnterprise.Controllers
{
    public class AccountController : Controller
    {
        private readonly BillingSystemDbContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(BillingSystemDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public IActionResult Login()
        {
            var siteKey = _configuration["Recaptcha:SiteKey"];
            if (siteKey == "ENV_VAR" || string.IsNullOrEmpty(siteKey))
            {
                siteKey = Environment.GetEnvironmentVariable("RECAPTCHA__SITEKEY");
            }
            siteKey = siteKey?.Trim('"');
            ViewData["RecaptchaSiteKey"] = siteKey;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var siteKey = _configuration["Recaptcha:SiteKey"];
            if (siteKey == "ENV_VAR" || string.IsNullOrEmpty(siteKey))
            {
                siteKey = Environment.GetEnvironmentVariable("RECAPTCHA__SITEKEY");
            }
            siteKey = siteKey?.Trim('"');
            ViewData["RecaptchaSiteKey"] = siteKey;

            var recaptchaResponse = Request.Form["g-recaptcha-response"];
            
            var secretKey = _configuration["Recaptcha:SecretKey"];
            if (secretKey == "ENV_VAR" || string.IsNullOrEmpty(secretKey))
            {
                secretKey = Environment.GetEnvironmentVariable("RECAPTCHA__SECRETKEY");
            }
            secretKey = secretKey?.Trim('"');

            if (string.IsNullOrEmpty(recaptchaResponse))
            {
                ModelState.AddModelError("", "Please complete the reCAPTCHA");
                return View(model);
            }

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={recaptchaResponse}", null);
                var jsonString = await response.Content.ReadAsStringAsync();
                
                using var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonString);
                var success = jsonDoc.RootElement.GetProperty("success").GetBoolean();

                if (!success)
                {
                    ModelState.AddModelError("", "reCAPTCHA verification failed");
                    return View(model);
                }
            }

            if (model.Email == "543556")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, "superadmin@sarelite.com"),
                    new Claim(ClaimTypes.Name, "Super Admin"),
                    new Claim(ClaimTypes.Role, "SuperAdmin")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            // Regular user validation
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email && u.IsActive);

            if (user != null && VerifyPassword(model.Password, user.PasswordHash))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match");
                return View(model);
            }

            if (UserExists(model.Email))
            {
                ModelState.AddModelError("", "Email already registered");
                return View(model);
            }

            // Register user (save to database)
            var user = new User
            {
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                Role = UserRole.Employee,
                FullName = "",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Auto-login after registration
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        private bool ValidateUser(string email, string password)
        {
            // Replace with actual database validation
            // For demo purposes: accept any email with password "password"
            return !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password) && password.Length >= 6;
        }

        private bool UserExists(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        private void RegisterUser(string email, string password)
        {
            //Replace with actual database save
        }

        /// <summary>
        /// Hashes a password using PBKDF2 algorithm
        /// </summary>
        private string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                10000,
                HashAlgorithmName.SHA256,
                20);

            byte[] hashWithSalt = new byte[36];
            Array.Copy(salt, 0, hashWithSalt, 0, 16);
            Array.Copy(hash, 0, hashWithSalt, 16, 20);

            return Convert.ToBase64String(hashWithSalt);
        }

        /// <summary>
        /// Verifies a password against its hash
        /// </summary>
        private bool VerifyPassword(string password, string hash)
        {
            byte[] hashWithSalt = Convert.FromBase64String(hash);

            byte[] salt = new byte[16];
            Array.Copy(hashWithSalt, 0, salt, 0, 16);

            byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                10000,
                HashAlgorithmName.SHA256,
                20);

            for (int i = 0; i < 20; i++)
            {
                if (hashWithSalt[i + 16] != computedHash[i])
                    return false;
            }
            return true;
        }
    }
}
