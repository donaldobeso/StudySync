using StudySync.ViewModels;
using StudySync.Shared.Services;

namespace StudySync;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage()
    {
        InitializeComponent();
        var services = Application.Current?.Handler?.MauiContext?.Services;
        _viewModel = services?.GetService<LoginViewModel>()
            ?? new LoginViewModel(services?.GetService<IAuthService>()
                ?? throw new InvalidOperationException("IAuthService not registered."));
        BindingContext = _viewModel;
    }

    public LoginPage(LoginViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        var success = await _viewModel.LoginAsync();

        if (success)
        {
            await Shell.Current.GoToAsync("//mainTabs/dashboard");
        }
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//register");
    }
}