using Plugin.Firebase.Firestore;
using StudySync.Shared.Models;
using StudySync.Shared.Services;

namespace StudySync.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly LocalDatabaseService _localDb;

        public SubjectService(LocalDatabaseService localDb)
        {
            _localDb = localDb;
        }

        public async Task<List<Subject>> GetSubjectsAsync(string userUid)
        {
            try
            {
                var snapshot = await CrossFirebaseFirestore.Current
                    .GetCollection("users")
                    .GetDocument(userUid)
                    .GetCollection("subjects")
                    .GetDocumentsAsync<Subject>();

                var subjects = snapshot.Documents
                    .Select(doc =>
                    {
                        var s = doc.Data;
                        s.FirestoreId = doc.Reference.Id;
                        s.UserUid = userUid;
                        return s;
                    })
                    .ToList();

                await _localDb.ClearAndSaveSubjectsAsync(subjects);
                return subjects;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firestore GetSubjects error: {ex.Message}");
                return await _localDb.GetSubjectsAsync(userUid);
            }
        }

        public async Task AddSubjectAsync(Subject subject, string userUid)
        {
            subject.UserUid = userUid;

            try
            {
                // Ensure parent user document exists
                var userDocRef = CrossFirebaseFirestore.Current
                    .GetCollection("users")
                    .GetDocument(userUid);

                await userDocRef.SetDataAsync(new Dictionary<string, object>
        {
            { "email", userUid },
            { "exists", true }
        });

                var docRef = CrossFirebaseFirestore.Current
                    .GetCollection("users")
                    .GetDocument(userUid)
                    .GetCollection("subjects")
                    .CreateDocument();

                subject.FirestoreId = docRef.Id;
                await docRef.SetDataAsync(subject);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firestore AddSubject error: {ex.Message}");
            }

            await _localDb.SaveSubjectAsync(subject);
        }

        public async Task UpdateSubjectAsync(Subject subject, string userUid)
        {
            try
            {
                await CrossFirebaseFirestore.Current
                    .GetCollection("users")
                    .GetDocument(userUid)
                    .GetCollection("subjects")
                    .GetDocument(subject.FirestoreId)
                    .SetDataAsync(subject);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firestore UpdateSubject error: {ex.Message}");
            }

            await _localDb.SaveSubjectAsync(subject);
        }

        public async Task DeleteSubjectAsync(string firestoreId, string userUid)
        {
            try
            {
                await CrossFirebaseFirestore.Current
                    .GetCollection("users")
                    .GetDocument(userUid)
                    .GetCollection("subjects")
                    .GetDocument(firestoreId)
                    .DeleteDocumentAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firestore DeleteSubject error: {ex.Message}");
            }

            await _localDb.DeleteSubjectAsync(firestoreId);
        }
    }
}