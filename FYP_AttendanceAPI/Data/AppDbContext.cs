using Microsoft.EntityFrameworkCore;
using FYP_AttendanceAPI.Models;

namespace FYP_AttendanceAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //  DbSets 
        public DbSet<User>           Users           { get; set; }
        public DbSet<Subject>        Subjects        { get; set; }
        public DbSet<StudentSubject> StudentSubjects { get; set; }
        public DbSet<Session>        Sessions        { get; set; }
        public DbSet<Attendance>     Attendances     { get; set; }
        public DbSet<DeviceLog>      DeviceLogs      { get; set; }
        public DbSet<AllowedNetwork> AllowedNetworks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //  Users 
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(u => u.Id);
                e.HasIndex(u => u.TPNumber).IsUnique();
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Role)
                 .HasConversion<string>()
                 .HasMaxLength(20);
            });

            //  Subjects 
            modelBuilder.Entity<Subject>(e =>
            {
                e.HasKey(s => s.Id);
                e.HasIndex(s => s.SubjectCode).IsUnique();
                e.HasOne(s => s.Lecturer)
                 .WithMany()
                 .HasForeignKey(s => s.LecturerId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            //  StudentSubjects 
            modelBuilder.Entity<StudentSubject>(e =>
            {
                e.HasKey(ss => ss.Id);
                // Prevent duplicate enrolment
                e.HasIndex(ss => new { ss.StudentId, ss.SubjectId }).IsUnique();
                e.HasOne(ss => ss.Student)
                 .WithMany(u => u.StudentSubjects)
                 .HasForeignKey(ss => ss.StudentId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(ss => ss.Subject)
                 .WithMany(s => s.StudentSubjects)
                 .HasForeignKey(ss => ss.SubjectId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            //  Sessions 
            modelBuilder.Entity<Session>(e =>
            {
                e.ToTable("Sessions");
                e.HasKey(s => s.Id);
                e.Property(s => s.SessionCode).HasMaxLength(3).IsFixedLength();
                e.HasOne(s => s.Subject)
                 .WithMany(sub => sub.Sessions)
                 .HasForeignKey(s => s.SubjectId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(s => s.Lecturer)
                 .WithMany(u => u.Sessions)
                 .HasForeignKey(s => s.LecturerId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            //  Attendance 
            modelBuilder.Entity<Attendance>(e =>
            {
                e.ToTable("Attendance"); 
                e.HasKey(a => a.Id);
                // One submission per student per session
                e.HasIndex(a => new { a.StudentId, a.SessionId }).IsUnique();
                e.Property(a => a.Status).HasMaxLength(20);
                e.HasOne(a => a.Student)
                 .WithMany(u => u.Attendances)
                 .HasForeignKey(a => a.StudentId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(a => a.Session)
                 .WithMany(s => s.Attendances)
                 .HasForeignKey(a => a.SessionId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            //  DeviceLogs 
            modelBuilder.Entity<DeviceLog>(e =>
            {
                e.ToTable("DeviceLogs");
                e.HasKey(d => d.Id);
                e.HasOne(d => d.Student)
                 .WithMany(u => u.DeviceLogs)
                 .HasForeignKey(d => d.StudentId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(d => d.Session)
                 .WithMany(s => s.DeviceLogs)
                 .HasForeignKey(d => d.SessionId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            //  AllowedNetworks 
            modelBuilder.Entity<AllowedNetwork>(e =>
            {
                e.HasKey(n => n.Id);
                e.Property(n => n.SSID).HasMaxLength(100);
            });

            //   Admin:         "Admin@123"
            //   Lecturer:      "Lecturer@123"
            //   Student:       "Student@123"
            //   User nazeer and 00001 user have same hash

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id           = 1,
                    TPNumber     = "ADMIN001",
                    FullName     = "System Administrator",
                    Email        = "admin@apu.edu.my",
                    PasswordHash = "$2a$11$URcdh2Oxz3ZXLUikxoaP8.9rLOq1FMdvxK.8VigsTD8ghPKhzLv8S",
                    Role         = "Admin",
                    IsActive     = true,
                    CreatedAt    = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id           = 2,
                    TPNumber     = "STAFF001",
                    FullName     = "Ts. Umapathy Eaganathan",
                    Email        = "umapathy@apu.edu.my",
                    PasswordHash = "$2a$11$4rcXaS/ZAAUmAPXcrMvuOeV56GC3TCuBGv4p/Uzfcu86Q6J771.Ni",
                    Role         = "Lecturer",
                    IsActive     = true,
                    CreatedAt    = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id           = 3,
                    TPNumber     = "TP077423",
                    FullName     = "Muhammad Nazeer Bin Rahman",
                    Email        = "tp077423@mail.apu.edu.my",
                    PasswordHash = "$2a$11$YXqUrCYrAaqnIw7AGWN4UenDtlO5kfuhMS.384jteuVOMciFabciK",
                    Role         = "Student",
                    IsActive     = true,
                    CreatedAt    = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id           = 4,
                    TPNumber     = "TP000001",
                    FullName     = "Test Student Two",
                    Email        = "tp000001@mail.apu.edu.my",
                    PasswordHash = "$2a$11$YXqUrCYrAaqnIw7AGWN4UenDtlO5kfuhMS.384jteuVOMciFabciK",
                    Role         = "Student",
                    IsActive     = true,
                    CreatedAt    = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            modelBuilder.Entity<Subject>().HasData(
                new Subject
                {
                    Id          = 1,
                    SubjectCode = "CT098-3-3-CYB",
                    SubjectName = "Final Year Project",
                    LecturerId  = 2
                }
            );

            modelBuilder.Entity<StudentSubject>().HasData(
                new StudentSubject { Id = 1, StudentId = 3, SubjectId = 1,
                    EnrolledAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new StudentSubject { Id = 2, StudentId = 4, SubjectId = 1,
                    EnrolledAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            modelBuilder.Entity<AllowedNetwork>().HasData(
                new AllowedNetwork
                {
                    Id          = 1, SSID = "APU-Student",
                    IPPrefix    = "10.", Description = "APU Main Student Wi-Fi",
                    IsActive    = true,
                    AddedAt     = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new AllowedNetwork
                {
                    Id          = 2, SSID = "APU-Staff",
                    IPPrefix    = "10.", Description = "APU Staff Wi-Fi",
                    IsActive    = true,
                    AddedAt     = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new AllowedNetwork
                {
                    Id          = 3, SSID = "APUnet",
                    IPPrefix    = "172.16.", Description = "APU Internal Network",
                    IsActive    = true,
                    AddedAt     = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
