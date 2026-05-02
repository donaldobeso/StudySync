using Plugin.LocalNotification;
using StudySync.Services;
using StudySync.Shared.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;

namespace StudySync.ViewModels
{
    public partial class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly NotificationService _notificationService;
        private readonly IAssignmentService _assignmentService;
        private readonly ISubjectService _subjectService;
        private bool _isDarkTheme;
        private bool _isLoading = false;
        private bool _notificationsEnabled;
        private string _userFullName = string.Empty;
        private string _userEmail = string.Empty;

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (_isDarkTheme != value)
                {
                    _isDarkTheme = value;
                    OnPropertyChanged();
                    ApplyTheme(value);
                }
            }
        }

        public bool NotificationsEnabled
        {
            get => _notificationsEnabled;
            set
            {
                if (_notificationsEnabled != value)
                {
                    _notificationsEnabled = value;
                    OnPropertyChanged();
                    _ = OnNotificationsToggledAsync(value);
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsNotLoading));
                }
            }
        }

        public bool IsNotLoading => !IsLoading;

        public string UserFullName
        {
            get => _userFullName;
            set
            {
                if (_userFullName != value)
                {
                    _userFullName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string UserEmail
        {
            get => _userEmail;
            set
            {
                if (_userEmail != value)
                {
                    _userEmail = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LogoutCommand { get; }
        public ICommand EditFullNameCommand { get; }

        public SettingsViewModel(IAuthService authService, NotificationService notificationService,
            IAssignmentService assignmentService, ISubjectService subjectService)
        {
            _authService = authService;
            _notificationService = notificationService;
            _assignmentService = assignmentService;
            _subjectService = subjectService;
            LogoutCommand = new Command(async () => await LogoutAsync(), () => !IsLoading);
            EditFullNameCommand = new Command(async () => await EditFullNameAsync(), () => !IsLoading);
            LoadThemePreference();
            LoadUserProfile();
            LoadNotificationPreference();
        }

        private void LoadThemePreference()
        {
            var savedTheme = Preferences.Get("app_theme", "light");
            _isDarkTheme = savedTheme.ToLower() == "dark";
            OnPropertyChanged(nameof(IsDarkTheme));
        }

        private void LoadNotificationPreference()
        {
            _notificationsEnabled = Preferences.Get("notifications_enabled", false);
            OnPropertyChanged(nameof(NotificationsEnabled));
        }

        private static void ApplyTheme(bool isDark)
        {
            Application.Current?.UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;
            Preferences.Set("app_theme", isDark ? "dark" : "light");
        }

        private async Task OnNotificationsToggledAsync(bool enabled)
        {
            Preferences.Set("notifications_enabled", enabled);

            if (!enabled)
            {
                LocalNotificationCenter.Current.CancelAll();
                return;
            }

            if (_authService.CurrentUser == null) return;

            try
            {
                await _notificationService.RequestPermissionAsync();
                var assignments = await _assignmentService.GetAssignmentsAsync(_authService.CurrentUser.Email);
                var subjects = await _subjectService.GetSubjectsAsync(_authService.CurrentUser.Email);
                await _notificationService.ScheduleAllRemindersAsync(assignments, subjects);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Notification scheduling error: {ex.Message}");
            }
        }

        private void LoadUserProfile()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser != null)
            {
                UserEmail = currentUser.Email;
                var uid = currentUser.FirebaseUid;
                if (!string.IsNullOrEmpty(uid))
                {
                    var savedName = Preferences.Get($"fullname_{uid}", "");
                    UserFullName = !string.IsNullOrEmpty(savedName) ? savedName : currentUser.FullName;
                }
                else
                {
                    UserFullName = currentUser.FullName;
                }
            }
        }

        private async Task EditFullNameAsync()
        {
            if (IsLoading || _authService.CurrentUser == null) return;

            var currentUser = _authService.CurrentUser;
            var currentName = UserFullName;

            var result = await Application.Current!.Windows[0].Page!.DisplayPromptAsync(
                "Edit Full Name", "Enter your full name:", "Save", "Cancel",
                currentName, -1, Keyboard.Default);

            if (!string.IsNullOrEmpty(result) && result != currentName)
            {
                IsLoading = true;
                try
                {
                    var uid = currentUser.FirebaseUid;
                    Preferences.Set($"fullname_{uid}", result);
                    UserFullName = result;
                    currentUser.FullName = result;
                    var path = Path.Combine(FileSystem.AppDataDirectory, "current_user.json");
                    File.WriteAllText(path, JsonSerializer.Serialize(currentUser));
                }
                catch (Exception ex)
                {
                    await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Error", $"Failed to update name: {ex.Message}", "OK");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        public async Task LogoutAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            try
            {
                await _authService.LogoutAsync();
                await Shell.Current.GoToAsync("//login");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}