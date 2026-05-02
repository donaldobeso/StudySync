using StudySync.Shared.Models;
using StudySync.Shared.Services;
using StudySync.Shared.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudySync.ViewModels
{
    public partial class AddSubjectViewModel : INotifyPropertyChanged
    {
        private readonly ISubjectService _subjectService;
        private readonly IAuthService _authService;
        private string _name = string.Empty;
        private string _instructor = string.Empty;
        private string _room = string.Empty;
        private string _schedule = string.Empty;

        // Days selection (multi-select)
        private List<object> _selectedDays = new();

        // Class times
        private TimeSpan _startTime = new TimeSpan(8, 0, 0);
        private TimeSpan _endTime = new TimeSpan(9, 0, 0);

        private string _selectedColor = "#4A90D9";
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;

        public List<string> AvailableDays => new() { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

        public List<object> SelectedDays
        {
            get => _selectedDays;
            set
            {
                if (_selectedDays != value)
                {
                    _selectedDays = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan StartTime
        {
            get => _startTime;
            set
            {
                if (_startTime != value)
                {
                    _startTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan EndTime
        {
            get => _endTime;
            set
            {
                if (_endTime != value)
                {
                    _endTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Instructor
        {
            get => _instructor;
            set
            {
                if (_instructor != value)
                {
                    _instructor = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Room
        {
            get => _room;
            set
            {
                if (_room != value)
                {
                    _room = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Schedule
        {
            get => _schedule;
            set
            {
                if (_schedule != value)
                {
                    _schedule = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedColor
        {
            get => _selectedColor;
            set
            {
                if (_selectedColor != value)
                {
                    _selectedColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
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
                }
            }
        }

        public List<string> Colors => new()
        {
            "#4A90D9",  // Blue
            "#FF6B6B",  // Red
            "#51CF66",  // Green
            "#FFD93D",  // Yellow
            "#9D84B7",  // Purple
            "#FF9FF3",  // Pink
            "#FF7675",  // Orange
            "#00B4D8"   // Cyan
        };

        public AddSubjectViewModel(ISubjectService subjectService, IAuthService authService)
        {
            _subjectService = subjectService;
            _authService = authService;
        }

        public async Task SaveSubjectAsync()
        {
            ErrorMessage = string.Empty;

            if (!Validator.IsNotEmpty(Name))
            {
                ErrorMessage = "⚠️ Please enter the subject name.";
                return;
            }

            if (!SelectedDays.Any())
            {
                ErrorMessage = "⚠️ Please select at least one class day.";
                return;
            }

            if (StartTime >= EndTime)
            {
                ErrorMessage = "⚠️ Start time must be before end time.";
                return;
            }

            try
            {
                IsLoading = true;
                await Task.Delay(300);

                if (_authService.CurrentUser != null)
                {
                    // Convert selected days to list of strings
                    var classDays = SelectedDays.Select(d => d.ToString()!).ToList();

                    // Build legacy schedule string for backward compatibility
                    var legacySchedule = string.Join(", ", classDays) + $" {StartTime:HH\\:mm}-{EndTime:HH\\:mm}";

                    var subject = new Subject
                    {
                        Name = Name,
                        Instructor = Instructor,
                        Room = Room,
                        Schedule = legacySchedule,
                        ClassDays = string.Join(";", classDays),
                        ClassStartTime = StartTime.ToString(@"HH\:mm"),  // ✅ 24-hour
                        ClassEndTime = EndTime.ToString(@"HH\:mm"),      // ✅ 24-hour
                        Color = SelectedColor
                    };

                    await _subjectService.AddSubjectAsync(subject, _authService.CurrentUser.Email);

                    // Reset fields
                    Name = string.Empty;
                    Instructor = string.Empty;
                    Room = string.Empty;
                    Schedule = string.Empty;
                    SelectedDays = new List<object>();
                    StartTime = new TimeSpan(8, 0, 0);
                    EndTime = new TimeSpan(9, 0, 0);
                    SelectedColor = "#4A90D9";
                }
            }
            catch (Exception ex)
            {
                await App.Current!.Windows[0].Page!.DisplayAlertAsync("Firestore Error", ex.Message, "OK");
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
