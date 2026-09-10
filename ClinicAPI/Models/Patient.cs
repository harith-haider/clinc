using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicAPI.Models
{
    public class Patient
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string FirstName { get; set; }

        [Required, MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required, MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(255)]
        public string Email { get; set; }

        // يمكن تخزين السجل الطبي كـ JSON String
        public string MedicalHistory { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // حقول المصادقة والصلاحيات
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "Patient";
        public bool IsApproved { get; set; } = false;
    }

}