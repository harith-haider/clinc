using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAPI.Models
{
    public class Appointment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // علاقة الربط مع المريض
        public Guid PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }

        // علاقة الربط مع الطبيب
        public Guid DentistId { get; set; }
        [ForeignKey("DentistId")]
        public Dentist Dentist { get; set; }

        // علاقة الربط مع الخدمة
        public Guid ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public Service Service { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } // Scheduled, Completed, Cancelled

        public string Notes { get; set; }
    }
}