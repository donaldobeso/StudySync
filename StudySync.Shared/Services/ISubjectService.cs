using StudySync.Shared.Models;

namespace StudySync.Shared.Services
{
    public interface ISubjectService
    {
        Task<List<Subject>> GetSubjectsAsync(string userUid);
        Task AddSubjectAsync(Subject subject, string userUid);
        Task DeleteSubjectAsync(string firestoreId, string userUid);
        Task UpdateSubjectAsync(Subject subject, string userUid);
    }
}