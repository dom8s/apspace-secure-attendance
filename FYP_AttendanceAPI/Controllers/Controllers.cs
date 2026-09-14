// Controllers/AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FYP_AttendanceAPI.DTOs;
using FYP_AttendanceAPI.Services;
using FYP_AttendanceAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace FYP_AttendanceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TPNumber) ||
                string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "TP Number and password are required." });

            var result = await _auth.LoginAsync(request);

            if (result is null)
                return Unauthorized(new { message = "Invalid TP number or password." });

            return Ok(result);
        }
    }
}

// Controllers/AttendanceController.cs
namespace FYP_AttendanceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendance;
        public AttendanceController(IAttendanceService attendance) => _attendance = attendance;

        [HttpPost("submit")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit([FromBody] SubmitAttendanceRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SessionCode) ||
                request.SessionCode.Length != 3)
                return BadRequest(new { message = "A valid 3-character session code is required." });

            if (string.IsNullOrWhiteSpace(request.DeviceFingerprint))
                return BadRequest(new { message = "Device fingerprint is required." });

            int studentId = GetCurrentUserId();
            var result = await _attendance.SubmitAttendanceAsync(studentId, request);

            if (!result.Success)
                return StatusCode(403, result);

            return Ok(result);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyAttendance()
        {
            int studentId = GetCurrentUserId();
            var records = await _attendance.GetStudentAttendanceAsync(studentId);
            return Ok(records);
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}

// Controllers/SessionController.cs
namespace FYP_AttendanceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _session;
        public SessionController(ISessionService session) => _session = session;

        [HttpPost("create")]
        [Authorize(Roles = "Lecturer,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSessionRequest request)
        {
            int lecturerId = GetCurrentUserId();
            var result = await _session.CreateSessionAsync(lecturerId, request);
            return Ok(result);
        }

        [HttpGet("{sessionId:int}")]
        [Authorize(Roles = "Lecturer,Admin")]
        public async Task<IActionResult> GetDetail(int sessionId)
        {
            var result = await _session.GetSessionDetailAsync(sessionId);
            if (result is null) return NotFound(new { message = "Session not found." });
            return Ok(result);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Lecturer,Admin")]
        public async Task<IActionResult> GetMySessions()
        {
            int lecturerId = GetCurrentUserId();
            var sessions = await _session.GetLecturerSessionsAsync(lecturerId);
            return Ok(sessions);
        }

        [HttpPut("{sessionId:int}/close")]
        [Authorize(Roles = "Lecturer,Admin")]
        public async Task<IActionResult> Close(int sessionId)
        {
            int lecturerId = GetCurrentUserId();
            var success = await _session.CloseSessionAsync(sessionId, lecturerId);
            if (!success) return NotFound(new { message = "Session not found or not yours." });
            return Ok(new { message = "Session closed successfully." });
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}

// Controllers/AdminController.cs
namespace FYP_AttendanceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _admin;
        public AdminController(IAdminService admin) => _admin = admin;

        [HttpGet("flagged")]
        public async Task<IActionResult> GetFlagged()
        {
            var result = await _admin.GetFlaggedAttendanceAsync();
            return Ok(result);
        }

        [HttpGet("devicelogs")]
        public async Task<IActionResult> GetDeviceLogs([FromQuery] bool flaggedOnly = false)
        {
            var result = await _admin.GetDeviceLogsAsync(flaggedOnly);
            return Ok(result);
        }

        [HttpPut("devicelogs/review")]
        public async Task<IActionResult> ReviewDeviceLog([FromBody] ReviewDeviceLogRequest request)
        {
            var success = await _admin.ReviewDeviceLogAsync(request);
            if (!success) return NotFound(new { message = "Log entry not found." });
            return Ok(new { message = "Log reviewed successfully." });
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _admin.GetAttendanceSummaryAsync();
            return Ok(result);
        }
    }
}

namespace FYP_AttendanceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly AppDbContext _db;
        public DebugController(AppDbContext db) => _db = db;

        /// Generates a real BCrypt hash. GET /api/debug/hash?password=Lecturer@123
        [HttpGet("hash")]
        public IActionResult GetHash([FromQuery] string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return BadRequest("Add ?password=YourPassword to the URL.");

            var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
            return Ok(new { password, hash, hashLength = hash.Length });
        }

        /// Verifies password against DB hash. POST /api/debug/verify
        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyRequest req)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.TPNumber == req.TPNumber);

            if (user is null)
                return NotFound(new { message = $"{req.TPNumber} not found." });

            bool valid = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);

            return Ok(new
            {
                tpNumber = user.TPNumber,
                role = user.Role,
                passwordValid = valid,
                hashInDb = user.PasswordHash,
                hashLength = user.PasswordHash.Length,
                result = valid ? "BCrypt OK - login will work"
                                      : "BCrypt FAILED - hash in DB is wrong"
            });
        }

        public class VerifyRequest
        {
            public string TPNumber { get; set; } = "";
            public string Password { get; set; } = "";
        }

        /// Shows what Authorization header the server receives.
        /// GET /api/debug/authheader
        [HttpGet("authheader")]
        [AllowAnonymous]
        public IActionResult CheckAuthHeader()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            return Ok(new
            {
                received = authHeader,
                isEmpty = string.IsNullOrEmpty(authHeader),
                startsWithBearer = authHeader.StartsWith("Bearer "),
                tokenPartLength = authHeader.Length > 7 ? authHeader.Substring(7).Length : 0
            });
        }

    }
}