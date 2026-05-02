using Plugin.Firebase.Firestore;
using StudySync.Shared.Models;
using StudySync.Shared.Services;
using System.Collections.Generic;

namespace StudySync.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly LocalDatabaseService _localDb;

        public AssignmentService(LocalDatabaseService localDb)
        {
            _localDb = localDb;
        }

        public async Task<List<Assignment>> GetAssignmentsAsync(string userUid)
        {
            try
            {
                var snapshot = await CrossFirebaseFirestore.Current
                    .GetCollection($"users/{userUid}/assignments")
                    .GetDocumentsAsync<Assignment>();

                var assignments = snapshot.Documents
                    .Select(doc =>
                    {
                        var a = doc.Data;
                        a.FirestoreId = doc.Reference.Id;
                        a.UserUid = userUid;
                        return a;
                    })
                    .ToList();

                await _localDb.ClearAndSaveAssignmentsAsync(assignments, userUid);
                return assignments;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firestore GetAssignments error: {ex.Message}");
                return await _localDb.GetAssignmentsAsync(userUid);
            }
        }

        public async Task AddAssignmentAsync(Assignment assignment, string userUid)
        {
            assignment.UserUid = userUid;

            try
            {
                // Ensure parent user document exists
                await CrossFirebaseFirestore.Current
                    .GetCollection("users")
                    .GetDocument(userUid)
                    .SetDataAsync(new Dictionary<string, object>
                    {
                { "email", userUid },
                { "exists", true }
                    });

                var docRef = CrossFirebaseFirestore.Current
                    .GetCollection($"users/{userUid}/assignments")
                    .CreateDocument();

                assignment.FirestoreId = docRef.Id;
                await docRef.SetDataAsync(assignment);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firestore AddAssignment error: {ex.Message}");
            }

            await _localDb.SaveAssignmentAsync(assignment);
        }

        public async Task DeleteAssignmentAsync(string firestoreId, string userUid)
        {
            try
            {
                await CrossFirebaseFirestore.Current
                    .GetCollection($"users/{userUid}/assignments")
                    .GetDocument(firestoreId)
                    .DeleteDocumentAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firestore DeleteAssignment error: {ex.Message}");
            }

            await _localDb.DeleteAssignmentAsync(firestoreId);
        }

        public async Task UpdateAssignmentAsync(Assignment assignment, string userUid)
        {
            try
            {
                await CrossFirebaseFirestore.Current
                    .GetCollection($"users/{userUid}/assignments")
                    .GetDocument(assignment.FirestoreId)
                    .SetDataAsync(assignment);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firestore UpdateAssignment error: {ex.Message}");
            }

            await _localDb.SaveAssignmentAsync(assignment);
        }

        public async Task MarkCompleteAsync(string firestoreId, string userUid)
        {
            try
            {
                var updates = new Dictionary<object, object>
        {
            { "isCompleted", true }
        };

                await CrossFirebaseFirestore.Current
                    .GetCollection($"users/{userUid}/assignments")
                    .GetDocument(firestoreId)
                    .UpdateDataAsync(updates);

                await _localDb.MarkAssignmentCompleteAsync(firestoreId, userUid);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MarkComplete FAILED: {ex.Message}");
                await App.Current!.Windows[0].Page!.DisplayAlertAsync("Firestore Error", ex.Message, "OK");
            }
        }
    }
}