using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using StudySync.Shared.Models;

namespace StudySync.Services
{
    public class NotificationService
    {
        public async Task RequestPermissionAsync()
        {
            if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
                await LocalNotificationCenter.Current.RequestNotificationPermission();
        }

        public async Task ScheduleAllRemindersAsync(List<Assignment> assignments, List<Subject> subjects)
        {
            LocalNotificationCenter.Current.CancelAll();

            var notificationId = 1000;
            var now = DateTime.Now;

            // Assignment reminders
            foreach (var assignment in assignments.Where(a => !a.IsCompleted))
            {
                var oneDayBefore = assignment.DueDate.Date.AddDays(-1).AddHours(8);
                var onDueDate = assignment.DueDate.Date.AddHours(8);

                if (oneDayBefore > now)
                {
                    await LocalNotificationCenter.Current.Show(new NotificationRequest
                    {
                        NotificationId = notificationId++,
                        Title = "📚 Assignment Due Tomorrow!",
                        Description = $"'{assignment.Title}' for {assignment.SubjectName} is due tomorrow.",
                        Schedule = new NotificationRequestSchedule { NotifyTime = oneDayBefore },
                        Android = new AndroidOptions
                        {
                            ChannelId = "studysync_reminders",
                            IconSmallName = new AndroidIcon("notification_icon")
                        }
                    });
                }

                if (onDueDate > now)
                {
                    await LocalNotificationCenter.Current.Show(new NotificationRequest
                    {
                        NotificationId = notificationId++,
                        Title = "⚠️ Assignment Due Today!",
                        Description = $"'{assignment.Title}' for {assignment.SubjectName} is due today!",
                        Schedule = new NotificationRequestSchedule { NotifyTime = onDueDate },
                        Android = new AndroidOptions
                        {
                            ChannelId = "studysync_reminders",
                            IconSmallName = new AndroidIcon("notification_icon")
                        }
                    });
                }
            }

            // Subject class reminders — daily at 7AM
            foreach (var subject in subjects)
            {
                if (string.IsNullOrEmpty(subject.Schedule)) continue;

                var daysOfWeek = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
                foreach (var day in daysOfWeek)
                {
                    if (!subject.Schedule.Contains(day, StringComparison.OrdinalIgnoreCase)) continue;

                    // Find next occurrence of this day
                    var today = DateTime.Today;
                    var targetDow = day switch
                    {
                        "Mon" => DayOfWeek.Monday,
                        "Tue" => DayOfWeek.Tuesday,
                        "Wed" => DayOfWeek.Wednesday,
                        "Thu" => DayOfWeek.Thursday,
                        "Fri" => DayOfWeek.Friday,
                        "Sat" => DayOfWeek.Saturday,
                        _ => DayOfWeek.Sunday
                    };

                    var daysUntil = ((int)targetDow - (int)today.DayOfWeek + 7) % 7;
                    if (daysUntil == 0) daysUntil = 7; // Next week if today
                    var notifyTime = today.AddDays(daysUntil).AddHours(7);

                    if (notifyTime > now)
                    {
                        await LocalNotificationCenter.Current.Show(new NotificationRequest
                        {
                            NotificationId = notificationId++,
                            Title = $"🏫 Class Today: {subject.Name}",
                            Description = $"📍 {subject.Room} • {subject.Instructor} • {subject.Schedule}",
                            Schedule = new NotificationRequestSchedule { NotifyTime = notifyTime },
                            Android = new AndroidOptions
                            {
                                ChannelId = "studysync_reminders",
                                IconSmallName = new AndroidIcon("notification_icon")
                            }
                        });
                    }
                }
            }
        }
    }
}