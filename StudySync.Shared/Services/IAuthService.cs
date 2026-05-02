using StudySync.Shared.Models;

namespace StudySync.Shared.Services
{
    public interface IAuthService
    {
        User? CurrentUser { get; }
        Task<bool> RegisterAsync(string fullName, string email, string password);
        Task<bool> LoginAsync(string email, string password);
        Task LogoutAsync();
        bool IsUserLoggedIn();
        string? LastError { get; }
        Task<bool> ResendVerificationEmailAsync();
    }
}
