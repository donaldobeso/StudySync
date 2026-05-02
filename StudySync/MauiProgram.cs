using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using SQLitePCL;
using StudySync.Services;
using StudySync.Shared.Services;
using StudySync.ViewModels;

namespace StudySync
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseLocalNotification(config =>
                {
                    config.AddAndroid(android =>
                    {
                        android.AddChannel(new NotificationChannelRequest
                        {
                            Id = "studysync_reminders",
                            Name = "Assignment Reminders",
                            Description = "Reminders for upcoming assignment due dates"
                        });
                    });
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register Services
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IAssignmentService, AssignmentService>();
            builder.Services.AddSingleton<ISubjectService, SubjectService>();
            builder.Services.AddSingleton<LocalDatabaseService>();
            builder.Services.AddSingleton<NotificationService>();

            // Register ViewModels
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<RegisterViewModel>();
            builder.Services.AddSingleton<DashboardViewModel>();
            builder.Services.AddSingleton<AssignmentViewModel>();
            builder.Services.AddSingleton<SubjectsViewModel>();
            builder.Services.AddSingleton<AddSubjectViewModel>();
            builder.Services.AddSingleton<SettingsViewModel>();

            // Register Pages
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<RegisterPage>();
            builder.Services.AddSingleton<DashboardPage>();
            builder.Services.AddSingleton<AssignmentPage>();
            builder.Services.AddSingleton<SubjectsPage>();
            builder.Services.AddSingleton<AddSubjectPage>();
            builder.Services.AddSingleton<SettingsPage>();
            builder.Services.AddSingleton<ScheduleViewModel>();
            builder.Services.AddSingleton<SchedulePage>();
            builder.Services.AddSingleton<AddAssignmentViewModel>();
            builder.Services.AddSingleton<AddAssignmentPage>();

            builder.Services.AddSingleton<AppShell>();

            SQLitePCL.Batteries_V2.Init();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}