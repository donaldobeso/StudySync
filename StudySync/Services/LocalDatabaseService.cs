using SQLite;
using StudySync.Shared.Models;

namespace StudySync.Services
{
    public class LocalDatabaseService
    {
        private SQLiteAsyncConnection? _db;

        private async Task InitAsync()
        {
            if (_db != null) return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "studysync.db");
            

            _db = new SQLiteAsyncConnection(dbPath);
            await _db.CreateTableAsync<Subject>();
            await _db.CreateTableAsync<Assignment>();
        }

        // Subjects
        public async Task<List<Subject>> GetSubjectsAsync(string userUid)
        {
            await InitAsync();
            return await _db!.Table<Subject>()
                .Where(s => s.UserUid == userUid)
                .ToListAsync();
        }

        public async Task SaveSubjectAsync(Subject subject)
        {
            await InitAsync();
            if (subject.Id == 0)
                await _db!.InsertAsync(subject);
            else
                await _db!.UpdateAsync(subject);
        }

        public async Task DeleteSubjectAsync(string firestoreId)
        {
            await InitAsync();
            var item = await _db!.Table<Subject>()
                .Where(s => s.FirestoreId == firestoreId)
                .FirstOrDefaultAsync();
            if (item != null)
                await _db!.DeleteAsync(item);
        }

        public async Task ClearAndSaveSubjectsAsync(List<Subject> subjects)
        {
            await InitAsync();
            await _db!.DeleteAllAsync<Subject>();
            await _db!.InsertAllAsync(subjects);
        }

        // Assignments
        public async Task<List<Assignment>> GetAssignmentsAsync(string userUid)
        {
            await InitAsync();
            return await _db!.Table<Assignment>()
                .Where(a => a.UserUid == userUid)
                .ToListAsync();
        }

        public async Task SaveAssignmentAsync(Assignment assignment)
        {
            await InitAsync();
            if (assignment.Id == 0)
                await _db!.InsertAsync(assignment);
            else
                await _db!.UpdateAsync(assignment);
        }

        public async Task DeleteAssignmentAsync(string firestoreId)
        {
            await InitAsync();
            var item = await _db!.Table<Assignment>()
                .Where(a => a.FirestoreId == firestoreId)
                .FirstOrDefaultAsync();
            if (item != null)
                await _db!.DeleteAsync(item);
        }

        public async Task ClearAndSaveAssignmentsAsync(List<Assignment> assignments, string userUid)
        {
            await InitAsync();
            // Delete only assignments for this user
            var existing = await _db!.Table<Assignment>().Where(a => a.UserUid == userUid).ToListAsync();
            foreach (var a in existing)
                await _db!.DeleteAsync(a);
            await _db!.InsertAllAsync(assignments);
        }

        public async Task MarkAssignmentCompleteAsync(string firestoreId, string userUid)
        {
            await InitAsync();
            var item = await _db!.Table<Assignment>()
                .Where(a => a.FirestoreId == firestoreId && a.UserUid == userUid)
                .FirstOrDefaultAsync();
            if (item != null)
            {
                item.IsCompleted = true;
                await _db!.UpdateAsync(item);
            }
        }
    }
}