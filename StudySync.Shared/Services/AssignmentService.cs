// Services/AssignmentService.cs
using StudySync.Shared.Models;

namespace StudySync.Shared.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly List<Assignment> _assignments = new();

        public List<Assignment> GetAssignments() => _assignments;

        public void AddAssignment(Assignment assignment)
        {
            assignment.Id = _assignments.Count + 1;
            _assignments.Add(assignment);
        }

        public void MarkComplete(int id)
        {
            var item = _assignments.FirstOrDefault(a => a.Id == id);
            if (item != null) item.IsCompleted = true;
        }
    }
}