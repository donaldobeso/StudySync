using StudySync.Shared.Services;
using StudySync.Shared.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudySync.ViewModels
{
    public partial class AssignmentViewModel : INotifyPropertyChanged
    {
        private readonly IAssignmentService _assignmentService;
        private readonly IAuthService _authService;
        private string _currentUserName = string.Empty;

        public string CurrentUserName
        {
            get => _currentUserName;
            set
            {
                if (_currentUserName != value)
                {
                    _currentUserName = value;
                    OnPropertyChanged();
                }
            }
        }

        public AssignmentViewModel(IAssignmentService assignmentService, IAuthService authService)
        {
            _assignmentService = assignmentService;
            _authService = authService;
            UpdateCurrentUser();
        }

        public void UpdateCurrentUser()
        {
            CurrentUserName = _authService.CurrentUser?.FullName ?? "User";
        }

        public IAuthService AuthService => _authService;

        public async Task LogoutAsync()
        {
            await _authService.LogoutAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
