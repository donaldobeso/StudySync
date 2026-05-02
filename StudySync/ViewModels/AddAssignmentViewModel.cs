using StudySync.Shared.Models;
using StudySync.Shared.Services;
using StudySync.Shared.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudySync.ViewModels
{
    public class AddAssignmentViewModel : INotifyPropertyChanged
    {
        private readonly IAssignmentService _assignmentService;
        private readonly IAuthService _authService;

        private string _title = string.Empty;
        private string _description = string.Empty;
        private string _subjectName = string.Empty;
        private string _selectedPriority = "Medium";
        private int _selectedHours = 1;
        private int _selectedMinutes = 0;
        private DateTime _dueDate = DateTime.Today.AddDays(7);
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;

        public string Title
        {
            get => _title;
            set { if (_title != value) { _title = value; OnPropertyChanged(); } }
        }

        public string Description
        {
            get => _description;
            set { if (_description != value) { _description = value; OnPropertyChanged(); } }
        }

        public string SubjectName
        {
            get => _subjectName;
            set { if (_subjectName != value) { _subjectName = value; OnPropertyChanged(); } }
        }

        public string SelectedPriority
        {
            get => _selectedPriority;
            set { if (_selectedPriority != value) { _selectedPriority = value; OnPropertyChanged(); } }
        }

        public int SelectedHours
        {
            get => _selectedHours;
            set
            {
                if (_selectedHours != value)
                {
                    _selectedHours = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(EstimatedTimePreview));
                }
            }
        }

        public int SelectedMinutes
        {
            get => _selectedMinutes;
            set
            {
                if (_selectedMinutes != value)
                {
                    _selectedMinutes = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(EstimatedTimePreview));
                }
            }
        }

        public DateTime DueDate
        {
            get => _dueDate;
            set { if (_dueDate != value) { _dueDate = value; OnPropertyChanged(); } }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { if (_errorMessage != value) { _errorMessage = value; OnPropertyChanged(); } }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { if (_isLoading != value) { _isLoading = value; OnPropertyChanged(); } }
        }

        public List<int> HourOptions => Enumerable.Range(0, 13).ToList(); // 0–12 hours
        public List<int> MinuteOptions => [0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55];

        public string EstimatedTimePreview
        {
            get
            {
                int total = (SelectedHours * 60) + SelectedMinutes;
                if (total == 0) return "⚠️ Please set a duration";
                if (SelectedHours == 0) return $"⏱️ {SelectedMinutes} minutes";
                if (SelectedMinutes == 0) return $"⏱️ {SelectedHours} hour{(SelectedHours > 1 ? "s" : "")}";
                return $"⏱️ {SelectedHours}h {SelectedMinutes}m";
            }
        }

        public List<string> PriorityOptions => ["Low", "Medium", "High"];

        public AddAssignmentViewModel(IAssignmentService assignmentService, IAuthService authService)
        {
            _assignmentService = assignmentService;
            _authService = authService;
        }

        public async Task SaveAssignmentAsync()
        {
            ErrorMessage = string.Empty;

            if (!Validator.IsNotEmpty(Title))
            {
                ErrorMessage = "⚠️ Please enter the assignment title.";
                return;
            }

            int totalMinutes = (SelectedHours * 60) + SelectedMinutes;
            if (totalMinutes == 0)
            {
                ErrorMessage = "⚠️ Please set an estimated duration.";
                return;
            }

            if (_authService.CurrentUser == null)
            {
                ErrorMessage = "⚠️ Not logged in.";
                return;
            }

            IsLoading = true;

            var assignment = new Assignment
            {
                Title = Title,
                Description = Description,
                SubjectName = SubjectName,
                Priority = SelectedPriority,
                EstimatedMinutes = totalMinutes,
                DueDate = DueDate,
                IsCompleted = false
            };

            await _assignmentService.AddAssignmentAsync(assignment, _authService.CurrentUser.Email);

            // Reset
            Title = string.Empty;
            Description = string.Empty;
            SubjectName = string.Empty;
            SelectedPriority = "Medium";
            SelectedHours = 1;
            SelectedMinutes = 0;
            DueDate = DateTime.Today.AddDays(7);

            IsLoading = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}