using StudySync.Shared.Models;
using StudySync.Shared.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudySync.ViewModels
{
    public partial class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ISubjectService _subjectService;
        private readonly IAuthService _authService;

        private string _welcomeMessage = string.Empty;
        private int _totalAssignments = 0;
        private int _completedAssignments = 0;
        private int _pendingAssignments = 0;
        private int _overdueAssignments = 0;
        private double _completionPercentage = 0;
        private int _totalSubjects = 0;
        private List<Assignment> _upcomingAssignments = [];
        private List<Assignment> _overdueAssignmentList = [];

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set { if (_welcomeMessage != value) { _welcomeMessage = value; OnPropertyChanged(); } }
        }

        public int TotalAssignments
        {
            get => _totalAssignments;
            set { if (_totalAssignments != value) { _totalAssignments = value; OnPropertyChanged(); } }
        }

        public int CompletedAssignments
        {
            get => _completedAssignments;
            set { if (_completedAssignments != value) { _completedAssignments = value; OnPropertyChanged(); } }
        }

        public int PendingAssignments
        {
            get => _pendingAssignments;
            set { if (_pendingAssignments != value) { _pendingAssignments = value; OnPropertyChanged(); } }
        }

        public int OverdueAssignments
        {
            get => _overdueAssignments;
            set { if (_overdueAssignments != value) { _overdueAssignments = value; OnPropertyChanged(); } }
        }

        public double CompletionPercentage
        {
            get => _completionPercentage;
            set { if (_completionPercentage != value) { _completionPercentage = value; OnPropertyChanged(); } }
        }

        public int TotalSubjects
        {
            get => _totalSubjects;
            set { if (_totalSubjects != value) { _totalSubjects = value; OnPropertyChanged(); } }
        }

        public List<Assignment> UpcomingAssignments
        {
            get => _upcomingAssignments;
            set { if (_upcomingAssignments != value) { _upcomingAssignments = value; OnPropertyChanged(); } }
        }

        public List<Assignment> OverdueAssignmentList
        {
            get => _overdueAssignmentList;
            set { if (_overdueAssignmentList != value) { _overdueAssignmentList = value; OnPropertyChanged(); } }
        }

        public bool HasOverdue => _overdueAssignmentList.Count > 0;

        public DashboardViewModel(IAssignmentService assignmentService, ISubjectService subjectService, IAuthService authService)
        {
            _assignmentService = assignmentService;
            _subjectService = subjectService;
            _authService = authService;
        }

        public async Task LoadDashboardDataAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) return;

            WelcomeMessage = $"Welcome back, {currentUser.FullName}! 👋";

            var allAssignments = await _assignmentService.GetAssignmentsAsync(currentUser.Email);
            var today = DateTime.Today;

            TotalAssignments = allAssignments.Count;
            CompletedAssignments = allAssignments.Count(a => a.IsCompleted);
            PendingAssignments = allAssignments.Count(a => !a.IsCompleted);

            // Overdue = not completed and past due date
            var overdueList = allAssignments
                .Where(a => !a.IsCompleted && a.DueDate.Date < today)
                .OrderBy(a => a.DueDate)
                .ToList();

            OverdueAssignmentList = overdueList;
            OverdueAssignments = overdueList.Count;
            OnPropertyChanged(nameof(HasOverdue));

            CompletionPercentage = TotalAssignments > 0
                ? (double)CompletedAssignments / TotalAssignments
                : 0;

            var subjects = await _subjectService.GetSubjectsAsync(currentUser.Email);
            TotalSubjects = subjects.Count;

            // Upcoming = not completed, not overdue, next 3 by due date
            UpcomingAssignments = allAssignments
                .Where(a => !a.IsCompleted && a.DueDate.Date >= today)
                .OrderBy(a => a.DueDate)
                .Take(3)
                .ToList();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}