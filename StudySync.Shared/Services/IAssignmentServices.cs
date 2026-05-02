using StudySync.Shared.Models;

namespace StudySync.Shared.Services
{
    public interface IAssignmentService
    {
        Task<List<Assignment>> GetAssignmentsAsync(string userUid);
        Task AddAssignmentAsync(Assignment assignment, string userUid);
        Task MarkCompleteAsync(string firestoreId, string userUid);
        Task DeleteAssignmentAsync(string firestoreId, string userUid);
        Task UpdateAssignmentAsync(Assignment assignment, string userUid);
    }
}