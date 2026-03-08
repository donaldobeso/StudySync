// Services/IAssignmentService.cs
using StudySync.Shared.Models;

namespace StudySync.Shared.Services
{
    public interface IAssignmentService
    {
        List<Assignment> GetAssignments();
        void AddAssignment(Assignment assignment);
        void MarkComplete(int id);
    }
}