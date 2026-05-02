using SQLite;
using Plugin.Firebase.Firestore;

namespace StudySync.Shared.Models
{
    [SQLite.Table("Assignments")]
    public class Assignment : IFirestoreObject
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id { get; set; }

        [FirestoreDocumentId]
        public string FirestoreId { get; set; } = string.Empty;

        [FirestoreProperty("userUid")]
        public string UserUid { get; set; } = string.Empty;

        [FirestoreProperty("title")]
        public string Title { get; set; } = string.Empty;

        [FirestoreProperty("description")]
        public string Description { get; set; } = string.Empty;

        [FirestoreProperty("subjectName")]
        public string SubjectName { get; set; } = string.Empty;

        [FirestoreProperty("priority")]
        public string Priority { get; set; } = "Medium";

        [FirestoreProperty("estimatedMinutes")]
        public int EstimatedMinutes { get; set; } = 60;

        [FirestoreProperty("dueDateString")]
        public string DueDateString { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");

        [SQLite.Ignore]
        public DateTime DueDate
        {
            get => DateTime.TryParse(DueDateString, out var d) ? d : DateTime.Today;
            set => DueDateString = value.ToString("yyyy-MM-dd");
        }

        [FirestoreProperty("isCompleted")]
        public bool IsCompleted { get; set; }

        // Computed for display
        [SQLite.Ignore]
        public string EstimatedTimeDisplay => EstimatedMinutes >= 60
            ? $"{EstimatedMinutes / 60}h {EstimatedMinutes % 60}m"
            : $"{EstimatedMinutes}m";
    }
}