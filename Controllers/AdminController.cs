using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SarEquipEnterprise.Data;
using SarEquipEnterprise.Models;

namespace SarEquipEnterprise.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly BillingSystemDbContext _context;

        public AdminController(BillingSystemDbContext context)
        {
            _context = context;
        }

        // GET: Admin/CreateUser
        public IActionResult CreateUser()
        {
            var model = new AdminCreateUserModel();
            return View(model);
        }

        // POST: Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(AdminCreateUserModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // check duplicate email
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("", "Email already registered");
                return View(model);
            }

            var user = new User
            {
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                Role = model.Role,
                FullName = model.FullName,
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "User created successfully.";
            return RedirectToAction(nameof(Users));
        }

        // Password hashing (same approach used in AccountController)
        private string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                10000,
                System.Security.Cryptography.HashAlgorithmName.SHA256,
                20);

            byte[] hashWithSalt = new byte[36];
            Array.Copy(salt, 0, hashWithSalt, 0, 16);
            Array.Copy(hash, 0, hashWithSalt, 16, 20);

            return Convert.ToBase64String(hashWithSalt);
        }

        // User Management - List all users
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                                      .Where(u => !u.IsArchived)
                                      .OrderByDescending(u => u.CreatedDate)
                                      .ToListAsync();
            return View(users);
        }

        // Edit user role
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, [Bind("UserId,Email,Role,IsActive,FullName")] User user)
        {
            if (id != user.UserId)
                return NotFound();

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return NotFound();

            // Prevent downgrading the last SuperAdmin
            if (existingUser.Role == UserRole.SuperAdmin && user.Role != UserRole.SuperAdmin)
            {
                var superAdminCount = await _context.Users.CountAsync(u => u.Role == UserRole.SuperAdmin);
                if (superAdminCount == 1)
                {
                    ModelState.AddModelError("", "Cannot downgrade the last Super Admin. There must be at least one Super Admin account.");
                    return View(user);
                }
            }

            existingUser.Role = user.Role;
            existingUser.IsActive = user.IsActive;
            existingUser.FullName = user.FullName;

            try
            {
                _context.Update(existingUser);
                await _context.SaveChangesAsync();
                TempData["Success"] = "User updated successfully.";
                return RedirectToAction(nameof(Users));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again later.");
                return View(user);
            }
        }

        // Deactivate user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return Json(new { success = false, message = "User not found" });

            // Prevent deactivating the last SuperAdmin
            if (user.Role == UserRole.SuperAdmin && user.IsActive)
            {
                var activeSuperAdmins = await _context.Users
                    .Where(u => u.Role == UserRole.SuperAdmin && u.IsActive)
                    .CountAsync();

                if (activeSuperAdmins == 1)
                    return Json(new { success = false, message = "Cannot deactivate the last active Super Admin." });
            }

            user.IsActive = !user.IsActive;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"User {(user.IsActive ? "activated" : "deactivated")} successfully." });
        }

        // Archive user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return Json(new { success = false, message = "User not found" });

            // Prevent archiving the last SuperAdmin
            if (user.Role == UserRole.SuperAdmin)
            {
                var superAdminCount = await _context.Users.CountAsync(u => u.Role == UserRole.SuperAdmin && !u.IsArchived);
                if (superAdminCount == 1)
                    return Json(new { success = false, message = "Cannot archive the last Super Admin account." });
            }

            user.IsArchived = true;
            user.IsActive = false; // Deactivate upon archiving
            _context.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "User archived successfully." });
        }

        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var activeUsersBase = _context.Users.Where(u => !u.IsArchived);
            var totalUsers = await activeUsersBase.CountAsync();
            var activeUsers = await activeUsersBase.CountAsync(u => u.IsActive);
            var admins = await activeUsersBase.CountAsync(u => u.Role == UserRole.Admin);
            var employees = await activeUsersBase.CountAsync(u => u.Role == UserRole.Employee);
            var superAdmins = await activeUsersBase.CountAsync(u => u.Role == UserRole.SuperAdmin);

            ViewBag.TotalUsers = totalUsers;
            ViewBag.ActiveUsers = activeUsers;
            ViewBag.Admins = admins;
            ViewBag.Employees = employees;
            ViewBag.SuperAdmins = superAdmins;

            return View();
        }
    }
}
