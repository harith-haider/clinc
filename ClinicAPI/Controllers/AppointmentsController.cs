using ClinicAPI.Data;
using ClinicAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. جلب جميع الحجوزات مع بيانات المريض والطبيب
        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Dentist)
                .Include(a => a.Service)
                .Select(a => new
                {
                    a.Id,
                    PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                    DentistName = a.Dentist.FirstName + " " + a.Dentist.LastName,
                    ServiceName = a.Service.Name,
                    a.StartTime,
                    a.EndTime,
                    a.Status,
                    a.Notes
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // 2. إنشاء حجز جديد مع فحص التعارض (Overlap Validation)
        [HttpPost]
        public async Task<IActionResult> BookAppointment([FromBody] AppointmentCreateDto request)
        {
            // التحقق من وجود الخدمة المطلوبة لمعرفة مدتها
            var service = await _context.Services.FindAsync(request.ServiceId);
            if (service == null)
                return BadRequest("الخدمة العلاجية غير موجودة.");

            // حساب وقت الانتهاء بناءً على مدة الخدمة
            var calculatedEndTime = request.StartTime.AddMinutes(service.DurationMinutes);

            // خوارزمية فحص تعارض المواعيد للطبيب
            bool isOverlapping = await _context.Appointments
                .AnyAsync(a => a.DentistId == request.DentistId
                            && a.Status != "Cancelled" // تجاهل المواعيد الملغاة
                            && a.StartTime < calculatedEndTime
                            && a.EndTime > request.StartTime);

            if (isOverlapping)
            {
                return Conflict(new { message = "يوجد تعارض! الطبيب لديه حجز آخر في هذا الوقت." });
            }

            // إنشاء الحجز الجديد
            var newAppointment = new Appointment
            {
                PatientId = request.PatientId,
                DentistId = request.DentistId,
                ServiceId = request.ServiceId,
                StartTime = request.StartTime,
                EndTime = calculatedEndTime,
                Status = "Scheduled",
                Notes = "" // <-- تم إضافة هذا السطر لحل المشكلة
            };

            _context.Appointments.Add(newAppointment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAppointments), new { id = newAppointment.Id }, newAppointment);
        }
    }

    // DTO: كلاس بسيط لاستقبال البيانات المطلوبة فقط من الفرونت اند
    public class AppointmentCreateDto
    {
        public Guid PatientId { get; set; }
        public Guid DentistId { get; set; }
        public Guid ServiceId { get; set; }
        public DateTime StartTime { get; set; }
    }
}