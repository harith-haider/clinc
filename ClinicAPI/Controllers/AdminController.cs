using ClinicAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace ClinicAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // تم تفعيل القفل بنجاح!
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. جلب قائمة المرضى المعلقين (الذين يحتاجون موافقة)
        [HttpGet("pending-patients")]
        public async Task<IActionResult> GetPendingPatients()
        {
            var patients = await _context.Patients
                .Where(p => !p.IsApproved)
                .Select(p => new { p.Id, p.FirstName, p.LastName, p.Email, p.Phone }) // نرسل البيانات الأساسية فقط
                .ToListAsync();

            return Ok(patients);
        }

        // 2. الموافقة على حساب مريض
        [HttpPut("approve-patient/{id}")]
        public async Task<IActionResult> ApprovePatient(Guid id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return NotFound("عفواً، الحساب غير موجود.");

            patient.IsApproved = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تفعيل حساب المريض بنجاح!" });
        }

        // 3. جلب قائمة الأطباء المعلقين
        [HttpGet("pending-dentists")]
        public async Task<IActionResult> GetPendingDentists()
        {
            var dentists = await _context.Dentists
                .Where(d => !d.IsApproved)
                .Select(d => new { d.Id, d.FirstName, d.LastName, d.Email, d.Specialty })
                .ToListAsync();

            return Ok(dentists);
        }

        // 4. الموافقة على حساب طبيب
        [HttpPut("approve-dentist/{id}")]
        public async Task<IActionResult> ApproveDentist(Guid id)
        {
            var dentist = await _context.Dentists.FindAsync(id);
            if (dentist == null)
                return NotFound("عفواً، حساب الطبيب غير موجود.");

            dentist.IsApproved = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تفعيل حساب الطبيب بنجاح!" });
        }

        // 5. إحصائيات لوحة التحكم (Dashboard)
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            // إجمالي عدد المرضى في النظام
            var totalPatients = await _context.Patients.CountAsync();

            // إجمالي الأرباح (مجموع مبالغ الفواتير المدفوعة فقط)
            var totalRevenue = await _context.Invoices
                .Where(i => i.IsPaid)
                .SumAsync(i => i.Amount);

            // عدد مواعيد اليوم
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var todayAppointmentsCount = await _context.Appointments
                .Where(a => a.StartTime >= today && a.StartTime < tomorrow)
                .CountAsync();

            // تجميع البيانات في كائن واحد لإرسالها للواجهة
            var stats = new
            {
                TotalPatients = totalPatients,
                TotalRevenue = totalRevenue,
                TodayAppointments = todayAppointmentsCount
            };

            return Ok(stats);
        }
    }
}