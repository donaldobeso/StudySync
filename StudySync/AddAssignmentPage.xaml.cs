using StudySync.ViewModels;
using StudySync.Shared.Services;

namespace StudySync;

public partial class AddAssignmentPage : ContentPage
{
    private readonly AddAssignmentViewModel _viewModel;

    public AddAssignmentPage()
    {
        InitializeComponent();
        var services = Application.Current?.Handler?.MauiContext?.Services;
        _viewModel = services?.GetService<AddAssignmentViewModel>()
            ?? new AddAssignmentViewModel(
                services?.GetService<IAssignmentService>()!,
                services?.GetService<IAuthService>()!);
        BindingContext = _viewModel;
    }

    public AddAssignmentPage(AddAssignmentViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        await _viewModel.SaveAssignmentAsync();

        if (string.IsNullOrEmpty(_viewModel.ErrorMessage))
            await Shell.Current.GoToAsync("//mainTabs/assignment");
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//mainTabs/assignment");
    }
}