using Plugin.Firebase.Auth;
using StudySync.Shared.Models;
using StudySync.Shared.Services;
using System.Text.Json;

namespace StudySync.Services
{
    public class AuthService : IAuthService
    {
        private User? _currentUser;
        public User? CurrentUser => _currentUser;
        private string? _lastError;
        public string? LastError => _lastError;

        public AuthService()
        {
            LoadCurrentUser();
        }

        public async Task<bool> RegisterAsync(string fullName, string email, string password)
        {
            try
            {
                var firebaseUser = await CrossFirebaseAuth.Current
                    .CreateUserAsync(email, password);

                if (firebaseUser == null) return false;

                Preferences.Set("pending_fullname", fullName);
                Preferences.Set("pending_email", email);

                await firebaseUser.SendEmailVerificationAsync();

                _lastError = "verification_pending";
                return false;
            }
            catch (Exception ex)
            {
                _lastError = $"{ex.GetType().Name}: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var firebaseUser = await CrossFirebaseAuth.Current
                    .SignInWithEmailAndPasswordAsync(email, password);

                if (firebaseUser == null) return false;

                await CrossFirebaseAuth.Current.ReloadCurrentUserAsync();

                var refreshedUser = CrossFirebaseAuth.Current.CurrentUser;

                if (refreshedUser == null || !refreshedUser.IsEmailVerified)
                {
                    _lastError = "email_not_verified";
                    await CrossFirebaseAuth.Current.SignOutAsync();
                    return false;
                }

                var uid = refreshedUser.Uid;

                var savedName = Preferences.ContainsKey($"fullname_{uid}")
                    ? Preferences.Get($"fullname_{uid}", "")
                    : Preferences.ContainsKey("pending_fullname")
                        ? Preferences.Get("pending_fullname", "")
                        : null;

                if (!string.IsNullOrEmpty(savedName))
                    Preferences.Set($"fullname_{uid}", savedName);

                var fullName = !string.IsNullOrEmpty(savedName)
                    ? savedName
                    : !string.IsNullOrEmpty(refreshedUser.DisplayName)
                        ? refreshedUser.DisplayName
                        : refreshedUser.Email ?? email;

                _currentUser = new User
                {
                    Id = 1,
                    FullName = fullName,
                    Email = refreshedUser.Email ?? email,
                    FirebaseUid = refreshedUser.Uid
                };

                SaveCurrentUser(_currentUser);
                return true;
            }
            catch (Exception ex)
            {
                _lastError = $"{ex.GetType().Name}: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> ResendVerificationEmailAsync()
        {
            try
            {
                var firebaseUser = CrossFirebaseAuth.Current.CurrentUser;
                if (firebaseUser == null) return false;

                await firebaseUser.SendEmailVerificationAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastError = $"{ex.GetType().Name}: {ex.Message}";
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await CrossFirebaseAuth.Current.SignOutAsync();
                _currentUser = null;
                var path = Path.Combine(FileSystem.AppDataDirectory, "current_user.json");
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LogoutAsync error: {ex.Message}");
            }
        }

        public bool IsUserLoggedIn()
        {
            return _currentUser != null || CrossFirebaseAuth.Current.CurrentUser != null;
        }

        private void LoadCurrentUser()
        {
            try
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, "current_user.json");
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    _currentUser = JsonSerializer.Deserialize<User>(json);
                }
            }
            catch
            {
                _currentUser = null;
            }
        }

        private void SaveCurrentUser(User user)
        {
            try
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, "current_user.json");
                File.WriteAllText(path, JsonSerializer.Serialize(user));
            }
            catch { }
        }
    }
}