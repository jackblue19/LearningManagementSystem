# LearningManagementSystem
LMS
---
### Features by Actors
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

### Interfaces & Implementations
* Repositories:
    - Interfaces → Entities Class
* Services:
    - Interfaces → Role based → Services per Actors
