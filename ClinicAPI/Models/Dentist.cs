using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicAPI.Models
{
    public class Dentist
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string FirstName { get; set; }

        [Required, MaxLength(100)]
        public string LastName { get; set; }

        [Required, MaxLength(100)]
        public string Specialty { get; set; }

        [Required, MaxLength(20)]
        public string Phone { get; set; }

        [Required, MaxLength(255)]
        public string Email { get; set; }

        public bool IsActive { get; set; } = true;
        // حقول المصادقة والصلاحيات
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "Dentist";
        public bool IsApproved { get; set; } = false;
    }


}