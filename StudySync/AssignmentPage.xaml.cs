using StudySync.Shared.Models;
using StudySync.Shared.Services;
using StudySync.ViewModels;
using StudySync.Services;

namespace StudySync;

public partial class AssignmentPage : ContentPage
{
    private readonly IAssignmentService _assignmentService;
    private readonly AssignmentViewModel _viewModel;
    private List<Assignment> _allAssignments = [];

    public AssignmentPage()
    {
        InitializeComponent();
        var services = Application.Current?.Handler?.MauiContext?.Services;
        _viewModel = services?.GetService<AssignmentViewModel>()
            ?? new AssignmentViewModel(
                services?.GetService<IAssignmentService>()!,
                services?.GetService<IAuthService>()!);
        _assignmentService = services?.GetService<IAssignmentService>()
            ?? new AssignmentService(services?.GetService<LocalDatabaseService>()!);
        BindingContext = _viewModel;
    }

    public AssignmentPage(AssignmentViewModel viewModel, IAssignmentService assignmentService) : this()
    {
        _viewModel = viewModel;
        _assignmentService = assignmentService;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshListAsync();
    }

    private async Task RefreshListAsync()
    {
        string userUid = _viewModel.AuthService.CurrentUser?.Email ?? "";
        _allAssignments = await _assignmentService.GetAssignmentsAsync(userUid);
        ApplyFilterAndSort();
    }

    private void ApplyFilterAndSort()
    {
        var query = SearchEntry.Text ?? "";
        var filtered = string.IsNullOrEmpty(query)
            ? _allAssignments
            : _allAssignments.Where(a =>
                a.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                a.SubjectName.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        AssignmentList.ItemsSource = SortPicker.SelectedIndex switch
        {
            1 => filtered.OrderBy(a => a.Priority switch { "High" => 0, "Medium" => 1, _ => 2 }).ToList(),
            2 => filtered.OrderBy(a => a.SubjectName).ToList(),
            3 => filtered.OrderBy(a => a.Title).ToList(),
            _ => filtered.OrderBy(a => a.DueDate).ToList()
        };
    }

    private void OnSearchChanged(object? sender, TextChangedEventArgs e) => ApplyFilterAndSort();
    private void OnSortChanged(object? sender, EventArgs e) => ApplyFilterAndSort();

    private async void OnAddAssignmentClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("addassignment");
    }

    private async void OnMarkCompleteClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is Assignment assignment)
        {
            if (!assignment.IsCompleted)
            {
                assignment.IsCompleted = true;
                string userUid = _viewModel.AuthService.CurrentUser?.Email ?? "";
                await _assignmentService.MarkCompleteAsync(assignment.FirestoreId, userUid);
                await RefreshListAsync();
            }
        }
    }

    private async void OnEditAssignmentClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is Assignment assignment)
        {
            var title = await DisplayPromptAsync("Edit Assignment", "Title:", initialValue: assignment.Title);
            if (string.IsNullOrEmpty(title)) return;

            var subject = await DisplayPromptAsync("Edit Assignment", "Subject:", initialValue: assignment.SubjectName);
            var description = await DisplayPromptAsync("Edit Assignment", "Description:", initialValue: assignment.Description);

            var priorityOptions = new[] { "Low", "Medium", "High" };
            var priority = await DisplayActionSheetAsync("Priority", "Cancel", null, priorityOptions);
            if (priority == "Cancel" || string.IsNullOrEmpty(priority)) priority = assignment.Priority;

            var dueDateStr = await DisplayPromptAsync("Edit Assignment", "Due Date (YYYY-MM-DD):",
                initialValue: assignment.DueDate.ToString("yyyy-MM-dd"),
                keyboard: Keyboard.Default);

            var estStr = await DisplayPromptAsync("Edit Assignment", "Estimated minutes:",
                initialValue: assignment.EstimatedMinutes.ToString(),
                keyboard: Keyboard.Numeric);

            assignment.Title = title;
            assignment.SubjectName = subject ?? assignment.SubjectName;
            assignment.Description = description ?? assignment.Description;
            assignment.Priority = priority;

            if (!string.IsNullOrEmpty(dueDateStr) && DateTime.TryParse(dueDateStr, out var newDate))
                assignment.DueDate = newDate;

            if (!string.IsNullOrEmpty(estStr) && int.TryParse(estStr, out var mins))
                assignment.EstimatedMinutes = mins;

            string userUid = _viewModel.AuthService.CurrentUser?.Email ?? "";
            await _assignmentService.UpdateAssignmentAsync(assignment, userUid);
            await RefreshListAsync();
        }
    }

    private async void OnDeleteAssignmentClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is Assignment assignment)
        {
            bool confirm = await DisplayAlertAsync("Delete Assignment",
                $"Are you sure you want to delete '{assignment.Title}'?", "Yes", "No");

            if (confirm)
            {
                string userUid = _viewModel.AuthService.CurrentUser?.Email ?? "";
                await _assignmentService.DeleteAssignmentAsync(assignment.FirestoreId, userUid);
                await RefreshListAsync();
            }
        }
    }
}