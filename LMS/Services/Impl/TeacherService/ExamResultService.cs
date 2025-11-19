using LMS.Repositories.Interfaces.Assessment;
using LMS.Services.Interfaces.TeacherService;

namespace LMS.Services.Impl.TeacherService;

public class ExamResultService : IExamResultService
{
    private readonly IExamResultRepository _examResultRepo;

    public ExamResultService(IExamResultRepository examResultRepo)
    {
        _examResultRepo = examResultRepo;
    }
}
