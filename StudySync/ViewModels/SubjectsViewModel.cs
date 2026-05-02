using StudySync.Shared.Models;
using StudySync.Shared.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudySync.ViewModels
{
    public partial class SubjectsViewModel : INotifyPropertyChanged
    {
        private readonly ISubjectService _subjectService;
        private readonly IAuthService _authService;
        private List<Subject> _allSubjects = [];
        private List<Subject> _subjects = [];
        private string _searchQuery = string.Empty;
        private string _sortOption = "Name";

        public List<Subject> Subjects
        {
            get => _subjects;
            set { if (_subjects != value) { _subjects = value; OnPropertyChanged(); } }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged();
                    ApplyFilterAndSort();
                }
            }
        }

        public string SortOption
        {
            get => _sortOption;
            set
            {
                if (_sortOption != value)
                {
                    _sortOption = value;
                    OnPropertyChanged();
                    ApplyFilterAndSort();
                }
            }
        }

        public List<string> SortOptions => ["Name", "Instructor", "Room"];

        public SubjectsViewModel(ISubjectService subjectService, IAuthService authService)
        {
            _subjectService = subjectService;
            _authService = authService;
        }

        public async Task LoadSubjectsAsync()
        {
            if (_authService.CurrentUser != null)
            {
                _allSubjects = await _subjectService.GetSubjectsAsync(_authService.CurrentUser.Email);
                ApplyFilterAndSort();
            }
        }

        private void ApplyFilterAndSort()
        {
            var filtered = string.IsNullOrEmpty(SearchQuery)
                ? _allSubjects
                : _allSubjects.Where(s =>
                    s.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    s.Instructor.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)).ToList();

            Subjects = SortOption switch
            {
                "Instructor" => filtered.OrderBy(s => s.Instructor).ToList(),
                "Room" => filtered.OrderBy(s => s.Room).ToList(),
                _ => filtered.OrderBy(s => s.Name).ToList()
            };
        }

        public async Task DeleteSubjectAsync(Subject subject)
        {
            if (_authService.CurrentUser != null)
            {
                await _subjectService.DeleteSubjectAsync(subject.FirestoreId, _authService.CurrentUser.Email);
                await LoadSubjectsAsync();
            }
        }

        public async Task UpdateSubjectAsync(Subject subject)
        {
            if (_authService.CurrentUser != null)
            {
                await _subjectService.UpdateSubjectAsync(subject, _authService.CurrentUser.Email);
                await LoadSubjectsAsync();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}