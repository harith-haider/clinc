using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAPI.Models
{
    public class Invoice
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // علاقة الربط مع الحجز
        public Guid AppointmentId { get; set; }
        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; }

        // تم إضافة علاقة الربط مع المريض (لأن الكنترولر يحتاجه)
        public Guid PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }

        [Required]
        public decimal Amount { get; set; } // تم التعديل لتطابق الكنترولر

        [Required]
        public bool IsPaid { get; set; } = false; // تم التعديل لتطابق الكنترولر

        public DateTime IssueDate { get; set; } = DateTime.UtcNow; // تم التعديل لتطابق الكنترولر

        // حقل ممتاز لمعرفة تاريخ الدفع الفعلي (يمكن أن يكون Null)
        public DateTime? PaidAt { get; set; }
    }
}