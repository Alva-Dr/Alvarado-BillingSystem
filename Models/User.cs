using System.ComponentModel.DataAnnotations;

namespace SarEquipEnterprise.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(256)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        public UserRole Role { get; set; } = UserRole.Employee;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
        
        public bool IsArchived { get; set; } = false;

        [StringLength(256)]
        public string? FullName { get; set; }
    }

    public enum UserRole
    {
        Employee = 0,
        Admin = 1,
        SuperAdmin = 2
    }
}
