using SQLite;
using Plugin.Firebase.Firestore;

namespace StudySync.Shared.Models
{
    [SQLite.Table("Subjects")]
    public class Subject : IFirestoreObject
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id { get; set; }

        [FirestoreDocumentId]
        public string FirestoreId { get; set; } = string.Empty;

        [FirestoreProperty("userUid")]
        public string UserUid { get; set; } = string.Empty;

        [FirestoreProperty("name")]
        public string Name { get; set; } = string.Empty;

        [FirestoreProperty("instructor")]
        public string Instructor { get; set; } = string.Empty;

        [FirestoreProperty("room")]
        public string Room { get; set; } = string.Empty;

        [FirestoreProperty("schedule")]
        public string Schedule { get; set; } = string.Empty;

        [FirestoreProperty("classDays")]
        public string ClassDays { get; set; } = string.Empty;

        [FirestoreProperty("classStartTime")]
        public string ClassStartTime { get; set; } = string.Empty;

        [FirestoreProperty("classEndTime")]
        public string ClassEndTime { get; set; } = string.Empty;

        [FirestoreProperty("color")]
        public string Color { get; set; } = "#4A90D9";
    }
}