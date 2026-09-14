// Services/SessionService.cs
using Microsoft.EntityFrameworkCore;
using FYP_AttendanceAPI.Data;
using FYP_AttendanceAPI.DTOs;
using FYP_AttendanceAPI.Models;

namespace FYP_AttendanceAPI.Services
{
    public class SessionService : ISessionService
    {
        private readonly AppDbContext    _db;
        private readonly IConfiguration _config;

        public SessionService(AppDbContext db, IConfiguration config)
        {
            _db     = db;
            _config = config;
        }

        //  Create a new attendance session 
        public async Task<SessionDto> CreateSessionAsync(
            int lecturerId, CreateSessionRequest request)
        {
            // Expire any previously active sessions for this subject
            var activeSessions = await _db.Sessions
                .Where(s => s.SubjectId == request.SubjectId &&
                            s.LecturerId == lecturerId &&
                            s.IsActive)
                .ToListAsync();

            foreach (var old in activeSessions)
                old.IsActive = false;

            // Generate unique 3-digit code (e.g. "4K7")
            string code = GenerateSessionCode();

            int expiryMins = int.Parse(
                _config["AttendanceSettings:SessionExpiryMinutes"] ?? "15");

            var session = new Session
            {
                SubjectId   = request.SubjectId,
                LecturerId  = lecturerId,
                SessionCode = code,
                CreatedAt   = DateTime.UtcNow,
                ExpiresAt   = DateTime.UtcNow.AddMinutes(expiryMins),
                IsActive    = true
            };

            _db.Sessions.Add(session);
            await _db.SaveChangesAsync();

            var subject = await _db.Subjects.FindAsync(request.SubjectId);

            return new SessionDto
            {
                SessionId   = session.Id,
                SessionCode = session.SessionCode,
                SubjectCode = subject?.SubjectCode ?? "",
                SubjectName = subject?.SubjectName ?? "",
                CreatedAt   = session.CreatedAt,
                ExpiresAt   = session.ExpiresAt,
                IsActive    = session.IsActive
            };
        }

        //  Get full session detail including attendees 
        public async Task<SessionDetailDto?> GetSessionDetailAsync(int sessionId)
        {
            var session = await _db.Sessions
                .Include(s => s.Subject)
                .Include(s => s.Attendances)
                    .ThenInclude(a => a.Student)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session is null) return null;

            return new SessionDetailDto
            {
                SessionId   = session.Id,
                SessionCode = session.SessionCode,
                SubjectCode = session.Subject.SubjectCode,
                SubjectName = session.Subject.SubjectName,
                CreatedAt   = session.CreatedAt,
                ExpiresAt   = session.ExpiresAt,
                IsActive    = session.IsActive,
                Attendees   = session.Attendances.Select(a => new SessionAttendeeDto
                {
                    TPNumber    = a.Student.TPNumber,
                    FullName    = a.Student.FullName,
                    Status      = a.Status,
                    SubmittedAt = a.SubmittedAt
                }).ToList()
            };
        }

        //  Get all sessions created by a lecturer 
        public async Task<List<SessionDto>> GetLecturerSessionsAsync(int lecturerId)
        {
            return await _db.Sessions
                .Include(s => s.Subject)
                .Where(s => s.LecturerId == lecturerId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new SessionDto
                {
                    SessionId   = s.Id,
                    SessionCode = s.SessionCode,
                    SubjectCode = s.Subject.SubjectCode,
                    SubjectName = s.Subject.SubjectName,
                    CreatedAt   = s.CreatedAt,
                    ExpiresAt   = s.ExpiresAt,
                    IsActive    = s.IsActive
                })
                .ToListAsync();
        }

        //  Close/deactivate a session 
        public async Task<bool> CloseSessionAsync(int sessionId, int lecturerId)
        {
            var session = await _db.Sessions
                .FirstOrDefaultAsync(s => s.Id == sessionId &&
                                          s.LecturerId == lecturerId);

            if (session is null) return false;

            session.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        //  Private: Generate a random 3-character alphanumeric code 
        private static string GenerateSessionCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no ambiguous chars
            var random = new Random();
            return new string(Enumerable.Range(0, 3)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
        }
    }
}

// Services/AdminService.cs
namespace FYP_AttendanceAPI.Services
{
    using Microsoft.EntityFrameworkCore;
    using FYP_AttendanceAPI.Data;
    using FYP_AttendanceAPI.DTOs;

    public class AdminService : IAdminService
    {
        private readonly AppDbContext _db;

        public AdminService(AppDbContext db) => _db = db;

        //  Get all flagged and rejected attendance records 
        public async Task<List<FlaggedAttendanceDto>> GetFlaggedAttendanceAsync()
        {
            return await _db.Attendances
                .Include(a => a.Student)
                .Include(a => a.Session)
                    .ThenInclude(s => s.Subject)
                .Where(a => a.Status == "Flagged" || a.Status == "Rejected")
                .OrderByDescending(a => a.SubmittedAt)
                .Select(a => new FlaggedAttendanceDto
                {
                    AttendanceId    = a.Id,
                    TPNumber        = a.Student.TPNumber,
                    StudentName     = a.Student.FullName,
                    SubjectCode     = a.Session.Subject.SubjectCode,
                    SubjectName     = a.Session.Subject.SubjectName,
                    SessionCode     = a.Session.SessionCode,
                    SessionDate     = a.Session.CreatedAt,
                    WifiSSID        = a.WifiSSID,
                    IPAddress       = a.IPAddress,
                    Status          = a.Status,
                    RejectionReason = a.RejectionReason,
                    SubmittedAt     = a.SubmittedAt
                })
                .ToListAsync();
        }

        //  Get device logs (optionally filter by flagged only) 
        public async Task<List<DeviceLogDto>> GetDeviceLogsAsync(bool flaggedOnly = false)
        {
            var query = _db.DeviceLogs
                .Include(d => d.Student)
                .Include(d => d.Session)
                    .ThenInclude(s => s.Subject)
                .AsQueryable();

            if (flaggedOnly)
                query = query.Where(d => d.IsFlagged);

            return await query
                .OrderByDescending(d => d.LoggedAt)
                .Select(d => new DeviceLogDto
                {
                    LogId             = d.Id,
                    DeviceFingerprint = d.DeviceFingerprint,
                    TPNumber          = d.Student.TPNumber,
                    StudentName       = d.Student.FullName,
                    SubjectCode       = d.Session.Subject.SubjectCode,
                    SessionCode       = d.Session.SessionCode,
                    SessionDate       = d.Session.CreatedAt,
                    IsFlagged         = d.IsFlagged,
                    FlagReason        = d.FlagReason,
                    ReviewedByAdmin   = d.ReviewedByAdmin,
                    AdminNotes        = d.AdminNotes,
                    LoggedAt          = d.LoggedAt
                })
                .ToListAsync();
        }

        //  Admin marks a device log as reviewed 
        public async Task<bool> ReviewDeviceLogAsync(ReviewDeviceLogRequest request)
        {
            var log = await _db.DeviceLogs.FindAsync(request.LogId);
            if (log is null) return false;

            log.ReviewedByAdmin = true;
            log.AdminNotes      = request.AdminNotes;
            await _db.SaveChangesAsync();
            return true;
        }

        //  Get attendance summary per student per subject 
        public async Task<List<AttendanceSummaryDto>> GetAttendanceSummaryAsync()
        {
            return await _db.Attendances
                .Include(a => a.Student)
                .Include(a => a.Session)
                    .ThenInclude(s => s.Subject)
                .GroupBy(a => new
                {
                    a.Student.TPNumber,
                    a.Student.FullName,
                    a.Session.Subject.SubjectCode,
                    a.Session.Subject.SubjectName
                })
                .Select(g => new AttendanceSummaryDto
                {
                    TPNumber        = g.Key.TPNumber,
                    FullName        = g.Key.FullName,
                    SubjectCode     = g.Key.SubjectCode,
                    SubjectName     = g.Key.SubjectName,
                    TotalSubmissions = g.Count(),
                    Accepted        = g.Count(a => a.Status == "Accepted"),
                    Flagged         = g.Count(a => a.Status == "Flagged"),
                    Rejected        = g.Count(a => a.Status == "Rejected")
                })
                .ToListAsync();
        }
    }
}
