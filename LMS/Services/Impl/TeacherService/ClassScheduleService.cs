using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LMS.Models.Entities;
using LMS.Models.ViewModels.Scheduling;
using LMS.Repositories.Interfaces.Academic;
using LMS.Repositories.Interfaces.Scheduling;
using LMS.Services.Impl;
using LMS.Services.Interfaces.TeacherService;
using Microsoft.EntityFrameworkCore;
using LMS.Data;

namespace LMS.Services.Impl.TeacherService;

public class ClassScheduleService : CrudService<ClassSchedule, long>, IClassScheduleService
{
    private readonly IClassScheduleRepository _classScheduleRepository;
    private readonly IClassScheduleRepository _scheduleRepo;
    private readonly IClassRepository _classRepository;
    private readonly ITimeSlotRepository _timeSlotRepository;
    private readonly IRoomAvailabilityRepository _roomAvailabilityRepository;
    private readonly ITeacherAvailabilityRepository _teacherAvailabilityRepository;
    private readonly CenterDbContext _db;

    public ClassScheduleService(
        IClassScheduleRepository classScheduleRepository,
        IClassRepository classRepository,
        ITimeSlotRepository timeSlotRepository,
        IRoomAvailabilityRepository roomAvailabilityRepository,
        ITeacherAvailabilityRepository teacherAvailabilityRepository,
        IClassScheduleRepository scheduleRepo,
        CenterDbContext db)
        : base(classScheduleRepository)
    {
        _classScheduleRepository = classScheduleRepository;
        _scheduleRepo = scheduleRepo;
        _classRepository = classRepository;
        _timeSlotRepository = timeSlotRepository;
        _roomAvailabilityRepository = roomAvailabilityRepository;
        _teacherAvailabilityRepository = teacherAvailabilityRepository;
        _db = db;
    }

    public Task<IReadOnlyList<ClassSchedule>> GetClassScheduleAsync(
        Guid classId,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken ct = default)
        => _classScheduleRepository.GetByClassAsync(classId, from, to, ct);

    public async Task<ScheduleAvailabilityResult> CheckAvailabilityAsync(
        ClassSchedule schedule,
        CancellationToken ct = default)
    {
        var context = await BuildValidationContextAsync(schedule, ct);
        return await ValidateAsync(context, ct);
    }

    public async Task<ScheduleOperationResult> ScheduleAsync(
        ClassSchedule schedule,
        bool saveNow = true,
        CancellationToken ct = default)
    {
        var context = await BuildValidationContextAsync(schedule, ct);
        var availability = await ValidateAsync(context, ct);

        if (!availability.IsAvailable)
        {
            return ScheduleOperationResult.FromAvailability(availability);
        }

        schedule.StartTime = context.StartTime;
        schedule.EndTime = context.EndTime;

        var created = await _classScheduleRepository.AddAsync(schedule, saveNow, ct);
        return ScheduleOperationResult.FromAvailability(availability, created);
    }

    private async Task<ScheduleAvailabilityResult> ValidateAsync(
        ScheduleValidationContext context,
        CancellationToken ct)
    {
        var conflicts = new List<ScheduleConflict>(context.Conflicts);

        if (!context.ReadyForAvailabilityChecks)
        {
            return conflicts.Count == 0
                ? ScheduleAvailabilityResult.Success()
                : ScheduleAvailabilityResult.Failure(conflicts);
        }

        var start = context.StartTime!.Value;
        var end = context.EndTime!.Value;
        var ignoreId = context.IgnoreScheduleId;

        if (await _classScheduleRepository.HasClassConflictAsync(
                context.ClassId,
                context.SessionDate,
                start,
                end,
                context.SlotId,
                ignoreId,
                ct))
        {
            AddConflict(conflicts, ScheduleConflictCodes.ClassConflict,
                "Class already has a session scheduled for the requested time.");
        }

        if (await _classScheduleRepository.HasTeacherConflictAsync(
                context.TeacherId,
                context.SessionDate,
                start,
                end,
                context.SlotId,
                ignoreId,
                ct))
        {
            AddConflict(conflicts, ScheduleConflictCodes.TeacherConflict,
                "Teacher is already assigned to another class during the requested time.");
        }

        var hasTeacherAvailability = await _teacherAvailabilityRepository.HasAvailabilityWindowAsync(
            context.TeacherId,
            context.DayOfWeek,
            start,
            end,
            ct);

        if (!hasTeacherAvailability)
        {
            AddConflict(conflicts, ScheduleConflictCodes.TeacherAvailabilityNotConfigured,
                "Teacher availability does not cover the requested time range.");
        }

        if (context.RoomId.HasValue)
        {
            var roomId = context.RoomId.Value;

            if (await _classScheduleRepository.HasRoomConflictAsync(
                    roomId,
                    context.SessionDate,
                    start,
                    end,
                    context.SlotId,
                    ignoreId,
                    ct))
            {
                AddConflict(conflicts, ScheduleConflictCodes.RoomConflict,
                    "Room is already booked for the requested time.");
            }

            var hasRoomAvailability = await _roomAvailabilityRepository.HasAvailabilityWindowAsync(
                roomId,
                context.DayOfWeek,
                start,
                end,
                ct);

            if (!hasRoomAvailability)
            {
                AddConflict(conflicts, ScheduleConflictCodes.RoomUnavailable,
                    "Room availability does not cover the requested time range.");
            }
        }

        return conflicts.Count == 0
            ? ScheduleAvailabilityResult.Success()
            : ScheduleAvailabilityResult.Failure(conflicts);
    }

    private async Task<ScheduleValidationContext> BuildValidationContextAsync(
        ClassSchedule schedule,
        CancellationToken ct)
    {
        if (schedule is null) throw new ArgumentNullException(nameof(schedule));

        var context = new ScheduleValidationContext(schedule)
        {
            IgnoreScheduleId = schedule.ScheduleId > 0 ? schedule.ScheduleId : null
        };

        var classEntity = await _classRepository.GetByIdAsync(schedule.ClassId, ct: ct);
        if (classEntity is null)
        {
            AddConflict(context.Conflicts, ScheduleConflictCodes.ClassNotFound,
                "Class could not be found.");
        }
        else
        {
            context.Class = classEntity;
        }

        await PopulateTimeContextAsync(context, ct);

        if (!schedule.RoomId.HasValue)
        {
            AddConflict(context.Conflicts, ScheduleConflictCodes.RoomNotSpecified,
                "Room must be specified to schedule a session.");
        }

        return context;
    }

    private async Task PopulateTimeContextAsync(
        ScheduleValidationContext context,
        CancellationToken ct)
    {
        var schedule = context.Schedule;
        var start = schedule.StartTime;
        var end = schedule.EndTime;

        if (schedule.SlotId.HasValue)
        {
            var slot = await _timeSlotRepository.GetByIdAsync(schedule.SlotId.Value, ct: ct);
            if (slot is null)
            {
                AddConflict(context.Conflicts, ScheduleConflictCodes.MissingTimeDefinition,
                    "Requested time slot could not be found.");
            }
            else
            {
                context.Slot = slot;
                start ??= slot.StartTime;
                end ??= slot.EndTime;
            }
        }

        if (!start.HasValue || !end.HasValue)
        {
            AddConflict(context.Conflicts, ScheduleConflictCodes.MissingTimeDefinition,
                "Start and end times are required to schedule a class.");
        }
        else if (start.Value >= end.Value)
        {
            AddConflict(context.Conflicts, ScheduleConflictCodes.InvalidTimeRange,
                "End time must be later than start time.");
        }

        context.StartTime = start;
        context.EndTime = end;
    }

    private static void AddConflict(ICollection<ScheduleConflict> conflicts, string code, string message)
    {
        if (conflicts.Any(c => string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase))) return;
        conflicts.Add(ScheduleConflict.From(code, message));
    }

    private sealed class ScheduleValidationContext
    {
        public ScheduleValidationContext(ClassSchedule schedule)
        {
            Schedule = schedule;
        }

        public ClassSchedule Schedule { get; }
        public Class? Class { get; set; }
        public TimeSlot? Slot { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public long? IgnoreScheduleId { get; set; }
        public List<ScheduleConflict> Conflicts { get; } = new();

        public Guid ClassId => Schedule.ClassId;
        public DateOnly SessionDate => Schedule.SessionDate;
        public byte? SlotId => Schedule.SlotId;
        public Guid? RoomId => Schedule.RoomId;
        public Guid TeacherId => Class?.TeacherId ?? Guid.Empty;
        public byte DayOfWeek => (byte)Schedule.SessionDate.DayOfWeek;
        public bool ReadyForAvailabilityChecks => Class is not null && StartTime.HasValue && EndTime.HasValue && RoomId.HasValue;
    }

    public async Task<ClassSchedule?> GetScheduleByIdAsync(
        long scheduleId,
        CancellationToken ct = default)
    {
        return await _db.ClassSchedules
            .AsNoTracking()
            .Include(s => s.Class)
            .Include(s => s.Room)
            .Include(s => s.Slot)
            .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId, ct);
    }

    public async Task<IReadOnlyList<ClassSchedule>> GetSchedulesByClassIdAsync(
        Guid classId,
        CancellationToken ct = default)
    {
        return await _db.ClassSchedules
            .AsNoTracking()
            .Where(s => s.ClassId == classId)
            .Include(s => s.Room)
            .Include(s => s.Slot)
            .OrderBy(s => s.SessionDate)
            .ToListAsync(ct);
    }
}
