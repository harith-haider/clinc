using ClinicAPI.Data;
using ClinicAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ClinicAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. تسجيل حساب مريض جديد (يحتاج موافقة)
        [HttpPost("register")]
        public async Task<IActionResult> RegisterPatient([FromBody] RegisterDto request)
        {
            if (await _context.Patients.AnyAsync(p => p.Email == request.Email))
                return BadRequest("البريد الإلكتروني مستخدم بالفعل.");

            var newPatient = new Patient
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "Patient",
                IsApproved = false,
                MedicalHistory = ""
            };

            _context.Patients.Add(newPatient);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم إنشاء حسابك بنجاح! يرجى الانتظار حتى توافق الإدارة عليه لتتمكن من الدخول." });
        }

        // 2. تسجيل حساب طبيب جديد (يحتاج موافقة الإدمن)
        [HttpPost("register-dentist")]
        public async Task<IActionResult> RegisterDentist([FromBody] RegisterDto request)
        {
            if (await _context.Dentists.AnyAsync(d => d.Email == request.Email))
                return BadRequest("البريد الإلكتروني مستخدم بالفعل للطبيب.");

            var newDentist = new Dentist
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "Dentist",
                IsApproved = false,
                Specialty = ""
            };

            _context.Dentists.Add(newDentist);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم إنشاء حساب الطبيب بنجاح! يرجى الانتظار حتى توافق الإدارة عليه لتتمكن من الدخول." });
        }

        // 3. مسار خاص لإنشاء أول مدير للنظام (يعمل مرة واحدة فقط)
        [HttpPost("setup-admin")]
        public async Task<IActionResult> SetupAdmin([FromBody] LoginDto request)
        {
            // منع إنشاء أكثر من مدير بهذه الطريقة لحماية النظام
            if (await _context.Admins.AnyAsync())
                return BadRequest("يوجد مدير بالفعل في النظام. تم إغلاق هذا المسار للأمان.");

            var newAdmin = new Admin
            {
                Id = Guid.NewGuid(),
                Name = "مدير النظام",
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "Admin"
            };

            _context.Admins.Add(newAdmin);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم إنشاء حساب المدير بنجاح! يمكنك الآن تسجيل الدخول." });
        }

        // 4. تسجيل الدخول (للمرضى، الأطباء، والمدراء)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            // أ. البحث في جدول المرضى أولاً
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Email == request.Email);
            if (patient != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Password, patient.PasswordHash))
                    return Unauthorized("كلمة المرور غير صحيحة.");

                if (!patient.IsApproved)
                    return StatusCode(403, new { message = "حسابك قيد المراجعة ولم يتم تفعيله بعد." });

                var token = GenerateJwtToken(patient.Id, patient.Email, patient.Role);
                return Ok(new { token, role = patient.Role, message = "تم تسجيل الدخول كـ مريض بنجاح!" });
            }

            // ب. إذا لم يكن مريضاً، نبحث في جدول الأطباء
            var dentist = await _context.Dentists.FirstOrDefaultAsync(d => d.Email == request.Email);
            if (dentist != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Password, dentist.PasswordHash))
                    return Unauthorized("كلمة المرور غير صحيحة.");

                if (!dentist.IsApproved)
                    return StatusCode(403, new { message = "حساب الطبيب قيد المراجعة ولم يتم تفعيله بعد." });

                var token = GenerateJwtToken(dentist.Id, dentist.Email, dentist.Role);
                return Ok(new { token, role = dentist.Role, message = "تم تسجيل الدخول كـ طبيب بنجاح!" });
            }

            // ج. إذا لم يكن طبيباً، نبحث في جدول المدراء
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == request.Email);
            if (admin != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
                    return Unauthorized("كلمة المرور غير صحيحة.");

                var token = GenerateJwtToken(admin.Id, admin.Email, admin.Role);
                return Ok(new { token, role = admin.Role, message = "تم تسجيل الدخول كـ مدير بنجاح!" });
            }

            return Unauthorized("البريد الإلكتروني غير مسجل في النظام.");
        }

        // دالة مساعدة لتوليد توكن الحماية (JWT)
        private string GenerateJwtToken(Guid id, string email, string role)
        {
            var jwtKey = "ThisIsAMySuperSecretKeyForClinicApp2026!@#";
            var key = Encoding.ASCII.GetBytes(jwtKey);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    // DTOs لاستقبال البيانات من واجهة React
    public class RegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}