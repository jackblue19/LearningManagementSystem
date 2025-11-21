using LMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(CenterDbContext context)
    {
        // Check if data already exists
        if (await context.Users.AnyAsync())
        {
            Console.WriteLine("Database already seeded. Skipping...");
            return;
        }

        Console.WriteLine("Starting data seeding...");

        // Hash password function (same as AuthService)
        string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password + "LMS_SALT_2025");
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // Default password for all test accounts: "123456"
        var defaultPasswordHash = HashPassword("123456");
        Console.WriteLine(defaultPasswordHash);

        // 1. Seed Users (Admin, Manager, Teacher, Students)
        var adminId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();
        var student1Id = Guid.NewGuid();
        var student2Id = Guid.NewGuid();
        var student3Id = Guid.NewGuid();
        var student4Id = Guid.NewGuid();
        var student5Id = Guid.NewGuid();

        var users = new List<User>
        {
            new User
            {
                UserId = adminId,
                Username = "admin",
                Email = "admin@lms.com",
                PasswordHash = defaultPasswordHash,
                FullName = "System Administrator",
                Phone = "0900000000",
                RoleDesc = "admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Avatar = "/images/avatars/admin.jpg"
            },
            new User
            {
                UserId = managerId,
                Username = "manager",
                Email = "manager@lms.com",
                PasswordHash = defaultPasswordHash,
                FullName = "Nguyễn Văn Quản Lý",
                Phone = "0901234567",
                RoleDesc = "manager",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Avatar = "/images/avatars/manager.jpg"
            },
            new User
            {
                UserId = teacherId,
                Username = "teacher",
                Email = "teacher@lms.com",
                PasswordHash = defaultPasswordHash,
                FullName = "Trần Thị Giáo Viên",
                Phone = "0902345678",
                RoleDesc = "teacher",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Avatar = "/images/avatars/teacher.jpg"
            },
            new User
            {
                UserId = student1Id,
                Username = "student01",
                Email = "student01@lms.com",
                PasswordHash = defaultPasswordHash,
                FullName = "Lê Văn An",
                Phone = "0903456789",
                RoleDesc = "student",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = student2Id,
                Username = "student02",
                Email = "student02@lms.com",
                PasswordHash = defaultPasswordHash,
                FullName = "Phạm Thị Bình",
                Phone = "0904567890",
                RoleDesc = "student",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = student3Id,
                Username = "student03",
                Email = "student03@lms.com",
                PasswordHash = defaultPasswordHash,
                FullName = "Hoàng Văn Cường",
                Phone = "0905678901",
                RoleDesc = "student",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = student4Id,
                Username = "student04",
                Email = "student04@lms.com",
                PasswordHash = defaultPasswordHash,
                FullName = "Đặng Thị Dung",
                Phone = "0906789012",
                RoleDesc = "student",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = student5Id,
                Username = "student05",
                Email = "student05@lms.com",
                PasswordHash = defaultPasswordHash,
                FullName = "Vũ Văn Em",
                Phone = "0907890123",
                RoleDesc = "student",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
        Console.WriteLine("✓ Seeded 8 users (1 admin, 1 manager, 1 teacher, 5 students)");

        // 2. Seed Centers
        var centerId = Guid.NewGuid();
        var center = new Center
        {
            CenterId = centerId,
            CenterName = "Trung tâm FPT Education",
            CenterAddress = "Lô E2a-7, Đường D1, Đ. D1, Long Thạnh Mỹ, Thành Phố Thủ Đức, Hồ Chí Minh",
            Phone = "0281234567",
            CenterEmail = "contact@fptedu.vn",
            ManagerId = managerId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Logo = "/images/centers/fpt-logo.png"
        };

        await context.Centers.AddAsync(center);
        await context.SaveChangesAsync();
        Console.WriteLine("✓ Seeded 1 center");

        // 3. Seed Subjects
        var subjects = new List<Subject>
        {
            new Subject
            {
                SubjectName = "PRN221 - Advanced Programming with .NET",
                GradeLevel = "Advanced",
                CenterId = centerId,
                CreatedAt = DateTime.UtcNow
            },
            new Subject
            {
                SubjectName = "PRN212 - Basics of Object-Oriented Programming",
                GradeLevel = "Intermediate",
                CenterId = centerId,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Subjects.AddRangeAsync(subjects);
        await context.SaveChangesAsync();
        var subject1Id = subjects[0].SubjectId;
        var subject2Id = subjects[1].SubjectId;
        Console.WriteLine("✓ Seeded 2 subjects");

        // 4. Seed Rooms (CRITICAL for TeacherRooms feature)
        var room1Id = Guid.NewGuid();
        var room2Id = Guid.NewGuid();
        var room3Id = Guid.NewGuid();
        var rooms = new List<Room>
        {
            new Room
            {
                RoomId = room1Id,
                CenterId = centerId,
                RoomName = "Room 501",
                Capacity = 30,
                IsActive = true
            },
            new Room
            {
                RoomId = room2Id,
                CenterId = centerId,
                RoomName = "Room 502",
                Capacity = 40,
                IsActive = true
            },
            new Room
            {
                RoomId = room3Id,
                CenterId = centerId,
                RoomName = "Lab 301",
                Capacity = 25,
                IsActive = true
            }
        };

        await context.Rooms.AddRangeAsync(rooms);
        await context.SaveChangesAsync();
        Console.WriteLine("✓ Seeded 3 rooms");

        // 5. Seed TimeSlots
        var timeSlots = new List<TimeSlot>
        {
            new TimeSlot { SlotLabel = "Slot 1", StartTime = new TimeOnly(7, 0), EndTime = new TimeOnly(9, 0), SlotOrder = 1 },
            new TimeSlot { SlotLabel = "Slot 2", StartTime = new TimeOnly(9, 15), EndTime = new TimeOnly(11, 15), SlotOrder = 2 },
            new TimeSlot { SlotLabel = "Slot 3", StartTime = new TimeOnly(12, 30), EndTime = new TimeOnly(14, 30), SlotOrder = 3 },
            new TimeSlot { SlotLabel = "Slot 4", StartTime = new TimeOnly(14, 45), EndTime = new TimeOnly(16, 45), SlotOrder = 4 },
            new TimeSlot { SlotLabel = "Slot 5", StartTime = new TimeOnly(17, 0), EndTime = new TimeOnly(19, 0), SlotOrder = 5 },
            new TimeSlot { SlotLabel = "Slot 6", StartTime = new TimeOnly(19, 15), EndTime = new TimeOnly(21, 15), SlotOrder = 6 }
        };

        await context.TimeSlots.AddRangeAsync(timeSlots);
        await context.SaveChangesAsync();
        var slot1Id = timeSlots[0].SlotId;
        var slot2Id = timeSlots[1].SlotId;
        Console.WriteLine("✓ Seeded 6 time slots");

        // 6. Seed Classes
        var class1Id = Guid.NewGuid();
        var class2Id = Guid.NewGuid();
        var classes = new List<Class>
        {
            new Class
            {
                ClassId = class1Id,
                ClassName = "PRN221_FA25_001",
                SubjectId = subject1Id,
                TeacherId = teacherId,
                CenterId = centerId,
                ClassAddress = "Room 501, FPT Education Center",
                UnitPrice = 500000m,
                TotalSessions = 30,
                StartDate = new DateOnly(2025, 11, 18),
                EndDate = new DateOnly(2026, 2, 15),
                ClassStatus = "active",
                ScheduleDesc = "Monday, Wednesday, Friday - Slot 1",
                CreatedAt = DateTime.UtcNow
            },
            new Class
            {
                ClassId = class2Id,
                ClassName = "PRN212_FA25_002",
                SubjectId = subject2Id,
                TeacherId = teacherId,
                CenterId = centerId,
                ClassAddress = "Room 502, FPT Education Center",
                UnitPrice = 450000m,
                TotalSessions = 28,
                StartDate = new DateOnly(2025, 11, 20),
                EndDate = new DateOnly(2026, 2, 20),
                ClassStatus = "active",
                ScheduleDesc = "Tuesday, Thursday - Slot 2",
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Classes.AddRangeAsync(classes);
        await context.SaveChangesAsync();
        Console.WriteLine("✓ Seeded 2 classes");

        // 7. Seed Class Registrations
        var registrations = new List<ClassRegistration>
        {
            new ClassRegistration
            {
                ClassId = class1Id,
                StudentId = student1Id,
                RegistrationStatus = "approved",
                RegisteredAt = DateTime.UtcNow.AddDays(-7)
            },
            new ClassRegistration
            {
                ClassId = class1Id,
                StudentId = student2Id,
                RegistrationStatus = "approved",
                RegisteredAt = DateTime.UtcNow.AddDays(-6)
            },
            new ClassRegistration
            {
                ClassId = class1Id,
                StudentId = student3Id,
                RegistrationStatus = "approved",
                RegisteredAt = DateTime.UtcNow.AddDays(-5)
            },
            new ClassRegistration
            {
                ClassId = class1Id,
                StudentId = student4Id,
                RegistrationStatus = "approved",
                RegisteredAt = DateTime.UtcNow.AddDays(-4)
            },
            new ClassRegistration
            {
                ClassId = class2Id,
                StudentId = student3Id,
                RegistrationStatus = "approved",
                RegisteredAt = DateTime.UtcNow.AddDays(-3)
            },
            new ClassRegistration
            {
                ClassId = class2Id,
                StudentId = student5Id,
                RegistrationStatus = "approved",
                RegisteredAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        await context.ClassRegistrations.AddRangeAsync(registrations);
        await context.SaveChangesAsync();
        Console.WriteLine("✓ Seeded 6 class registrations");

        // 8. Seed Class Schedules (More data for GlobalSchedule testing)
        var today = DateOnly.FromDateTime(DateTime.Now);
        var monday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        
        var schedules = new List<ClassSchedule>
        {
            // Monday (Thứ 2) - Multiple rooms in same slot
            new ClassSchedule
            {
                ClassId = class1Id,
                SessionDate = monday,
                RoomName = "Room 501",
                RoomId = room1Id,
                SlotId = slot1Id,
                StartTime = new TimeOnly(7, 0),
                EndTime = new TimeOnly(9, 0),
                SlotOrder = 1,
                ScheduleLabel = "PRN221 - Session 1",
                CreatedAt = DateTime.UtcNow,
                IsAutoGenerated = false
            },
            new ClassSchedule
            {
                ClassId = class2Id,
                SessionDate = monday,
                RoomName = "Room 502",
                RoomId = room2Id,
                SlotId = slot1Id,
                StartTime = new TimeOnly(7, 0),
                EndTime = new TimeOnly(9, 0),
                SlotOrder = 1,
                ScheduleLabel = "PRN212 - Session 1",
                CreatedAt = DateTime.UtcNow,
                IsAutoGenerated = false
            },
            new ClassSchedule
            {
                ClassId = class1Id,
                SessionDate = monday,
                RoomName = "Lab 301",
                RoomId = room3Id,
                SlotId = slot2Id,
                StartTime = new TimeOnly(9, 15),
                EndTime = new TimeOnly(11, 15),
                SlotOrder = 2,
                ScheduleLabel = "PRN221 - Lab Session",
                CreatedAt = DateTime.UtcNow,
                IsAutoGenerated = false
            },
            
            // Tuesday (Thứ 3)
            new ClassSchedule
            {
                ClassId = class1Id,
                SessionDate = monday.AddDays(1),
                RoomName = "Room 501",
                RoomId = room1Id,
                SlotId = slot2Id,
                StartTime = new TimeOnly(9, 15),
                EndTime = new TimeOnly(11, 15),
                SlotOrder = 2,
                ScheduleLabel = "PRN221 - Session 2",
                CreatedAt = DateTime.UtcNow,
                IsAutoGenerated = false
            },
            new ClassSchedule
            {
                ClassId = class2Id,
                SessionDate = monday.AddDays(1),
                RoomName = "Room 502",
                RoomId = room2Id,
                SlotId = timeSlots[2].SlotId, // Slot 3
                StartTime = new TimeOnly(12, 30),
                EndTime = new TimeOnly(14, 30),
                SlotOrder = 3,
                ScheduleLabel = "PRN212 - Session 2",
                CreatedAt = DateTime.UtcNow,
                IsAutoGenerated = false
            },
            
            // Wednesday (Thứ 4) - All 3 rooms in Slot 1
            new ClassSchedule
            {
                ClassId = class1Id,
                SessionDate = monday.AddDays(2),
                RoomName = "Room 501",
                RoomId = room1Id,
                SlotId = slot1Id,
                StartTime = new TimeOnly(7, 0),
                EndTime = new TimeOnly(9, 0),
                SlotOrder = 1,
                ScheduleLabel = "PRN221 - Session 3",
                CreatedAt = DateTime.UtcNow,
                IsAutoGenerated = false
            },
            new ClassSchedule
            {
                ClassId = class2Id,
                SessionDate = monday.AddDays(2),
                RoomName = "Room 502",
                RoomId = room2Id,
                SlotId = slot1Id,
                StartTime = new TimeOnly(7, 0),
                EndTime = new TimeOnly(9, 0),
                SlotOrder = 1,
                ScheduleLabel = "PRN212 - Session 3",
                CreatedAt = DateTime.UtcNow,
                IsAutoGenerated = false
            },
            new ClassSchedule
            {
                ClassId = class1Id,
                SessionDate = monday.AddDays(2),
                RoomName = "Lab 301",
                RoomId = room3Id,
                SlotId = slot1Id,
                StartTime = new TimeOnly(7, 0),
                EndTime = new TimeOnly(9, 0),
                SlotOrder = 1,
                ScheduleLabel = "PRN221 - Extra Lab",
                CreatedAt = DateTime.UtcNow,
                IsAutoGenerated = false
            },
            
            // Thursday (Thứ 5)
            new ClassSchedule
            {
                ClassId = class2Id,
                SessionDate = monday.AddDays(3),
                RoomName = "Room 502",
                RoomId = room2Id,
                SlotId = slot2Id,
                StartTime = new TimeOnly(9, 15),
                EndTime = new TimeOnly(11, 15),
                SlotOrder = 2,
                ScheduleLabel = "PRN212 - Session 4",
                CreatedAt = DateTime.UtcNow,
                IsAutoGenerated = false
            },
            
            // Friday (Thứ 6) - 2 rooms
            new ClassSchedule
            {
                ClassId = class1Id,
                SessionDate = monday.AddDays(4),
                RoomName = "Room 501",
                RoomId = room1Id,
                SlotId = slot1Id,
                StartTime = new TimeOnly(7, 0),
                EndTime = new TimeOnly(9, 0),
                SlotOrder = 1,
                ScheduleLabel = "PRN221 - Session 4",
                CreatedAt = DateTime.UtcNow,
                IsAutoGenerated = false
            },
            new ClassSchedule
            {
                ClassId = class2Id,
                SessionDate = monday.AddDays(4),
                RoomName = "Lab 301",
                RoomId = room3Id,
                SlotId = timeSlots[4].SlotId, // Slot 5
                StartTime = new TimeOnly(17, 0),
                EndTime = new TimeOnly(19, 0),
                SlotOrder = 5,
                ScheduleLabel = "PRN212 - Evening Session",
                CreatedAt = DateTime.UtcNow,
                IsAutoGenerated = false
            }
        };

        await context.ClassSchedules.AddRangeAsync(schedules);
        await context.SaveChangesAsync();
        var schedule1Id = schedules[0].ScheduleId;
        Console.WriteLine("✓ Seeded 11 class schedules (for GlobalSchedule testing)");

        // 9. Seed Class Materials
        var materials = new List<ClassMaterial>
        {
            new ClassMaterial
            {
                MaterialId = Guid.NewGuid(),
                ClassId = class1Id,
                Title = "Lecture 01 - Introduction to .NET Core",
                FileUrl = "/uploads/materials/lecture01.pdf",
                UploadedByUserId = teacherId,
                UploadedAt = DateTime.UtcNow.AddDays(-2),
                MaterialType = "slide",
                Note = "Slide bài giảng buổi 1",
                FileSize = 2048000
            },
            new ClassMaterial
            {
                MaterialId = Guid.NewGuid(),
                ClassId = class1Id,
                Title = "Lab Assignment 01",
                FileUrl = "/uploads/materials/lab01.docx",
                UploadedByUserId = teacherId,
                UploadedAt = DateTime.UtcNow.AddDays(-1),
                MaterialType = "exercise",
                Note = "Bài tập thực hành buổi 1",
                FileSize = 512000
            },
            new ClassMaterial
            {
                MaterialId = Guid.NewGuid(),
                ClassId = class1Id,
                Title = "Sample Project - Razor Pages Demo",
                FileUrl = "/uploads/materials/demo-project.zip",
                UploadedByUserId = teacherId,
                UploadedAt = DateTime.UtcNow,
                MaterialType = "document",
                Note = "Source code mẫu",
                FileSize = 5120000
            },
            new ClassMaterial
            {
                MaterialId = Guid.NewGuid(),
                ClassId = class2Id,
                Title = "OOP Fundamentals",
                FileUrl = "/uploads/materials/oop-basics.pdf",
                UploadedByUserId = teacherId,
                UploadedAt = DateTime.UtcNow.AddDays(-1),
                MaterialType = "slide",
                Note = "Tài liệu lý thuyết OOP",
                FileSize = 3072000
            }
        };

        await context.ClassMaterials.AddRangeAsync(materials);
        await context.SaveChangesAsync();
        Console.WriteLine("✓ Seeded 4 class materials");

        // 10. Seed Attendances
        var attendances = new List<Attendance>
        {
            new Attendance
            {
                ScheduleId = schedule1Id,
                StudentId = student1Id,
                StudentStatus = "present",
                Note = null
            },
            new Attendance
            {
                ScheduleId = schedule1Id,
                StudentId = student2Id,
                StudentStatus = "late",
                Note = "Đến muộn 15 phút"
            },
            new Attendance
            {
                ScheduleId = schedule1Id,
                StudentId = student3Id,
                StudentStatus = "present",
                Note = null
            },
            new Attendance
            {
                ScheduleId = schedule1Id,
                StudentId = student4Id,
                StudentStatus = "absent",
                Note = "Xin nghỉ ốm"
            }
        };

        await context.Attendances.AddRangeAsync(attendances);
        await context.SaveChangesAsync();
        Console.WriteLine("✓ Seeded 4 attendances");

        // 11. Seed Feedbacks
        var feedbacks = new List<Feedback>
        {
            new Feedback
            {
                UserId = student1Id,
                ClassId = class1Id,
                RegistrationId = registrations[0].RegistrationId,
                Content = "Khóa học rất bổ ích, thầy giáo nhiệt tình.",
                Rating = 5,
                FbStatus = "approved",
                IsVisible = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Feedback
            {
                UserId = student2Id,
                ClassId = class1Id,
                RegistrationId = registrations[1].RegistrationId,
                Content = "Bài tập hơi khó, cần thêm thời gian thực hành.",
                Rating = 4,
                FbStatus = "pending",
                IsVisible = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Feedback
            {
                UserId = student3Id,
                ClassId = class2Id,
                RegistrationId = registrations[4].RegistrationId,
                Content = "Cơ sở vật chất tốt, máy lạnh mát.",
                Rating = 5,
                FbStatus = "approved",
                IsVisible = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Feedback
            {
                UserId = student5Id,
                ClassId = class2Id,
                RegistrationId = registrations[5].RegistrationId,
                Content = "Giảng viên dạy hơi nhanh.",
                Rating = 3,
                FbStatus = "rejected",
                IsVisible = false,
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new Feedback
            {
                UserId = student4Id,
                ClassId = class1Id,
                RegistrationId = registrations[3].RegistrationId,
                Content = "Tôi xin phép nghỉ ốm nên không đánh giá chi tiết được.",
                Rating = null,
                FbStatus = "approved",
                IsVisible = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        await context.Feedbacks.AddRangeAsync(feedbacks);
        await context.SaveChangesAsync();
      
        // 11. Seed Teacher Availabilities (For Manager features testing)
        var teacherAvailabilities = new List<TeacherAvailability>
        {
            // Monday slots
            new TeacherAvailability
            {
                TeacherId = teacherId,
                DayOfWeek = 2, // Monday
                StartTime = new TimeOnly(7, 0),
                EndTime = new TimeOnly(9, 0),
                Note = "Buổi sáng thứ 2 - Slot 1"
            },
            new TeacherAvailability
            {
                TeacherId = teacherId,
                DayOfWeek = 2,
                StartTime = new TimeOnly(9, 15),
                EndTime = new TimeOnly(11, 15),
                Note = "Buổi sáng thứ 2 - Slot 2"
            },
            // Wednesday slots
            new TeacherAvailability
            {
                TeacherId = teacherId,
                DayOfWeek = 4, // Wednesday
                StartTime = new TimeOnly(7, 0),
                EndTime = new TimeOnly(9, 0),
                Note = "Buổi sáng thứ 4 - Slot 1"
            },
            new TeacherAvailability
            {
                TeacherId = teacherId,
                DayOfWeek = 4,
                StartTime = new TimeOnly(14, 45),
                EndTime = new TimeOnly(16, 45),
                Note = "Buổi chiều thứ 4 - Slot 4"
            },
            // Friday slots
            new TeacherAvailability
            {
                TeacherId = teacherId,
                DayOfWeek = 6, // Friday
                StartTime = new TimeOnly(7, 0),
                EndTime = new TimeOnly(9, 0),
                Note = "Buổi sáng thứ 6 - Slot 1"
            },
            new TeacherAvailability
            {
                TeacherId = teacherId,
                DayOfWeek = 6,
                StartTime = new TimeOnly(17, 0),
                EndTime = new TimeOnly(19, 0),
                Note = "Buổi tối thứ 6 - Slot 5"
            },
            // Thursday slots
            new TeacherAvailability
            {
                TeacherId = teacherId,
                DayOfWeek = 5, // Thursday
                StartTime = new TimeOnly(9, 15),
                EndTime = new TimeOnly(11, 15),
                Note = "Buổi sáng thứ 5 - Slot 2 (trùng với lớp PRN212)"
            },
            // Tuesday slots
            new TeacherAvailability
            {
                TeacherId = teacherId,
                DayOfWeek = 3, // Tuesday
                StartTime = new TimeOnly(9, 15),
                EndTime = new TimeOnly(11, 15),
                Note = "Buổi sáng thứ 3 - Slot 2"
            },
            new TeacherAvailability
            {
                TeacherId = teacherId,
                DayOfWeek = 3,
                StartTime = new TimeOnly(12, 30),
                EndTime = new TimeOnly(14, 30),
                Note = "Buổi trưa thứ 3 - Slot 3"
            }
        };

        await context.TeacherAvailabilities.AddRangeAsync(teacherAvailabilities);
        await context.SaveChangesAsync();
       
    }
}
