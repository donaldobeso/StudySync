using Microsoft.Extensions.DependencyInjection;

namespace StudySync
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Apply saved theme on startup
            var savedTheme = Preferences.Get("app_theme", "light");
            UserAppTheme = savedTheme.ToLower() == "dark" ? AppTheme.Dark : AppTheme.Light;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}