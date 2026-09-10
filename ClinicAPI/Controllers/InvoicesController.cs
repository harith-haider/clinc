using ClinicAPI.Data;
using ClinicAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ClinicAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InvoicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. توليد فاتورة بناءً على رقم الحجز
        [HttpPost("generate/{appointmentId}")]
        public async Task<IActionResult> GenerateInvoice(Guid appointmentId)
        {
            // جلب الحجز مع بيانات الخدمة لمعرفة السعر
            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                return NotFound("عفواً، الحجز غير موجود.");

            // التأكد من عدم إصدار فاتورة مسبقاً لنفس الحجز لمنع التكرار
            bool invoiceExists = await _context.Invoices.AnyAsync(i => i.AppointmentId == appointmentId);
            if (invoiceExists)
                return BadRequest("تم إصدار فاتورة لهذا الحجز مسبقاً.");

            // إنشاء الفاتورة الجديدة
            var newInvoice = new Invoice
            {
                Id = Guid.NewGuid(),
                AppointmentId = appointment.Id,
                PatientId = appointment.PatientId,
                Amount = appointment.Service.Price, // سحب السعر تلقائياً من الخدمة
                IssueDate = DateTime.UtcNow,
                IsPaid = false // الفاتورة غير مدفوعة افتراضياً
            };

            _context.Invoices.Add(newInvoice);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم إصدار الفاتورة بنجاح!", invoice = newInvoice });
        }

        // 2. دفع الفاتورة
        [HttpPut("pay/{invoiceId}")]
        public async Task<IActionResult> PayInvoice(Guid invoiceId)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null)
                return NotFound("الفاتورة غير موجودة.");

            if (invoice.IsPaid)
                return BadRequest("الفاتورة مدفوعة مسبقاً.");

            invoice.IsPaid = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تسديد الفاتورة بنجاح!" });
        }
    }
}