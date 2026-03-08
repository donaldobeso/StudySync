// Models/Assignment.cs
namespace StudySync.Shared.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Priority { get; set; } = "Medium"; // Low, Medium, High
        public bool IsCompleted { get; set; }
        public string SubjectName { get; set; } = string.Empty;
    }
}