using FYP_AttendanceAPI.DTOs;
using FYP_AttendanceAPI.Models;

namespace FYP_AttendanceAPI.Services
{
    // IAuthService
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
    }

    // IAttendanceService
    public interface IAttendanceService
    {
        Task<SubmitAttendanceResponse> SubmitAttendanceAsync(
            int studentId,
            SubmitAttendanceRequest request);

        Task<List<AttendanceRecordDto>> GetStudentAttendanceAsync(int studentId);
    }

    // ISessionService
    public interface ISessionService
    {
        Task<SessionDto>       CreateSessionAsync(int lecturerId, CreateSessionRequest request);
        Task<SessionDetailDto?> GetSessionDetailAsync(int sessionId);
        Task<List<SessionDto>> GetLecturerSessionsAsync(int lecturerId);
        Task<bool>             CloseSessionAsync(int sessionId, int lecturerId);
    }

    // IAdminService
    public interface IAdminService
    {
        Task<List<FlaggedAttendanceDto>> GetFlaggedAttendanceAsync();
        Task<List<DeviceLogDto>>         GetDeviceLogsAsync(bool flaggedOnly = false);
        Task<bool>                       ReviewDeviceLogAsync(ReviewDeviceLogRequest request);
        Task<List<AttendanceSummaryDto>> GetAttendanceSummaryAsync();
    }
}
