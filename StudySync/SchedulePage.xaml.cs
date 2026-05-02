using StudySync.ViewModels;
using StudySync.Shared.Services;

namespace StudySync;

public partial class SchedulePage : ContentPage
{
    private readonly ScheduleViewModel _viewModel;

    public SchedulePage()
    {
        InitializeComponent();
        var services = Application.Current?.Handler?.MauiContext?.Services;
        _viewModel = services?.GetService<ScheduleViewModel>()
            ?? new ScheduleViewModel(
                services?.GetService<IAssignmentService>()!,
                services?.GetService<ISubjectService>()!,
                services?.GetService<IAuthService>()!);
        BindingContext = _viewModel;
    }

    public SchedulePage(ScheduleViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private async void OnDailyClicked(object? sender, EventArgs e)
    {
        await _viewModel.GenerateDailyScheduleAsync();
    }

    private async void OnWeeklyClicked(object? sender, EventArgs e)
    {
        await _viewModel.GenerateWeeklyScheduleAsync();
    }
}