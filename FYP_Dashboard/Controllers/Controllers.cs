// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using FYP_Dashboard.Models;
using FYP_Dashboard.Services;

namespace FYP_Dashboard.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApiService _api;
        public AuthController(IApiService api) => _api = api;

        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect to correct dashboard
            var role = HttpContext.Session.GetString("Role");
            if (role == "Admin")    return RedirectToAction("Dashboard", "Admin");
            if (role == "Lecturer") return RedirectToAction("Dashboard", "Lecturer");
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var result = await _api.LoginAsync(model.TPNumber, model.Password);

            if (result is null)
            {
                model.ErrorMessage = "Invalid TP number or password. Please try again.";
                return View(model);
            }

            // Only allow Admin and Lecturer to access dashboard
            if (result.Role != "Admin" && result.Role != "Lecturer")
            {
                model.ErrorMessage = "Access denied. This dashboard is for Admin and Lecturers only.";
                return View(model);
            }

            // Store in session
            HttpContext.Session.SetString("Token",    result.Token);
            HttpContext.Session.SetString("TPNumber", result.TPNumber);
            HttpContext.Session.SetString("FullName", result.FullName);
            HttpContext.Session.SetString("Role",     result.Role);

            // Redirect based on role
            if (result.Role == "Admin")    return RedirectToAction("Dashboard", "Admin");
            if (result.Role == "Lecturer") return RedirectToAction("Dashboard", "Lecturer");

            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Error() => View();
    }
}

// Controllers/AdminController.cs
namespace FYP_Dashboard.Controllers
{
    public class AdminController : Controller
    {
        private readonly IApiService _api;
        public AdminController(IApiService api) => _api = api;

        // Guard: only Admin can access
        private string? Token => HttpContext.Session.GetString("Token");

        private IActionResult? Guard()
        {
            if (string.IsNullOrEmpty(Token) ||
                HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Auth");
            return null;
        }

        //  Dashboard (summary stats + recent flagged) 
        public async Task<IActionResult> Dashboard()
        {
            var guard = Guard(); if (guard != null) return guard;
            var model = await _api.GetAdminDashboardAsync(Token!);
            return View(model);
        }

        //  Flagged Attendance 
        public async Task<IActionResult> Flagged()
        {
            var guard = Guard(); if (guard != null) return guard;
            var model = await _api.GetFlaggedAsync(Token!);
            return View(model);
        }

        //  Device Logs 
        public async Task<IActionResult> DeviceLogs(bool flaggedOnly = false)
        {
            var guard = Guard(); if (guard != null) return guard;
            var model = await _api.GetDeviceLogsAsync(Token!, flaggedOnly);
            ViewBag.FlaggedOnly = flaggedOnly;
            return View(model);
        }

        //  Review Device Log (GET: show form) 
        public IActionResult Review(int logId)
        {
            var guard = Guard(); if (guard != null) return guard;
            return View(new ReviewDeviceLogViewModel { LogId = logId });
        }

        //  Review Device Log (POST: submit notes) 
        [HttpPost]
        public async Task<IActionResult> Review(ReviewDeviceLogViewModel model)
        {
            var guard = Guard(); if (guard != null) return guard;
            await _api.ReviewDeviceLogAsync(Token!, model.LogId, model.AdminNotes);
            TempData["Success"] = "Device log reviewed and notes saved.";
            return RedirectToAction("DeviceLogs");
        }

        //  Attendance Summary 
        public async Task<IActionResult> Summary()
        {
            var guard = Guard(); if (guard != null) return guard;
            var model = await _api.GetSummaryAsync(Token!);
            return View(model);
        }
    }
}

// Controllers/LecturerController.cs
namespace FYP_Dashboard.Controllers
{
    public class LecturerController : Controller
    {
        private readonly IApiService _api;
        public LecturerController(IApiService api) => _api = api;

        private string? Token => HttpContext.Session.GetString("Token");

        private IActionResult? Guard()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(Token) ||
                (role != "Lecturer" && role != "Admin"))
                return RedirectToAction("Login", "Auth");
            return null;
        }

        // Dashboard 
        public async Task<IActionResult> Dashboard()
        {
            var guard = Guard(); if (guard != null) return guard;
            var sessions = await _api.GetMySessionsAsync(Token!);

            var model = new LecturerDashboardViewModel
            {
                TotalSessions  = sessions.Count,
                ActiveSessions = sessions.Count(s => s.IsActive),
                TotalAttendees = 0, // loaded per session
                RecentSessions = sessions.Take(5).ToList()
            };
            return View(model);
        }

        //  All Sessions 
        public async Task<IActionResult> Sessions()
        {
            var guard = Guard(); if (guard != null) return guard;
            var model = await _api.GetMySessionsAsync(Token!);
            return View(model);
        }

        //  Session Detail 
        public async Task<IActionResult> SessionDetail(int id)
        {
            var guard = Guard(); if (guard != null) return guard;
            var model = await _api.GetSessionDetailAsync(Token!, id);
            if (model is null) return RedirectToAction("Sessions");
            return View(model);
        }

        //  Create Session (GET) 
        public IActionResult CreateSession()
        {
            var guard = Guard(); if (guard != null) return guard;
            return View(new CreateSessionViewModel());
        }

        //  Create Session (POST) 
        [HttpPost]
        public async Task<IActionResult> CreateSession(CreateSessionViewModel model)
        {
            var guard = Guard(); if (guard != null) return guard;

            var result = await _api.CreateSessionAsync(Token!, model.SubjectId);

            if (result is null)
            {
                model.ErrorMessage = "Failed to create session. Please try again.";
                return View(model);
            }

            model.SuccessMessage = $"Session created successfully!";
            model.CreatedCode    = result.SessionCode;
            return View(model);
        }

        //  Close Session 
        [HttpPost]
        public async Task<IActionResult> CloseSession(int sessionId)
        {
            var guard = Guard(); if (guard != null) return guard;
            await _api.CloseSessionAsync(Token!, sessionId);
            TempData["Success"] = "Session closed successfully.";
            return RedirectToAction("Sessions");
        }
    }
}
