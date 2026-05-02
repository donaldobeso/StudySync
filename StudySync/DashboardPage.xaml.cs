using StudySync.Shared.Services;
using StudySync.ViewModels;

namespace StudySync;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage()
    {
        InitializeComponent();
        var services = Application.Current?.Handler?.MauiContext?.Services;
        _viewModel = services?.GetService<DashboardViewModel>() ?? new DashboardViewModel(
            services?.GetService<IAssignmentService>()!,
            services?.GetService<ISubjectService>()!,
            services?.GetService<IAuthService>()!);
        BindingContext = _viewModel;
    }

    public DashboardPage(DashboardViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _viewModel.LoadDashboardDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DashboardPage.OnAppearing - LoadDashboardDataAsync error: {ex}");
            await DisplayAlertAsync("Error", $"Failed to load dashboard: {ex.Message}", "OK");
        }
    }
}
