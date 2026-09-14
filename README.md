# Enhanced Secure Attendance Verification for APSpace

![APU](https://img.shields.io/badge/Institution-Asia%20Pacific%20University-orange)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-blue)
![License](https://img.shields.io/badge/License-MIT-green)

> Final Year Project | APD3F211CS(CYB) <br>
> Muhammad Nazeer Bin Rahman | TP077423 <br>
> Supervised by Ts. Umapathy Eaganathan <br>

---

## Project Overview

This system enhances the existing APSpace attendance mechanism at Asia Pacific
University (APU) by introducing layered security controls to prevent fraudulent
attendance submissions. The current QR/code-based system allows students to mark
attendance remotely through code sharing or proxy device usage. This project
addresses those weaknesses by implementing:

- **Wi-Fi Network Validation**: attendance is only accepted from authorised
  campus Wi-Fi networks
- **Device Fingerprint Detection**: SHA-256 hashed device identifiers detect
  when the same device is used across multiple accounts in the same session
- **Audit Logging**: all submissions including rejections are logged for
  administrative review

---

## System Architecture

Presentation Tier <br>
├── Web Dashboard (ASP.NET Core MVC): Admin & Lecturer <br>
└── Mobile Web App (HTML/CSS/JS): Student <br>

Logic Tier <br>
└── REST API (ASP.NET Core Web API): Validation, Auth, Business Logic <br>

Data Tier <br>
└── SQL Server: 7 tables including Attendance, DeviceLogs, AllowedNetworks <br>


---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend API | ASP.NET Core Web API (.NET 8) |
| Web Dashboard | ASP.NET Core MVC (.NET 8) |
| Mobile Web App | HTML5, CSS3, JavaScript |
| Database | Microsoft SQL Server |
| ORM | Entity Framework Core |
| Authentication | JWT Bearer Token + BCrypt |
| Device Fingerprinting | SHA-256 via Web Crypto API |

---

## Security Features

### 1. Wi-Fi Network Validation
Every attendance submission is validated against an AllowedNetworks table
containing authorised campus Wi-Fi SSIDs and IP address prefixes. Submissions
from off-campus networks or mobile data are automatically rejected.

### 2. Device Fingerprint Duplicate Detection
A SHA-256 hash is generated from a persistent UUID combined with device
information. If the same fingerprint is detected across multiple student
accounts in the same session, the submission is flagged for admin review.

### 3. Account Lockout
After 5 failed login attempts, the account is locked for 15 minutes to
prevent brute force attacks.

### 4. Role-Based Access Control
JWT tokens carry role claims (Student, Lecturer, Admin). All API endpoints
enforce role requirements. A student token cannot access admin endpoints.

---

## Getting Started

### Prerequisites
- Visual Studio 2022+ with ASP.NET and .NET 8 workloads
- SQL Server (local instance)
- SQL Server Management Studio (SSMS)

### Setup

**1. Database**
```sql
-- Run FYP_Database_Schema.sql in SSMS
-- Creates FYP_AttendanceDB with all 7 tables and seed data
```

**2. API**
```bash
cd FYP_AttendanceAPI
# Open in Visual Studio and press F5
# API runs at https://localhost:7223
# Swagger UI at https://localhost:7223/swagger
```

**3. Web Dashboard**
```bash
cd FYP_Dashboard
# Open in Visual Studio and press F5
# Login: ADMIN001 / Admin@123 or STAFF001 / Lecturer@123
```

**4. Mobile Web App**
```bash
# Open attendance_app.html in Chrome
# Update API_BASE to match your API port
# Login: TP077423 / Student@123
```

---

## Test Accounts

| Role | TP Number | Password |
|---|---|---|
| Admin | ADMIN001 | Admin@123 |
| Lecturer | STAFF001 | Lecturer@123 |
| Student | TP077423 | Student@123 |
| Student 2 | TP000001 | Student@123 |

---

## Test Scenarios

| Scenario | Description | Expected Result |
|---|---|---|
| S1 Normal | Valid code + campus Wi-Fi | Accepted |
| S2 Code Sharing | Valid code + home Wi-Fi | Rejected |
| S3 Duplicate Device | Same device, two accounts | Flagged |
| S4 Network Bypass | Valid code + mobile data | Rejected |

---

## Project Structure

apspace-secure-attendance/ <br>
├── FYP_AttendanceAPI/ # ASP.NET Core Web API <br>
│ ├── Controllers/ # API endpoint controllers <br>
│ ├── Services/ # Business logic & validation <br>
│ ├── Models/ # Database entity models <br>
│ ├── Data/ # EF Core DbContext <br>
│ └── DTOs/ # Request/response objects <br>
├── FYP_Dashboard/ # ASP.NET Core MVC Web Dashboard <br>
│ ├── Controllers/ # MVC controllers <br>
│ ├── Views/ # Razor views (Admin & Lecturer) <br>
│ ├── Models/ # View models <br>
│ └── Services/ # API service layer <br>
└── attendance_app.html # Student mobile web application <br>

---

## Academic Information

- **Institution:** Asia Pacific University of Technology and Innovation (APU)
- **Programme:** BSc (Hons) Computer Science (Cyber Security)
- **Module:** APD3F211CS(CYB): Final Year Project
- **Supervisor:** Ts. Umapathy Eaganathan
- **2nd Marker:** Ts. Dr. Nurzati Iwani Othman
- **SDG Alignment:** SDG 4: Quality Education

---

## License

This project is licensed under the MIT License.
