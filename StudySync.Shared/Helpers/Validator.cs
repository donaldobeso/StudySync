// Helpers/Validator.cs
namespace StudySync.Shared.Helpers
{
    public static class Validator
    {
        public static bool IsValidEmail(string email) =>
            !string.IsNullOrWhiteSpace(email) && email.Contains("@") && email.Contains(".");

        public static bool IsNotEmpty(string value) =>
            !string.IsNullOrWhiteSpace(value);
    }
}