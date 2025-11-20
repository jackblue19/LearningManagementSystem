using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LMS.Models.ViewModels.StudentService;
using LMS.Services.Interfaces.StudentService;

namespace LMS.Pages.Student.temp;

public class ScheduleModel : PageModel
{
    private readonly IStudentScheduleService _scheduleSvc;

    public ScheduleModel(IStudentScheduleService scheduleSvc)
    {
        _scheduleSvc = scheduleSvc;
    }

    [BindProperty(SupportsGet = true)]
    public Guid StudentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? From { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? To { get; set; }

    public IReadOnlyList<StudentScheduleItemVm> Items { get; set; } = Array.Empty<StudentScheduleItemVm>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        var from = From ?? DateOnly.FromDateTime(DateTime.Today);
        var to = To ?? from.AddDays(14);
        Items = await _scheduleSvc.GetScheduleAsync(StudentId, from, to, ct);
        From = from; To = to;
    }
}
