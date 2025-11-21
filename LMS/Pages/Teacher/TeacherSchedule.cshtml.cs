using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using LMS.Models.Entities;
using LMS.Models.ViewModels.Scheduling;
using LMS.Services.Interfaces.TeacherService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LMS.Pages.Teacher;

[Authorize(Policy = "TeacherOnly")]
public class TeacherScheduleModel : PageModel
{
    private readonly IClassManagementService _classService;
    private readonly IClassScheduleService _scheduleService;
    private readonly ITimeSlotService _timeSlotService;
    private readonly IRoomService _roomService;

    public TeacherScheduleModel(
        IClassManagementService classService,
        IClassScheduleService scheduleService,
        ITimeSlotService timeSlotService,
        IRoomService roomService)
    {
        _classService = classService;
        _scheduleService = scheduleService;
        _timeSlotService = timeSlotService;
        _roomService = roomService;
    }

    public IReadOnlyList<Class> TeacherClasses { get; private set; } = new List<Class>();
    public Class? SelectedClass { get; private set; }
    public IReadOnlyList<ClassSchedule> Schedules { get; private set; } = new List<ClassSchedule>();
    public IReadOnlyList<TimeSlot> TimeSlots { get; private set; } = new List<TimeSlot>();
    public IReadOnlyList<Room> Rooms { get; private set; } = new List<Room>();
    public IReadOnlyCollection<ScheduleConflict> Conflicts { get; private set; } = Array.Empty<ScheduleConflict>();
    public IReadOnlyList<Subject> Subjects { get; private set; } = new List<Subject>();
    public IReadOnlyList<Center> Centers { get; private set; } = new List<Center>();

    [BindProperty]
    public ScheduleInput Input { get; set; } = new();

    [BindProperty]
    public CreateClassInput NewClass { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public bool ShowCreateClass { get; set; }

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? classId, CancellationToken ct = default)
    {
        if (!TryGetTeacherId(out var teacherId))
        {
            return Challenge();
        }

        await LoadReferenceDataAsync(teacherId, classId, ct);

        if (!TeacherClasses.Any())
        {
            Message ??= "Bạn chưa có lớp nào. Tạo lớp mới để bắt đầu lên lịch.";
            ShowCreateClass = true;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostScheduleAsync(CancellationToken ct = default)
    {
        if (!TryGetTeacherId(out var teacherId))
        {
            return Challenge();
        }

        ShowCreateClass = false;
        ClearCreateClassValidation();

        if (Input.ClassId == Guid.Empty)
        {
            ModelState.AddModelError(nameof(Input.ClassId), "Class selection is required.");
        }

        if (Input.RoomId == Guid.Empty)
        {
            ModelState.AddModelError(nameof(Input.RoomId), "Room selection is required.");
        }

        if (Input.SessionDate == default)
        {
            ModelState.AddModelError(nameof(Input.SessionDate), "Session date is required.");
        }

        if (!ModelState.IsValid)
        {
            await LoadReferenceDataAsync(teacherId, Input.ClassId, ct);
            ErrorMessage = BuildValidationMessage();
            return Page();
        }

        await LoadReferenceDataAsync(teacherId, Input.ClassId, ct);

        if (SelectedClass is null)
        {
            ErrorMessage ??= "The selected class is no longer available.";
            return Page();
        }

        var isUnassignedClass = SelectedClass.TeacherId == Guid.Empty;

        if (!isUnassignedClass && SelectedClass.TeacherId != teacherId)
        {
            return Forbid();
        }

        if (isUnassignedClass)
        {
            try
            {
                await _classService.AssignTeacherAsync(SelectedClass.ClassId, teacherId, ct);
            }
            catch (System.InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
                Conflicts = Array.Empty<ScheduleConflict>();
                return Page();
            }

            await LoadReferenceDataAsync(teacherId, Input.ClassId, ct);

            if (SelectedClass is null || SelectedClass.TeacherId != teacherId)
            {
                ErrorMessage = "Unable to assign the class to your account.";
                return Page();
            }
        }

        var roomName = Rooms.FirstOrDefault(r => r.RoomId == Input.RoomId)?.RoomName;

        var schedule = new ClassSchedule
        {
            ClassId = Input.ClassId,
            SessionDate = Input.SessionDate,
            SlotId = Input.SlotId,
            StartTime = Input.StartTime,
            EndTime = Input.EndTime,
            RoomId = Input.RoomId,
            RoomName = roomName,
            ScheduleLabel = Input.Label,
            ScheduleNote = Input.Note,
            IsAutoGenerated = false,
            CreatedAt = DateTime.UtcNow
        };

        var operation = await _scheduleService.ScheduleAsync(schedule, saveNow: true, ct);

        if (!operation.Availability.IsAvailable)
        {
            Conflicts = operation.Availability.Conflicts;
            ErrorMessage = "Unable to schedule session. Resolve the conflicts below.";
            return Page();
        }

        Message = "Class session scheduled successfully.";
        return RedirectToPage(new { classId = Input.ClassId });
    }

    private string BuildValidationMessage()
    {
        var builder = new System.Text.StringBuilder();
        builder.Append("Please correct the highlighted errors.");

        var problems = ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .Select(kvp => new
            {
                Field = kvp.Key,
                Messages = kvp.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                    ? e.Exception?.Message
                    : e.ErrorMessage)
            })
            .Where(entry => entry.Messages.Any(m => !string.IsNullOrWhiteSpace(m)))
            .ToList();

        if (problems.Count > 0)
        {
            builder.Append(' ');
            builder.Append(string.Join(" | ", problems.Select(p =>
                $"{p.Field}: {string.Join(", ", p.Messages.Where(m => !string.IsNullOrWhiteSpace(m)))}")));
        }

        return builder.ToString();
    }

    public async Task<IActionResult> OnPostCreateClassAsync(CancellationToken ct = default)
    {
        if (!TryGetTeacherId(out var teacherId))
        {
            return Challenge();
        }

        ShowCreateClass = true;
        ClearScheduleValidation();

        ValidateNewClass();

        if (!ModelState.IsValid)
        {
            await LoadReferenceDataAsync(teacherId, null, ct);
            ErrorMessage = BuildValidationMessage();
            return Page();
        }

        try
        {
            var trimmedName = NewClass.ClassName?.Trim() ?? string.Empty;
            NewClass.ClassName = trimmedName;

            var created = await _classService.CreateSelfManagedClassAsync(
                teacherId,
                NewClass.CenterId,
                NewClass.SubjectId,
                NewClass.ClassName,
                NewClass.ClassAddress,
                ct);

            Message = "Tạo lớp thành công. Bạn có thể lên lịch ngay bây giờ.";
            return RedirectToPage(new { classId = created.ClassId });
        }
        catch (Exception ex)
        {
            await LoadReferenceDataAsync(teacherId, null, ct);
            ErrorMessage = ex.Message;
            return Page();
        }
    }

    private bool TryGetTeacherId(out Guid teacherId)
    {
        teacherId = Guid.Empty;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out teacherId);
    }

    private async Task LoadReferenceDataAsync(Guid teacherId, Guid? classId, CancellationToken ct)
    {
        await LoadCatalogsAsync(ct);

        TeacherClasses = await _classService.GetClassesForSchedulingAsync(teacherId, ct);

        var slotResult = await _timeSlotService.ListAsync(pageSize: 100, ct: ct);
        TimeSlots = slotResult.Items;

        if (!TeacherClasses.Any())
        {
            SelectedClass = null;
            Rooms = Array.Empty<Room>();
            Schedules = Array.Empty<ClassSchedule>();
            Conflicts = Array.Empty<ScheduleConflict>();
            Input.ClassId = Guid.Empty;
            ShowCreateClass = true;
            return;
        }

        SelectedClass = classId.HasValue
            ? TeacherClasses.FirstOrDefault(c => c.ClassId == classId.Value)
            : null;

        SelectedClass ??= TeacherClasses.First();

        if (Input == null)
        {
            Input = new ScheduleInput();
        }

        Input.ClassId = Input.ClassId != Guid.Empty ? Input.ClassId : SelectedClass.ClassId;

        if (Input.SessionDate == default)
        {
            Input.SessionDate = DateOnly.FromDateTime(DateTime.Today);
        }

        Rooms = await _roomService.GetRoomsByCenterIdAsync(SelectedClass.CenterId, isActive: true, ct);

        if (Input.RoomId == Guid.Empty && Rooms.Any())
        {
            Input.RoomId = Rooms.First().RoomId;
        }

        if (!Input.SlotId.HasValue && TimeSlots.Any())
        {
            Input.SlotId = TimeSlots.First().SlotId;
        }

        if (Input.SlotId.HasValue)
        {
            var slot = TimeSlots.FirstOrDefault(s => s.SlotId == Input.SlotId.Value);
            if (slot is not null)
            {
                Input.StartTime ??= slot.StartTime;
                Input.EndTime ??= slot.EndTime;
            }
        }

        Schedules = await _scheduleService.GetSchedulesByClassIdAsync(SelectedClass.ClassId, ct);
        Conflicts = Array.Empty<ScheduleConflict>();
    }

    private async Task LoadCatalogsAsync(CancellationToken ct)
    {
        Subjects = await _classService.GetSubjectsAsync(ct);
        Centers = await _classService.GetCentersAsync(ct);

        if (NewClass is null)
        {
            NewClass = new CreateClassInput();
        }

        if (NewClass.SubjectId == 0 && Subjects.Any())
        {
            NewClass.SubjectId = Subjects.First().SubjectId;
        }

        if (NewClass.CenterId == Guid.Empty && Centers.Any())
        {
            NewClass.CenterId = Centers.First().CenterId;
        }
    }

    private void ValidateNewClass()
    {
        ModelState.ClearValidationState(nameof(NewClass));
        ModelState.MarkFieldValid(nameof(NewClass));

        if (NewClass is null)
        {
            ModelState.AddModelError(nameof(NewClass), "Thông tin tạo lớp không hợp lệ.");
            return;
        }

        var trimmedName = NewClass.ClassName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            ModelState.AddModelError($"{nameof(NewClass)}.{nameof(CreateClassInput.ClassName)}", "Tên lớp là bắt buộc.");
        }
        else if (trimmedName.Length < 3 || trimmedName.Length > 150)
        {
            ModelState.AddModelError($"{nameof(NewClass)}.{nameof(CreateClassInput.ClassName)}", "Tên lớp phải có độ dài từ 3 đến 150 ký tự.");
        }

        if (NewClass.SubjectId <= 0)
        {
            ModelState.AddModelError($"{nameof(NewClass)}.{nameof(CreateClassInput.SubjectId)}", "Vui lòng chọn môn học.");
        }

        if (NewClass.CenterId == Guid.Empty)
        {
            ModelState.AddModelError($"{nameof(NewClass)}.{nameof(CreateClassInput.CenterId)}", "Vui lòng chọn trung tâm.");
        }

        if (!string.IsNullOrWhiteSpace(NewClass.ClassAddress) && NewClass.ClassAddress.Length > 300)
        {
            ModelState.AddModelError($"{nameof(NewClass)}.{nameof(CreateClassInput.ClassAddress)}", "Địa điểm tối đa 300 ký tự.");
        }
    }

    private void ClearCreateClassValidation()
    {
        ModelState.ClearValidationState(nameof(NewClass));
        ModelState.MarkFieldValid(nameof(NewClass));
        ModelState.Remove($"{nameof(NewClass)}.{nameof(CreateClassInput.ClassName)}");
        ModelState.Remove($"{nameof(NewClass)}.{nameof(CreateClassInput.SubjectId)}");
        ModelState.Remove($"{nameof(NewClass)}.{nameof(CreateClassInput.CenterId)}");
        ModelState.Remove($"{nameof(NewClass)}.{nameof(CreateClassInput.ClassAddress)}");
    }

    private void ClearScheduleValidation()
    {
        ModelState.ClearValidationState(nameof(Input));
        ModelState.MarkFieldValid(nameof(Input));
        ModelState.Remove($"{nameof(Input)}.{nameof(ScheduleInput.ClassId)}");
        ModelState.Remove($"{nameof(Input)}.{nameof(ScheduleInput.SessionDate)}");
        ModelState.Remove($"{nameof(Input)}.{nameof(ScheduleInput.RoomId)}");
        ModelState.Remove($"{nameof(Input)}.{nameof(ScheduleInput.SlotId)}");
        ModelState.Remove($"{nameof(Input)}.{nameof(ScheduleInput.StartTime)}");
        ModelState.Remove($"{nameof(Input)}.{nameof(ScheduleInput.EndTime)}");
        ModelState.Remove($"{nameof(Input)}.{nameof(ScheduleInput.Label)}");
        ModelState.Remove($"{nameof(Input)}.{nameof(ScheduleInput.Note)}");
    }

    public sealed class ScheduleInput
    {
        [Required]
        public Guid ClassId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly SessionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        public byte? SlotId { get; set; }

        [DataType(DataType.Time)]
        public TimeOnly? StartTime { get; set; }

        [DataType(DataType.Time)]
        public TimeOnly? EndTime { get; set; }

        [Required]
        public Guid RoomId { get; set; }

        [StringLength(150)]
        public string? Label { get; set; }

        [StringLength(150)]
        public string? Note { get; set; }
    }

    public sealed class CreateClassInput
    {
        public string ClassName { get; set; } = string.Empty;
        public long SubjectId { get; set; }
        public Guid CenterId { get; set; }
        public string? ClassAddress { get; set; }
    }
}
