using LMS.Repositories.Interfaces.Assessment;
using LMS.Services.Interfaces.TeacherService;

namespace LMS.Services.Impl.TeacherService;

public class ExamService : IExamService
{
    private readonly IExamRepository _examRepo;

    public ExamService(IExamRepository examRepo)
    {
        _examRepo = examRepo;
    }
}
