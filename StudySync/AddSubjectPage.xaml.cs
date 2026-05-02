using StudySync.ViewModels;
using StudySync.Shared.Services;

namespace StudySync;

public partial class AddSubjectPage : ContentPage
{
    private readonly AddSubjectViewModel _viewModel;

    public AddSubjectPage()
    {
        InitializeComponent();
        var services = Application.Current?.Handler?.MauiContext?.Services;
        _viewModel = services?.GetService<AddSubjectViewModel>() ?? new AddSubjectViewModel(
            services?.GetService<ISubjectService>()!,
            services?.GetService<IAuthService>()!);
        BindingContext = _viewModel;
    }

    public AddSubjectPage(AddSubjectViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        await _viewModel.SaveSubjectAsync();

        if (string.IsNullOrEmpty(_viewModel.ErrorMessage))
        {
            await Shell.Current.GoToAsync("///subjects");
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///subjects");
    }
}
