// Services/ApiService.cs
// Handles all calls to the FYP Attendance API
using System.Net.Http.Headers;
using System.Text;
using FYP_Dashboard.Models;
using Newtonsoft.Json;

namespace FYP_Dashboard.Services
{
    public interface IApiService
    {
        Task<LoginResponse?>                      LoginAsync(string tpNumber, string password);
        Task<AdminDashboardViewModel>             GetAdminDashboardAsync(string token);
        Task<List<FlaggedAttendanceViewModel>>    GetFlaggedAsync(string token);
        Task<List<DeviceLogViewModel>>            GetDeviceLogsAsync(string token, bool flaggedOnly = false);
        Task<bool>                                ReviewDeviceLogAsync(string token, int logId, string notes);
        Task<List<AttendanceSummaryViewModel>>    GetSummaryAsync(string token);
        Task<List<SessionViewModel>>              GetMySessionsAsync(string token);
        Task<SessionDetailViewModel?>             GetSessionDetailAsync(string token, int sessionId);
        Task<SessionViewModel?>                   CreateSessionAsync(string token, int subjectId);
        Task<bool>                                CloseSessionAsync(string token, int sessionId);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _http;

        public ApiService(HttpClient http) => _http = http;

        //  Helper: set Bearer token on every request 
        private void SetToken(string token) =>
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

        //  Helper: POST with JSON body 
        private StringContent Json(object obj) =>
            new StringContent(
                JsonConvert.SerializeObject(obj),
                Encoding.UTF8,
                "application/json");

        //  Login 
        public async Task<LoginResponse?> LoginAsync(string tpNumber, string password)
        {
            try
            {
                var response = await _http.PostAsync(
                    "api/Auth/login",
                    Json(new { tpNumber, password }));

                if (!response.IsSuccessStatusCode) return null;

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<LoginResponse>(content);
            }
            catch { return null; }
        }

        //  Admin Dashboard  
        public async Task<AdminDashboardViewModel> GetAdminDashboardAsync(string token)
        {
            SetToken(token);
            var dashboard = new AdminDashboardViewModel();

            try
            {
                // Get flagged list
                var flaggedResp = await _http.GetStringAsync("api/Admin/flagged");
                var flagged = JsonConvert.DeserializeObject<List<FlaggedAttendanceViewModel>>(flaggedResp)
                              ?? new();

                // Get summary for totals
                var summaryResp = await _http.GetStringAsync("api/Admin/summary");
                var summary = JsonConvert.DeserializeObject<List<AttendanceSummaryViewModel>>(summaryResp)
                              ?? new();

                dashboard.TotalFlagged  = flagged.Count(f => f.Status == "Flagged");
                dashboard.TotalRejected = flagged.Count(f => f.Status == "Rejected");
                dashboard.TotalAccepted = summary.Sum(s => s.Accepted);
                dashboard.TotalStudents = summary.Select(s => s.TPNumber).Distinct().Count();
                dashboard.TotalSessions = summary.Sum(s => s.TotalSubmissions);
                dashboard.RecentFlagged = flagged.Take(5).ToList();
            }
            catch { }

            return dashboard;
        }

        //  Admin Flagged attendance 
        public async Task<List<FlaggedAttendanceViewModel>> GetFlaggedAsync(string token)
        {
            SetToken(token);
            try
            {
                var resp = await _http.GetStringAsync("api/Admin/flagged");
                return JsonConvert.DeserializeObject<List<FlaggedAttendanceViewModel>>(resp)
                       ?? new();
            }
            catch { return new(); }
        }

        //  Admin Device logs 
        public async Task<List<DeviceLogViewModel>> GetDeviceLogsAsync(
            string token, bool flaggedOnly = false)
        {
            SetToken(token);
            try
            {
                var url  = $"api/Admin/devicelogs?flaggedOnly={flaggedOnly}";
                var resp = await _http.GetStringAsync(url);
                return JsonConvert.DeserializeObject<List<DeviceLogViewModel>>(resp)
                       ?? new();
            }
            catch { return new(); }
        }

        //  Admin Review device log 
        public async Task<bool> ReviewDeviceLogAsync(
            string token, int logId, string notes)
        {
            SetToken(token);
            try
            {
                var resp = await _http.PutAsync(
                    "api/Admin/devicelogs/review",
                    Json(new { logId, adminNotes = notes }));
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        //  Admin Attendance summary 
        public async Task<List<AttendanceSummaryViewModel>> GetSummaryAsync(string token)
        {
            SetToken(token);
            try
            {
                var resp = await _http.GetStringAsync("api/Admin/summary");
                return JsonConvert.DeserializeObject<List<AttendanceSummaryViewModel>>(resp)
                       ?? new();
            }
            catch { return new(); }
        }

        //  Lecturer Get my sessions 
        public async Task<List<SessionViewModel>> GetMySessionsAsync(string token)
        {
            SetToken(token);
            try
            {
                var resp = await _http.GetStringAsync("api/Session/my");
                return JsonConvert.DeserializeObject<List<SessionViewModel>>(resp)
                       ?? new();
            }
            catch { return new(); }
        }

        //  Lecturer Session detail 
        public async Task<SessionDetailViewModel?> GetSessionDetailAsync(
            string token, int sessionId)
        {
            SetToken(token);
            try
            {
                var resp = await _http.GetStringAsync($"api/Session/{sessionId}");
                return JsonConvert.DeserializeObject<SessionDetailViewModel>(resp);
            }
            catch { return null; }
        }

        //  Lecturer Create session 
        public async Task<SessionViewModel?> CreateSessionAsync(
            string token, int subjectId)
        {
            SetToken(token);
            try
            {
                var resp = await _http.PostAsync(
                    "api/Session/create",
                    Json(new { subjectId }));

                if (!resp.IsSuccessStatusCode) return null;

                var content = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SessionViewModel>(content);
            }
            catch { return null; }
        }

        //  Lecturer Close session 
        public async Task<bool> CloseSessionAsync(string token, int sessionId)
        {
            SetToken(token);
            try
            {
                var resp = await _http.PutAsync(
                    $"api/Session/{sessionId}/close",
                    Json(new { }));
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}
