using StudySync.Shared.Services;
using StudySync.Shared.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudySync.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;

        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
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

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<bool> LoginAsync()
        {
            ErrorMessage = string.Empty;

            if (!Validator.IsNotEmpty(Email))
            {
                ErrorMessage = "⚠️ Please enter your email.";
                return false;
            }

            if (!Validator.IsValidEmail(Email))
            {
                ErrorMessage = "⚠️ Please enter a valid email.";
                return false;
            }

            if (!Validator.IsNotEmpty(Password))
            {
                ErrorMessage = "⚠️ Please enter your password.";
                return false;
            }

            IsLoading = true;
            var success = await _authService.LoginAsync(Email, Password);
            IsLoading = false;

            if (!success)
            {
                if (_authService.LastError == "email_not_verified")
                    ErrorMessage = "⚠️ Please verify your email before logging in.";
                else
                    ErrorMessage = _authService.LastError ?? "⚠️ Login failed.";

                return false;
            }

            return true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
