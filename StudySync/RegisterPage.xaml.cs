using StudySync.ViewModels;
using StudySync.Shared.Services;
using Plugin.Firebase.Auth;

namespace StudySync;

public partial class RegisterPage : ContentPage
{
    private readonly RegisterViewModel _viewModel;

    public RegisterPage()
    {
        InitializeComponent();
        var services = Application.Current?.Handler?.MauiContext?.Services;
        _viewModel = services?.GetService<RegisterViewModel>() ?? new RegisterViewModel(services?.GetService<IAuthService>()!);
        BindingContext = _viewModel;
    }

    public RegisterPage(RegisterViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        await _viewModel.RegisterAsync();

        if (_viewModel.VerificationPending)
        {
            await DisplayAlertAsync(
                "Verify Your Email",
                "A verification email has been sent to your inbox. Please verify before logging in.",
                "OK");

            // Sign out Firebase session so no auto-login happens
            await CrossFirebaseAuth.Current.SignOutAsync();

            await Shell.Current.GoToAsync("///login");
        }
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///login");
    }
}