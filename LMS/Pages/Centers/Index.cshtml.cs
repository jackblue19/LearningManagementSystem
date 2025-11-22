using Microsoft.AspNetCore.Mvc.RazorPages;
using LMS.Services.Centers;
using LMS.Models.Entities;

namespace LMS.Pages.Centers;

public sealed class IndexModel : PageModel
{
    private readonly ICenterBrowseService _svc;
    public IndexModel(ICenterBrowseService svc) => _svc = svc;

    public IReadOnlyList<Center> Centers { get; private set; } = Array.Empty<Center>();

    public async Task OnGetAsync(CancellationToken ct) =>
        Centers = await _svc.ListCentersAsync(ct);
}
