using Microsoft.EntityFrameworkCore;
using FYP_AttendanceAPI.Data;
using FYP_AttendanceAPI.DTOs;
using FYP_AttendanceAPI.Models;

namespace FYP_AttendanceAPI.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext    _db;
        private readonly IConfiguration _config;

        public AttendanceService(AppDbContext db, IConfiguration config)
        {
            _db     = db;
            _config = config;
        }

        // MAIN: Submit Attendance
        public async Task<SubmitAttendanceResponse> SubmitAttendanceAsync(
            int studentId,
            SubmitAttendanceRequest request)
        {
            //  STEP 1: Validate session code 
            var session = await _db.Sessions
                .Include(s => s.Subject)
                .FirstOrDefaultAsync(s =>
                    s.SessionCode == request.SessionCode &&
                    s.IsActive &&
                    s.ExpiresAt > DateTime.UtcNow);

            if (session is null)
            {
                return new SubmitAttendanceResponse
                {
                    Success  = false,
                    Status   = "Rejected",
                    Message  = "Invalid or expired session code.",
                    SubmittedAt = DateTime.UtcNow
                };
            }

            //  STEP 2: Check student is enrolled in this subject 
            var isEnrolled = await _db.StudentSubjects
                .AnyAsync(ss => ss.StudentId == studentId &&
                                ss.SubjectId == session.SubjectId);

            if (!isEnrolled)
            {
                return new SubmitAttendanceResponse
                {
                    Success  = false,
                    Status   = "Rejected",
                    Message  = "You are not enrolled in this subject.",
                    SubmittedAt = DateTime.UtcNow
                };
            }

            //  STEP 3: Check for duplicate submission 
            var alreadySubmitted = await _db.Attendances
                .AnyAsync(a => a.StudentId == studentId &&
                               a.SessionId == session.Id);

            if (alreadySubmitted)
            {
                return new SubmitAttendanceResponse
                {
                    Success  = false,
                    Status   = "Rejected",
                    Message  = "Attendance already recorded for this session.",
                    SubmittedAt = DateTime.UtcNow
                };
            }

            //  STEP 4: Wi-Fi SSID Validation 
            //    Student must be connected to authorised APU Wi-Fi
            var wifiValid = await ValidateWifiAsync(request.WifiSSID, request.IPAddress);

            if (!wifiValid)
            {
                await LogAndSaveAttendanceAsync(
                    studentId, session.Id,
                    request.DeviceFingerprint,
                    request.WifiSSID, request.IPAddress,
                    "Rejected",
                    $"Not connected to authorised Wi-Fi. Detected: '{request.WifiSSID}'");

                return new SubmitAttendanceResponse
                {
                    Success  = false,
                    Status   = "Rejected",
                    Message  = "Attendance rejected: You must be connected to APU campus Wi-Fi.",
                    SubmittedAt = DateTime.UtcNow
                };
            }

            //  STEP 5: Device Fingerprint - Duplicate Detection 
            //    Check if this device was used by a DIFFERENT student in same session
            var duplicateDevice = await _db.Attendances
                .Where(a => a.SessionId         == session.Id &&
                            a.DeviceFingerprint == request.DeviceFingerprint &&
                            a.StudentId         != studentId)
                .Select(a => a.StudentId)
                .FirstOrDefaultAsync();

            bool isFlaggedForDevice = duplicateDevice != 0;
            string status           = isFlaggedForDevice ? "Flagged" : "Accepted";
            string? flagReason      = isFlaggedForDevice
                ? $"Device fingerprint already used by another account in this session."
                : null;

            //  STEP 6: Save attendance record 
            await LogAndSaveAttendanceAsync(
                studentId, session.Id,
                request.DeviceFingerprint,
                request.WifiSSID, request.IPAddress,
                status, flagReason);

            //  STEP 7: Log to DeviceLogs if flagged 
            if (isFlaggedForDevice)
            {
                _db.DeviceLogs.Add(new DeviceLog
                {
                    DeviceFingerprint = request.DeviceFingerprint,
                    StudentId         = studentId,
                    SessionId         = session.Id,
                    IsFlagged         = true,
                    FlagReason        = flagReason,
                    ReviewedByAdmin   = false,
                    LoggedAt          = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }

            return new SubmitAttendanceResponse
            {
                Success  = true,
                Status   = status,
                Message  = status == "Accepted"
                    ? "Attendance recorded successfully."
                    : "Attendance recorded but flagged for review. Your lecturer has been notified.",
                SubmittedAt = DateTime.UtcNow
            };
        }

        // Get attendance history for a student
        public async Task<List<AttendanceRecordDto>> GetStudentAttendanceAsync(int studentId)
        {
            return await _db.Attendances
                .Include(a => a.Session)
                    .ThenInclude(s => s.Subject)
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.SubmittedAt)
                .Select(a => new AttendanceRecordDto
                {
                    AttendanceId    = a.Id,
                    SubjectCode     = a.Session.Subject.SubjectCode,
                    SubjectName     = a.Session.Subject.SubjectName,
                    SessionCode     = a.Session.SessionCode,
                    Status          = a.Status,
                    RejectionReason = a.RejectionReason,
                    SubmittedAt     = a.SubmittedAt
                })
                .ToListAsync();
        }

        // PRIVATE: Wi-Fi validation logic
        private async Task<bool> ValidateWifiAsync(string ssid, string ipAddress)
        {
            // Check against AllowedNetworks table in DB
            // This allows admin to manage allowed networks dynamically
            var allowedNetworks = await _db.AllowedNetworks
                .Where(n => n.IsActive)
                .ToListAsync();

            foreach (var network in allowedNetworks)
            {
                bool ssidMatch = network.SSID.Equals(ssid, StringComparison.OrdinalIgnoreCase);
                bool ipMatch   = string.IsNullOrEmpty(network.IPPrefix) ||
                                 ipAddress.StartsWith(network.IPPrefix);

                if (ssidMatch && ipMatch) return true;
            }

            return false;
        }

        // Save attendance record
        private async Task LogAndSaveAttendanceAsync(
            int studentId, int sessionId,
            string deviceFingerprint,
            string? ssid, string? ip,
            string status, string? reason)
        {
            _db.Attendances.Add(new Attendance
            {
                StudentId         = studentId,
                SessionId         = sessionId,
                DeviceFingerprint = deviceFingerprint,
                WifiSSID          = ssid,
                IPAddress         = ip,
                Status            = status,
                RejectionReason   = reason,
                SubmittedAt       = DateTime.UtcNow
            });

            // Also add to DeviceLogs for every submission (for full audit trail)
            _db.DeviceLogs.Add(new DeviceLog
            {
                DeviceFingerprint = deviceFingerprint,
                StudentId         = studentId,
                SessionId         = sessionId,
                IsFlagged         = status == "Flagged",
                FlagReason        = reason,
                ReviewedByAdmin   = false,
                LoggedAt          = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }
    }
}
