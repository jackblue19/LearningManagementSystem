# LearningManagementSystem
LMS
---
## Features by Actors
* Teacher:
    - Đăng ký lịch dạy: dùng ClassSchedules (ngày, giờ, phòng, slot/batch) + phụ trợ TimeSlots, Rooms, TeacherAvailabilities. 
        - Có cả trigger đồng bộ giờ/room và proc rollback theo batch
    - Tạo bài kiểm tra: Exams (Title, ExamType, ExamDate, MaxScore, Duration, ExamStatus, FK Class/Teacher)
    - Up điểm: ExamResults (ExamId, StudentId, Score, Note)
    - Điểm danh: Attendances (ScheduleId, StudentId, StudentStatus, Note)
* Student:
    - Đăng ký khóa học: ClassRegistrations (RegistrationStatus…)
    - Xem lịch học: join ClassRegistrations → Classes → ClassSchedules ⇒ đủ (view/query level).
    - Xem & Nhập điểm: schema có Exams + ExamResults (nhập điểm), tra ExamResults theo StudentId
    - Giao dịch → lịch sử giao dịch: Payments (Amount, Method, Status, PaidAt, RegistrationId)
    - Danh sách lớp đã đăng ký
* Manager:
    - Quản lý tài khoản → tạo tài khoản GV: Users (RoleDesc=teacher, IsActive)
    - Set thông báo nộp tiền: Notifications (UserId/ClassId/CenterId, NotiType, IsRead)
    - Quản lý lớp học (timeline/schedule): Classes + ClassSchedules
    - Lịch sử giao dịch: Payments filter theo ngày/status
    - Quản lý dòng tiền → list GV → SL HS ⇒ “số tiền = số buổi * 50k”:
        - Dữ liệu “số buổi” có từ ClassSchedules; SL học sinh/lớp từ ClassRegistrations; nhưng bảng payout/đối soát chi GV chưa có.
        - Có thể báo cáo tính tức thời (view) từ lịch + số buổi đã diễn ra, nhưng nếu cần lưu chi trả thì thiếu bảng (vd. TeacherPayouts).
    - Công thức học phí bạn mô tả: “tiền học = (tổng buổi − buổi chưa học) × đơn giá 1 buổi”. DB có TotalSessions + UnitPrice ở Classes, và schedule thực tế ở ClassSchedules, nên có thể tính trên truy vấn/ứng dụng mà không cần thêm cột; UI có thể chỉ show tổng tiền khóa (= TotalSessions × UnitPrice), phần đơn giá/đối soát chỉ dành cho GV/manager.
* Admin:
    - Quản lý tài khoản manager: Users (RoleDesc=manager)
    - Audit log: AuditLogs
    - Quản lý feedback: Feedbacks (FbStatus, IsVisible)
    - Người dùng mới đăng ký: Users.CreatedAt + IsActive filter

---

## Interfaces & Implementations
* Repositories:
    - Interfaces → Entities Class
    *(trong src thì có chia theo module 1 xíu)*
    
| Repository Interface                                                | Entities                 |
| ------------------------------------------------------------------- | -------------------------------- |
| `IUserRepository`                                                   | Users                            |
| `ICenterRepository`                                                 | Centers                          |
| `ISubjectRepository`                                                | Subjects                         |
| `IClassRepository`                                                  | Classes                          |
| `IClassScheduleRepository`                                          | ClassSchedules                   |
| `IClassRegistrationRepository`                                      | ClassRegistrations               |
| `IAttendanceRepository`                                             | Attendances                      |
| `IExamRepository`                                                   | Exams                            |
| `IExamResultRepository`                                             | ExamResults                      |
| `IPaymentRepository`                                                | Payments                         |
| `IFeedbackRepository`                                               | Feedbacks                        |
| `IAuditLogRepository`                                               | AuditLogs                        |
| `INotificationRepository`                                           | Notifications                    |
| `IMaterialRepository`                                               | ClassMaterials                   |
| `ITimeSlotRepository`                                               | TimeSlots                        |
| `IRoomRepository`                                                   | Rooms                            |
| `IRoomAvailabilityRepository`                                       | RoomAvailabilities               |
| `ITeacherAvailabilityRepository`                                    | TeacherAvailabilities            |
| `IScheduleBatchRepository`                                          | ScheduleBatches                  |
| `ITeacherPayoutRepository`                                          | *(có thể thêm nếu tính migration)*|

* Services:
    - Interfaces → Role based → Services per Actors

    *(trong src thì được chia theo struct như dưới → gpt rcm, đọc lướt thấy ok → tạo y đúc luôn)*

### ___**Teacher Services**____
| Service Interface             | Liên quan bảng                                                           | Mô tả                             |
| ----------------------------- | ------------------------------------------------------------------------ | --------------------------------- |
| `IClassScheduleService`       | ClassSchedules, TimeSlots, Rooms, TeacherAvailabilities, ScheduleBatches | Đăng ký / tạo / rollback lịch dạy |
| `IExamService`                | Exams                                                                    | Tạo và quản lý bài kiểm tra       |
| `IExamResultService`          | ExamResults                                                              | Nhập điểm cho học viên            |
| `IAttendanceService`          | Attendances                                                              | Điểm danh học viên                |
| `IMaterialService`            | ClassMaterials                                                           | Upload tài liệu dạy học           |
| `ITimeSlotService`            | TimeSlots                                                                | Quản lý ca học (đọc slot)         |
| `IRoomService`                | Rooms                                                                    | Tra cứu phòng học                 |
| `ITeacherAvailabilityService` | TeacherAvailabilities                                                    | Quản lý lịch rảnh của giáo viên   |

### ___**Student Services**____
| Service Interface           | Liên quan bảng               | Mô tả                      |
| --------------------------- | ---------------------------- | -------------------------- |
| `IClassRegistrationService` | ClassRegistrations           | Đăng ký / hủy khóa học     |
| `IStudentScheduleService`   | Classes + ClassSchedules     | Xem lịch học               |
| `IStudentExamService`       | Exams                        | Xem danh sách bài kiểm tra |
| `IStudentExamResultService` | ExamResults                  | Xem điểm cá nhân           |
| `IPaymentService`           | Payments                     | Thanh toán học phí         |
| `IStudentCourseService`     | Classes + ClassRegistrations | Danh sách lớp đã đăng ký   |

### ___**Manager Services**____
| Service Interface         | Liên quan bảng                  | Mô tả                        |
| ------------------------- | ------------------------------- | ---------------------------- |
| `IUserManagementService`  | Users                           | Quản lý tài khoản giáo viên  |
| `IClassManagementService` | Classes, ClassSchedules         | Quản lý lớp học & lịch       |
| `INotificationService`    | Notifications                   | Gửi & quản lý thông báo      |
| `IPaymentReportService`   | Payments                        | Báo cáo giao dịch            |
| `ITeacherPayoutService`   | TeacherPayouts / ClassSchedules | Tính toán chi trả giáo viên  |
| `IRevenueReportService`   | Payments, Classes               | Báo cáo dòng tiền, doanh thu |

### ___**Admin Services**____
| Service Interface   | Liên quan bảng | Mô tả                     |
| ------------------- | -------------- | ------------------------- |
| `IAdminUserService` | Users          | Quản lý tài khoản manager |
| `IAuditLogService`  | AuditLogs      | Xem audit logs            |
| `IFeedbackService`  | Feedbacks      | Quản lý phản hồi          |
| `INewUserService`   | Users          | Duyệt người dùng mới      |

### ___**Common Services**____
| Service Interface     | Mục đích                   |
| --------------------- | -------------------------- |
| `IUserService`        | Quản lý user chung         |
| `IClassService`       | CRUD lớp học chung         |
| `IEmailService`       | Gửi mail thông báo         |
| `IFileStorageService` | Lưu file tài liệu, hóa đơn |
| `ICenterService`      | CRUD centers               |
| `ISubjectService`     | CRUD subjects              |

