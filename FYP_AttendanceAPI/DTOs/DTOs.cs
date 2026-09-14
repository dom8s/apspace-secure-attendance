// DTOs/AuthDTOs.cs
namespace FYP_AttendanceAPI.DTOs
{
    //  Request 
    public class LoginRequest
    {
        public string TPNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // Response 
    public class LoginResponse
    {
        public string Token     { get; set; } = string.Empty;
        public string TPNumber  { get; set; } = string.Empty;
        public string FullName  { get; set; } = string.Empty;
        public string Role      { get; set; } = string.Empty;
        public DateTime Expiry  { get; set; }
    }
}
 
// DTOs/AttendanceDTOs.cs 
namespace FYP_AttendanceAPI.DTOs
{
    //  Request: Student submitting  
    public class SubmitAttendanceRequest
    {
        public string SessionCode         { get; set; } = string.Empty; // 3-digit OTP
        public string DeviceFingerprint   { get; set; } = string.Empty; // SHA256 hashed device ID
        public string WifiSSID            { get; set; } = string.Empty; // from device
        public string IPAddress           { get; set; } = string.Empty; // from device
    }

    //  Response: Result of attendance submission
    public class SubmitAttendanceResponse
    {
        public bool   Success          { get; set; }
        public string Status           { get; set; } = string.Empty; // Accepted / Rejected / Flagged
        public string Message          { get; set; } = string.Empty;
        public DateTime SubmittedAt    { get; set; }
    }

    //  Response: Student's attendance record 
    public class AttendanceRecordDto
    {
        public int      AttendanceId    { get; set; }
        public string   SubjectCode     { get; set; } = string.Empty;
        public string   SubjectName     { get; set; } = string.Empty;
        public string   SessionCode     { get; set; } = string.Empty;
        public string   Status          { get; set; } = string.Empty;
        public string?  RejectionReason { get; set; }
        public DateTime SubmittedAt     { get; set; }
    }
}

// DTOs/SessionDTOs.cs
namespace FYP_AttendanceAPI.DTOs
{
    //  Request: Lecturer creating a session 
    public class CreateSessionRequest
    {
        public int SubjectId { get; set; }
    }

    //  Response: Session details 
    public class SessionDto
    {
        public int      SessionId   { get; set; }
        public string   SessionCode { get; set; } = string.Empty;
        public string   SubjectCode { get; set; } = string.Empty;
        public string   SubjectName { get; set; } = string.Empty;
        public DateTime CreatedAt   { get; set; }
        public DateTime ExpiresAt   { get; set; }
        public bool     IsActive    { get; set; }
    }

    //  Response: Session with attendance list 
    public class SessionDetailDto : SessionDto
    {
        public List<SessionAttendeeDto> Attendees { get; set; } = new();
    }

    public class SessionAttendeeDto
    {
        public string TPNumber  { get; set; } = string.Empty;
        public string FullName  { get; set; } = string.Empty;
        public string Status    { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }
}

// DTOs/AdminDTOs.cs
namespace FYP_AttendanceAPI.DTOs
{
    //  Flagged attendance entry 
    public class FlaggedAttendanceDto
    {
        public int      AttendanceId    { get; set; }
        public string   TPNumber        { get; set; } = string.Empty;
        public string   StudentName     { get; set; } = string.Empty;
        public string   SubjectCode     { get; set; } = string.Empty;
        public string   SubjectName     { get; set; } = string.Empty;
        public string   SessionCode     { get; set; } = string.Empty;
        public DateTime SessionDate     { get; set; }
        public string?  WifiSSID        { get; set; }
        public string?  IPAddress       { get; set; }
        public string   Status          { get; set; } = string.Empty;
        public string?  RejectionReason { get; set; }
        public DateTime SubmittedAt     { get; set; }
    }

    //  Device log entry 
    public class DeviceLogDto
    {
        public int      LogId             { get; set; }
        public string   DeviceFingerprint { get; set; } = string.Empty;
        public string   TPNumber          { get; set; } = string.Empty;
        public string   StudentName       { get; set; } = string.Empty;
        public string   SubjectCode       { get; set; } = string.Empty;
        public string   SessionCode       { get; set; } = string.Empty;
        public DateTime SessionDate       { get; set; }
        public bool     IsFlagged         { get; set; }
        public string?  FlagReason        { get; set; }
        public bool     ReviewedByAdmin   { get; set; }
        public string?  AdminNotes        { get; set; }
        public DateTime LoggedAt          { get; set; }
    }

    //  Request: Admin reviewing a device log 
    public class ReviewDeviceLogRequest
    {
        public int    LogId       { get; set; }
        public string AdminNotes  { get; set; } = string.Empty;
    }

    //  Attendance summary per student 
    public class AttendanceSummaryDto
    {
        public string TPNumber          { get; set; } = string.Empty;
        public string FullName          { get; set; } = string.Empty;
        public string SubjectCode       { get; set; } = string.Empty;
        public string SubjectName       { get; set; } = string.Empty;
        public int    TotalSubmissions  { get; set; }
        public int    Accepted          { get; set; }
        public int    Flagged           { get; set; }
        public int    Rejected          { get; set; }
    }
}
