// Models/ViewModels.cs
// All view models used in the dashboard
namespace FYP_Dashboard.Models
{
    //  Auth 
    public class LoginViewModel
    {
        public string TPNumber { get; set; } = "";
        public string Password { get; set; } = "";
        public string? ErrorMessage { get; set; }
    }

    public class LoginResponse
    {
        public string Token    { get; set; } = "";
        public string TPNumber { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Role     { get; set; } = "";
        public DateTime Expiry { get; set; }
    }

    //  Admin Dashboard 
    public class AdminDashboardViewModel
    {
        public int TotalStudents  { get; set; }
        public int TotalSessions  { get; set; }
        public int TotalFlagged   { get; set; }
        public int TotalRejected  { get; set; }
        public int TotalAccepted  { get; set; }
        public List<FlaggedAttendanceViewModel> RecentFlagged { get; set; } = new();
    }

    //  Flagged Attendance 
    public class FlaggedAttendanceViewModel
    {
        public int      AttendanceId    { get; set; }
        public string   TPNumber        { get; set; } = "";
        public string   StudentName     { get; set; } = "";
        public string   SubjectCode     { get; set; } = "";
        public string   SubjectName     { get; set; } = "";
        public string   SessionCode     { get; set; } = "";
        public DateTime SessionDate     { get; set; }
        public string?  WifiSSID        { get; set; }
        public string?  IPAddress       { get; set; }
        public string   Status          { get; set; } = "";
        public string?  RejectionReason { get; set; }
        public DateTime SubmittedAt     { get; set; }
    }

    //  Device Logs 
    public class DeviceLogViewModel
    {
        public int      LogId             { get; set; }
        public string   DeviceFingerprint { get; set; } = "";
        public string   TPNumber          { get; set; } = "";
        public string   StudentName       { get; set; } = "";
        public string   SubjectCode       { get; set; } = "";
        public string   SessionCode       { get; set; } = "";
        public DateTime SessionDate       { get; set; }
        public bool     IsFlagged         { get; set; }
        public string?  FlagReason        { get; set; }
        public bool     ReviewedByAdmin   { get; set; }
        public string?  AdminNotes        { get; set; }
        public DateTime LoggedAt          { get; set; }
    }

    public class ReviewDeviceLogViewModel
    {
        public int    LogId      { get; set; }
        public string AdminNotes { get; set; } = "";
    }

    //  Attendance Summary 
    public class AttendanceSummaryViewModel
    {
        public string TPNumber         { get; set; } = "";
        public string FullName         { get; set; } = "";
        public string SubjectCode      { get; set; } = "";
        public string SubjectName      { get; set; } = "";
        public int    TotalSubmissions { get; set; }
        public int    Accepted         { get; set; }
        public int    Flagged          { get; set; }
        public int    Rejected         { get; set; }
        public double AcceptanceRate =>
            TotalSubmissions > 0
                ? Math.Round((double)Accepted / TotalSubmissions * 100, 1)
                : 0;
    }

    //  Sessions 
    public class SessionViewModel
    {
        public int      SessionId   { get; set; }
        public string   SessionCode { get; set; } = "";
        public string   SubjectCode { get; set; } = "";
        public string   SubjectName { get; set; } = "";
        public DateTime CreatedAt   { get; set; }
        public DateTime ExpiresAt   { get; set; }
        public bool     IsActive    { get; set; }
    }

    public class SessionDetailViewModel
    {
        public int      SessionId   { get; set; }
        public string   SessionCode { get; set; } = "";
        public string   SubjectCode { get; set; } = "";
        public string   SubjectName { get; set; } = "";
        public DateTime CreatedAt   { get; set; }
        public DateTime ExpiresAt   { get; set; }
        public bool     IsActive    { get; set; }
        public List<AttendeeViewModel> Attendees { get; set; } = new();
    }

    public class AttendeeViewModel
    {
        public string   TPNumber    { get; set; } = "";
        public string   FullName    { get; set; } = "";
        public string   Status      { get; set; } = "";
        public DateTime SubmittedAt { get; set; }
    }

    public class CreateSessionViewModel
    {
        public int SubjectId { get; set; } = 1;
        public string? SuccessMessage { get; set; }
        public string? CreatedCode    { get; set; }
        public string? ErrorMessage   { get; set; }
    }

    //  Lecturer Dashboard 
    public class LecturerDashboardViewModel
    {
        public int TotalSessions       { get; set; }
        public int ActiveSessions      { get; set; }
        public int TotalAttendees      { get; set; }
        public List<SessionViewModel> RecentSessions { get; set; } = new();
    }
}
