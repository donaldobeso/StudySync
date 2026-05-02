using StudySync.Shared.Services;
using StudySync.Shared.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudySync.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private string _fullName = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;
        private bool _verificationPending = false;
        public bool VerificationPending
        {
            get => _verificationPending;
            set
            {
                if (_verificationPending != value)
                {
                    _verificationPending = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FullName
        {
            get => _fullName;
            set
            {
                if (_fullName != value)
                {
                    _fullName = value;
                    OnPropertyChanged();
                }
            }
        }

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

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (_confirmPassword != value)
                {
                    _confirmPassword = value;
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

        public RegisterViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task RegisterAsync()
        {
            ErrorMessage = string.Empty;

            if (!Validator.IsNotEmpty(FullName))
            {
                ErrorMessage = "⚠️ Please enter your full name.";
                return;
            }

            if (!Validator.IsNotEmpty(Email))
            {
                ErrorMessage = "⚠️ Please enter your email.";
                return;
            }

            if (!Validator.IsValidEmail(Email))
            {
                ErrorMessage = "⚠️ Please enter a valid email.";
                return;
            }

            if (!Validator.IsNotEmpty(Password))
            {
                ErrorMessage = "⚠️ Please enter a password.";
                return;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "⚠️ Password must be at least 6 characters.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "⚠️ Passwords do not match.";
                return;
            }

            IsLoading = true;
            var success = await _authService.RegisterAsync(FullName, Email, Password);
            IsLoading = false;

            if (!success)
            {
                if (_authService.LastError == "verification_pending")
                {
                    VerificationPending = true;
                    ErrorMessage = "📧 Verification email sent! Please check your inbox before logging in.";
                }
                else
                {
                    VerificationPending = false;
                    ErrorMessage = _authService.LastError ?? "⚠️ Registration failed.";
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
