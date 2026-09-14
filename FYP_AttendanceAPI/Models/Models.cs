// Models/User.cs
namespace FYP_AttendanceAPI.Models
{
    public class User
    {
        public int      Id           { get; set; }
        public string   TPNumber     { get; set; } = string.Empty;
        public string   FullName     { get; set; } = string.Empty;
        public string   Email        { get; set; } = string.Empty;
        public string   PasswordHash { get; set; } = string.Empty;
        public string   Role         { get; set; } = string.Empty; // Student, Lecturer, Admin
        public bool     IsActive     { get; set; } = true;
        public DateTime CreatedAt    { get; set; } = DateTime.UtcNow;
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutUntil { get; set; }

        // Navigation
        public ICollection<Attendance>     Attendances     { get; set; } = new List<Attendance>();
        public ICollection<DeviceLog>      DeviceLogs      { get; set; } = new List<DeviceLog>();
        public ICollection<Session>        Sessions        { get; set; } = new List<Session>();
        public ICollection<StudentSubject> StudentSubjects { get; set; } = new List<StudentSubject>();
    }
}

// Models/Subject.cs
namespace FYP_AttendanceAPI.Models
{
    public class Subject
    {
        public int    Id          { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int    LecturerId  { get; set; }

        // Navigation
        public User                        Lecturer        { get; set; } = null!;
        public ICollection<Session>        Sessions        { get; set; } = new List<Session>();
        public ICollection<StudentSubject> StudentSubjects { get; set; } = new List<StudentSubject>();
    }
}

// Models/StudentSubject.cs
namespace FYP_AttendanceAPI.Models
{
    public class StudentSubject
    {
        public int      Id         { get; set; }
        public int      StudentId  { get; set; }
        public int      SubjectId  { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User    Student { get; set; } = null!;
        public Subject Subject { get; set; } = null!;
    }
}

// Models/Session.cs
namespace FYP_AttendanceAPI.Models
{
    public class Session
    {
        public int      Id          { get; set; }
        public int      SubjectId   { get; set; }
        public int      LecturerId  { get; set; }
        public string   SessionCode { get; set; } = string.Empty; 
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt   { get; set; }
        public bool     IsActive    { get; set; } = true;

        // Navigation
        public Subject             Subject     { get; set; } = null!;
        public User                Lecturer    { get; set; } = null!;
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<DeviceLog>  DeviceLogs  { get; set; } = new List<DeviceLog>();
    }
}

// Models/Attendance.cs
namespace FYP_AttendanceAPI.Models
{
    public class Attendance
    {
        public int      Id                { get; set; }
        public int      StudentId         { get; set; }
        public int      SessionId         { get; set; }
        public string   DeviceFingerprint { get; set; } = string.Empty; // SHA256 hash
        public string?  WifiSSID          { get; set; }
        public string?  IPAddress         { get; set; }
        public string   Status            { get; set; } = string.Empty; // Accepted, Rejected, Flagged
        public string?  RejectionReason   { get; set; }
        public DateTime SubmittedAt       { get; set; } = DateTime.UtcNow;

        // Navigation
        public User    Student { get; set; } = null!;
        public Session Session { get; set; } = null!;
    }
}

// Models/DeviceLog.cs
namespace FYP_AttendanceAPI.Models
{
    public class DeviceLog
    {
        public int      Id                { get; set; }
        public string   DeviceFingerprint { get; set; } = string.Empty;
        public int      StudentId         { get; set; }
        public int      SessionId         { get; set; }
        public bool     IsFlagged         { get; set; } = false;
        public string?  FlagReason        { get; set; }
        public bool     ReviewedByAdmin   { get; set; } = false;
        public string?  AdminNotes        { get; set; }
        public DateTime LoggedAt          { get; set; } = DateTime.UtcNow;

        // Navigation
        public User    Student { get; set; } = null!;
        public Session Session { get; set; } = null!;
    }
}

// Models/AllowedNetwork.cs
namespace FYP_AttendanceAPI.Models
{
    public class AllowedNetwork
    {
        public int      Id          { get; set; }
        public string   SSID        { get; set; } = string.Empty;
        public string?  IPPrefix    { get; set; }
        public string?  Description { get; set; }
        public bool     IsActive    { get; set; } = true;
        public DateTime AddedAt     { get; set; } = DateTime.UtcNow;
    }
}
