using System;
using System.Collections.Generic;
using System.Linq;
using LMS.Models.Entities;

namespace LMS.Models.ViewModels.Scheduling;

public static class ScheduleConflictCodes
{
    public const string ClassNotFound = nameof(ClassNotFound);
    public const string MissingTimeDefinition = nameof(MissingTimeDefinition);
    public const string InvalidTimeRange = nameof(InvalidTimeRange);
    public const string RoomNotSpecified = nameof(RoomNotSpecified);
    public const string RoomConflict = nameof(RoomConflict);
    public const string RoomUnavailable = nameof(RoomUnavailable);
    public const string TeacherConflict = nameof(TeacherConflict);
    public const string TeacherUnavailable = nameof(TeacherUnavailable);
    public const string TeacherAvailabilityNotConfigured = nameof(TeacherAvailabilityNotConfigured);
    public const string ClassConflict = nameof(ClassConflict);
}

public sealed record ScheduleConflict(string Code, string Message)
{
    public static ScheduleConflict From(string code, string message) => new(code, message);
}

public sealed class ScheduleAvailabilityResult
{
    public bool IsAvailable { get; }
    public IReadOnlyCollection<ScheduleConflict> Conflicts { get; }

    private ScheduleAvailabilityResult(bool isAvailable, IReadOnlyCollection<ScheduleConflict> conflicts)
    {
        IsAvailable = isAvailable;
        Conflicts = conflicts;
    }

    public static ScheduleAvailabilityResult Success()
        => new(true, Array.Empty<ScheduleConflict>());

    public static ScheduleAvailabilityResult Failure(IEnumerable<ScheduleConflict> conflicts)
        => new(false, conflicts?.ToArray() ?? Array.Empty<ScheduleConflict>());

    public static ScheduleAvailabilityResult Failure(params ScheduleConflict[] conflicts)
        => new(false, conflicts?.Length > 0 ? conflicts : Array.Empty<ScheduleConflict>());
}

public sealed class ScheduleOperationResult
{
    public ScheduleAvailabilityResult Availability { get; }
    public ClassSchedule? CreatedSchedule { get; }
    public bool Succeeded => Availability.IsAvailable && CreatedSchedule is not null;

    private ScheduleOperationResult(ScheduleAvailabilityResult availability, ClassSchedule? schedule)
    {
        Availability = availability;
        CreatedSchedule = schedule;
    }

    public static ScheduleOperationResult Success(ClassSchedule schedule)
        => new(ScheduleAvailabilityResult.Success(), schedule);

    public static ScheduleOperationResult Failure(IEnumerable<ScheduleConflict> conflicts)
        => new(ScheduleAvailabilityResult.Failure(conflicts), null);

    public static ScheduleOperationResult FromAvailability(ScheduleAvailabilityResult availability, ClassSchedule? schedule = null)
        => new(availability, schedule);
}
