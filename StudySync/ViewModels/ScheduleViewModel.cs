using StudySync.Shared.Models;
using StudySync.Shared.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudySync.ViewModels
{
    public class ScheduleItem
    {
        public string TimeSlot { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ItemType { get; set; } = "task";
        public bool IsOptional { get; set; } = false;

        public string BackgroundColor => IsOptional ? "#FF9FF3" : ItemType switch
        {
            "meal" => "#FF9FF3",
            "class" => "#4A90D9",
            "study" => "#9D84B7",
            "assignment" => "#FF6B6B",
            "break" => "#51CF66",
            "freetime" => "#00B4D8",
            "overdue" => "#CC0000",
            _ => "#4A90D9"
        };

        public string Icon => IsOptional ? "💪" : ItemType switch
        {
            "meal" => "🍽️",
            "class" => "🏫",
            "study" => "📖",
            "assignment" => "📝",
            "break" => "☕",
            "freetime" => "🎮",
            "overdue" => "🚨",
            _ => "📌"
        };
    }

    public class DaySchedule
    {
        public string DayLabel { get; set; } = string.Empty;
        public List<ScheduleItem> Items { get; set; } = [];
        public bool HasItems => Items.Count > 0;
    }

    public class ScheduleViewModel : INotifyPropertyChanged
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ISubjectService _subjectService;
        private readonly IAuthService _authService;

        private List<ScheduleItem> _dailySchedule = [];
        private List<DaySchedule> _weeklySchedule = [];
        private bool _isLoading = false;
        private bool _showDaily = false;
        private bool _showWeekly = false;
        private string _statusMessage = string.Empty;

        public List<ScheduleItem> DailySchedule
        {
            get => _dailySchedule;
            set { _dailySchedule = value; OnPropertyChanged(); }
        }

        public List<DaySchedule> WeeklySchedule
        {
            get => _weeklySchedule;
            set { _weeklySchedule = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotLoading)); }
        }

        public bool IsNotLoading => !IsLoading;

        public bool ShowDaily
        {
            get => _showDaily;
            set { _showDaily = value; OnPropertyChanged(); }
        }

        public bool ShowWeekly
        {
            get => _showWeekly;
            set { _showWeekly = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasStatus)); }
        }

        public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);

        public ScheduleViewModel(IAssignmentService assignmentService, ISubjectService subjectService, IAuthService authService)
        {
            _assignmentService = assignmentService;
            _subjectService = subjectService;
            _authService = authService;
        }

        // Returns assignments scheduled for a specific day using consistent logic
        // required = urgent + priority-based non-urgent (1 max)
        // optional = 1 additional non-urgent if available
        private static (List<Assignment> required, Assignment? optional) GetAssignmentsForDay(
            DateTime date,
            List<Assignment> allPending,
            HashSet<string> alreadyScheduled)
        {
            var today = DateTime.Today;
            var dayOfWeek = (int)(date - today).TotalDays;

            var required = new List<Assignment>();
            Assignment? optional = null;

            // Urgent for THIS day: due today → schedule today, due tomorrow → schedule today too
            var urgentToday = allPending
                .Where(a => !alreadyScheduled.Contains(a.FirestoreId)
                    && a.DueDate.Date == date.Date)
                .OrderBy(a => a.DueDate)
                .ToList();

            var urgentTomorrow = allPending
                .Where(a => !alreadyScheduled.Contains(a.FirestoreId)
                    && a.DueDate.Date == date.AddDays(1).Date)
                .OrderBy(a => PriorityWeight(a.Priority))
                .ToList();

            // All urgent go in required
            required.AddRange(urgentToday);
            required.AddRange(urgentTomorrow);

            // Non-urgent — always pick 1 required + 1 optional (if available)
            // High priority: due in 2-5 days from this date
            var highPriority = allPending
                .Where(a => !alreadyScheduled.Contains(a.FirestoreId)
                    && !required.Any(r => r.FirestoreId == a.FirestoreId)
                    && a.Priority == "High"
                    && (a.DueDate.Date - date).TotalDays is >= 2 and <= 5)
                .OrderBy(a => a.DueDate)
                .FirstOrDefault();

            // Medium priority: due in 2-7 days from this date
            var mediumPriority = allPending
                .Where(a => !alreadyScheduled.Contains(a.FirestoreId)
                    && !required.Any(r => r.FirestoreId == a.FirestoreId)
                    && a.Priority == "Medium"
                    && (a.DueDate.Date - date).TotalDays is >= 2 and <= 7)
                .OrderBy(a => a.DueDate)
                .FirstOrDefault();

            var nonUrgentPick = highPriority ?? mediumPriority;

            if (nonUrgentPick != null)
            {
                required.Add(nonUrgentPick);

                // Optional: one more non-urgent
                optional = allPending
                    .Where(a => !alreadyScheduled.Contains(a.FirestoreId)
                        && !required.Any(r => r.FirestoreId == a.FirestoreId)
                        && a.Priority != "Low"
                        && (a.DueDate.Date - date).TotalDays >= 2)
                    .OrderBy(a => PriorityWeight(a.Priority))
                    .ThenBy(a => a.DueDate)
                    .FirstOrDefault();
            }
            else if (required.Count == 0)
            {
                // No urgent, no priority match — still show 1 if anything available
                var fallback = allPending
                    .Where(a => !alreadyScheduled.Contains(a.FirestoreId))
                    .OrderBy(a => PriorityWeight(a.Priority))
                    .ThenBy(a => a.DueDate)
                    .FirstOrDefault();

                if (fallback != null)
                    required.Add(fallback);
            }

            return (required, optional);
        }

        public async Task GenerateDailyScheduleAsync()
        {
            if (_authService.CurrentUser == null) return;

            IsLoading = true;
            ShowDaily = false;
            StatusMessage = "";

            try
            {
                var assignments = await _assignmentService.GetAssignmentsAsync(_authService.CurrentUser.Email);
                var subjects = await _subjectService.GetSubjectsAsync(_authService.CurrentUser.Email);

                var today = DateTime.Today;
                var dayName = today.DayOfWeek.ToString();
                var isWeekend = today.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

                var pending = assignments.Where(a => !a.IsCompleted && a.DueDate >= today).ToList();
                var overdue = assignments.Where(a => !a.IsCompleted && a.DueDate.Date < today).ToList();

                var (required, optional) = GetAssignmentsForDay(today, pending, []);

                // Timeline system
                var schedule = new List<(int start, int end, ScheduleItem item)>();
                var blocked = new List<(int start, int end)>();

                int DAY_START = 7 * 60;
                int DAY_END = required.Any(a => a.DueDate.Date == today) ? 23 * 60 + 59 : 21 * 60;

                int current = DAY_START;

                // ===== HELPER =====
                int GetSafeStart(int start, int duration)
                {
                    while (true)
                    {
                        var overlap = blocked.FirstOrDefault(b =>
                            start < b.end && (start + duration) > b.start);

                        if (overlap == default)
                            return start;

                        start = overlap.end;
                    }
                }

                void AddBlock(int start, int duration, ScheduleItem item)
                {
                    int safeStart = GetSafeStart(start, duration);
                    int end = safeStart + duration;

                    item.TimeSlot = $"{safeStart / 60:D2}:{safeStart % 60:D2} - {end / 60:D2}:{end % 60:D2}";

                    schedule.Add((safeStart, end, item));
                    blocked.Add((safeStart, end));
                }

                // ===== FIXED EVENTS =====
                AddBlock(7 * 60, 30, new ScheduleItem { Title = "Breakfast", Description = "Start your day right 🌅", ItemType = "meal" });
                AddBlock(12 * 60, 60, new ScheduleItem { Title = "Lunch", Description = "Midday break 🥗", ItemType = "meal" });
                AddBlock(18 * 60 + 30, 30, new ScheduleItem { Title = "Dinner", Description = "Evening meal 🍽️", ItemType = "meal" });

                // ===== CLASSES =====
                var hasClassToday = new HashSet<string>();

                foreach (var subject in subjects)
                {
                    bool hasClass =
                        (!string.IsNullOrEmpty(subject.ClassDays) &&
                         subject.ClassDays.Split(';').Contains(dayName[..3])) ||
                        (!string.IsNullOrEmpty(subject.Schedule) &&
                         subject.Schedule.Contains(dayName[..3]));

                    if (!hasClass) continue;

                    hasClassToday.Add(subject.Name);

                    int start = 9 * 60;
                    int end = 10 * 60;

                    if (TimeSpan.TryParse(subject.ClassStartTime, out var s))
                        start = (int)s.TotalMinutes;

                    if (TimeSpan.TryParse(subject.ClassEndTime, out var e))
                        end = (int)e.TotalMinutes;

                    AddBlock(start, end - start, new ScheduleItem
                    {
                        Title = $"Class: {subject.Name}",
                        Description = $"📍 {subject.Room} • {subject.Instructor}",
                        ItemType = "class"
                    });
                }

                // ===== OVERDUE =====
                int overdueStart = 7 * 60 + 30;

                foreach (var od in overdue.Take(2))
                {
                    int duration = Math.Min(od.EstimatedMinutes, 60);

                    AddBlock(overdueStart, duration, new ScheduleItem
                    {
                        Title = $"🚨 OVERDUE: {od.Title}",
                        Description = $"{od.SubjectName} — was due {od.DueDate:MMM dd}. Do this ASAP!",
                        ItemType = "overdue"
                    });

                    overdueStart += duration + 10;
                }

                // ===== ASSIGNMENTS (FIXED + SIMPLE SPLIT) =====

                var orderedAssignments = required
                    .OrderBy(a => a.DueDate)
                    .ThenBy(a => PriorityWeight(a.Priority))
                    .ToList();

                if (optional != null)
                    orderedAssignments.Add(optional);

                int sessionCount = 0;

                foreach (var a in orderedAssignments)
                {
                    bool isUrgent = a.DueDate.Date == today;

                    var chunks = SplitIntoChunks(a.EstimatedMinutes, isUrgent);

                    foreach (var duration in chunks)
                    {
                        if (current >= DAY_END) break;

                        current = GetSafeStart(current, duration);

                        // avoid lunch overlap
                        if (current < 13 * 60 && current + duration > 12 * 60)
                            current = 13 * 60;

                        var end = current + duration;

                        var days = (a.DueDate.Date - today).TotalDays;

                        AddBlock(current, duration, new ScheduleItem
                        {
                            Title = a.Title,
                            Description = $"{a.SubjectName} • {(days == 0 ? "⚠️ Due TODAY" : days == 1 ? "🔴 Due tomorrow" : $"📅 Due in {days} days")} • {duration} min",
                            ItemType = "assignment"
                        });

                        current = end;
                        sessionCount++;

                        // break every 2 sessions
                        if (sessionCount % 2 == 0)
                        {
                            AddBlock(current, 15, new ScheduleItem
                            {
                                Title = "Break Time",
                                Description = "Recharge ☕",
                                ItemType = "break"
                            });

                            current += 15;
                        }
                    }
                }

                // OPTIONAL
                if (optional != null)
                {
                    int duration = Math.Min(optional.EstimatedMinutes, 120);

                    current = GetSafeStart(current, duration);

                    AddBlock(current, duration, new ScheduleItem
                    {
                        Title = optional.Title + " (Optional)",
                        Description = $"{optional.SubjectName} • Challenge yourself 💪",
                        ItemType = "assignment",
                        IsOptional = true
                    });

                    current += duration;
                }

                // ===== FREE TIME (REAL GAP FILLING) =====
                int reviewStart = 17 * 60;

                var sortedBlocks = blocked.OrderBy(b => b.start).ToList();

                int pointer = DAY_START;

                foreach (var b in sortedBlocks)
                {
                    if (pointer < b.start)
                    {
                        int gap = b.start - pointer;

                        if (gap >= 20)
                        {
                            schedule.Add((pointer, b.start, new ScheduleItem
                            {
                                TimeSlot = $"{pointer / 60:D2}:{pointer % 60:D2} - {b.start / 60:D2}:{b.start % 60:D2}",
                                Title = "Free Time",
                                Description = "Relax, hobbies, chill 🎮",
                                ItemType = "freetime"
                            }));
                        }
                    }

                    pointer = Math.Max(pointer, b.end);
                }

                // tail gap
                if (pointer < DAY_END)
                {
                    schedule.Add((pointer, DAY_END, new ScheduleItem
                    {
                        TimeSlot = $"{pointer / 60:D2}:{pointer % 60:D2} - {DAY_END / 60:D2}:{DAY_END % 60:D2}",
                        Title = "Free Time",
                        Description = "Wind down 🌙",
                        ItemType = "freetime"
                    }));
                }

                // ===== REVIEW =====
                var tomorrow = today.AddDays(1);
                var tomorrowName = tomorrow.DayOfWeek.ToString();

                var reviews = subjects
                    .Where(s =>
                        !hasClassToday.Contains(s.Name) &&
                        (isWeekend ||
                         (!string.IsNullOrEmpty(s.ClassDays) &&
                          s.ClassDays.Split(';').Contains(tomorrowName[..3]))))
                    .Take(2)
                    .ToList();

                foreach (var s in reviews)
                {
                    AddBlock(reviewStart, 45, new ScheduleItem
                    {
                        Title = $"Review: {s.Name}",
                        Description = "Study notes, prep 📖",
                        ItemType = "study"
                    });

                    reviewStart += 55;
                }

                DailySchedule = schedule
                    .OrderBy(s => s.start)
                    .Select(s => s.item)
                    .ToList();

                ShowDaily = true;

                if (!required.Any() && !overdue.Any())
                    StatusMessage = "🎉 Nothing urgent today! Enjoy your free time.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task GenerateWeeklyScheduleAsync()
        {
            if (_authService.CurrentUser == null) return;

            IsLoading = true;
            ShowDaily = false;
            ShowWeekly = false;
            StatusMessage = "";

            try
            {
                var assignments = await _assignmentService.GetAssignmentsAsync(_authService.CurrentUser.Email);
                var subjects = await _subjectService.GetSubjectsAsync(_authService.CurrentUser.Email);

                var today = DateTime.Today;

                var pending = assignments.Where(a => !a.IsCompleted && a.DueDate >= today).ToList();
                var overdue = assignments.Where(a => !a.IsCompleted && a.DueDate.Date < today).ToList();

                var weekly = new List<DaySchedule>();
                var alreadyScheduled = new HashSet<string>();

                var assignmentChunks = new Dictionary<string, Queue<int>>();

                foreach (var a in pending)
                {
                    bool isUrgent = a.DueDate.Date == today;

                    var chunks = SplitIntoChunks(a.EstimatedMinutes, isUrgent);

                    assignmentChunks[a.FirestoreId] = new Queue<int>(chunks);
                }
                // ===== LOOP 7 DAYS =====
                for (int i = 0; i < 7; i++)
                {
                    var date = today.AddDays(i);
                    var dayName = date.DayOfWeek.ToString();
                    var isWeekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

                    var items = new List<ScheduleItem>();

                    // ===== TIMELINE SYSTEM (same as daily) =====
                    var blocked = new List<(int start, int end)>();

                    int GetSafeStart(int start, int duration)
                    {
                        while (true)
                        {
                            var overlap = blocked.FirstOrDefault(b =>
                                start < b.end && (start + duration) > b.start);

                            if (overlap == default)
                                return start;

                            start = overlap.end;
                        }
                    }

                    void AddBlock(int start, int duration, ScheduleItem item)
                    {
                        int safeStart = GetSafeStart(start, duration);
                        int end = safeStart + duration;

                        item.TimeSlot = $"{safeStart / 60:D2}:{safeStart % 60:D2}";

                        items.Add(item);
                        blocked.Add((safeStart, end));
                    }

                    // ===== MEALS (simplified display) =====
                    AddBlock(7 * 60, 30, new ScheduleItem { Title = "Breakfast 🍽️", ItemType = "meal" });
                    AddBlock(12 * 60, 60, new ScheduleItem { Title = "Lunch 🥗", ItemType = "meal" });
                    AddBlock(18 * 60 + 30, 30, new ScheduleItem { Title = "Dinner 🍽️", ItemType = "meal" });

                    // ===== OVERDUE (ONLY TODAY) =====
                    if (i == 0)
                    {
                        int overdueStart = 7 * 60 + 30;

                        foreach (var od in overdue.Take(2))
                        {
                            AddBlock(overdueStart, 60, new ScheduleItem
                            {
                                Title = $"🚨 {od.Title}",
                                Description = "Overdue — do ASAP!",
                                ItemType = "overdue"
                            });

                            overdueStart += 70;
                        }
                    }

                    // ===== CLASSES =====
                    var hasClassToday = new HashSet<string>();

                    foreach (var subject in subjects)
                    {
                        bool hasClass =
                            (!string.IsNullOrEmpty(subject.ClassDays) &&
                             subject.ClassDays.Split(';').Contains(dayName[..3])) ||
                            (!string.IsNullOrEmpty(subject.Schedule) &&
                             subject.Schedule.Contains(dayName[..3]));

                        if (!hasClass) continue;

                        hasClassToday.Add(subject.Name);

                        int start = 9 * 60;
                        int end = 10 * 60;

                        if (TimeSpan.TryParse(subject.ClassStartTime, out var s))
                            start = (int)s.TotalMinutes;

                        if (TimeSpan.TryParse(subject.ClassEndTime, out var e))
                            end = (int)e.TotalMinutes;

                        AddBlock(start, end - start, new ScheduleItem
                        {
                            Title = $"🏫 {subject.Name}",
                            Description = $"{subject.Room}",
                            ItemType = "class"
                        });
                    }

                    // ===== ASSIGNMENTS (SAME LOGIC AS DAILY) =====
                    var (required, _) = GetAssignmentsForDay(date, pending, alreadyScheduled);

                    int current = 9 * 60;

                    foreach (var a in required)
                    {
                        if (!assignmentChunks.ContainsKey(a.FirestoreId))
                            continue;

                        var queue = assignmentChunks[a.FirestoreId];

                        // take ONLY ONE chunk per day (THIS is the key fix)
                        if (queue.Count > 0)
                        {
                            int duration = queue.Dequeue();

                            current = GetSafeStart(current, duration);

                            AddBlock(current, duration, new ScheduleItem
                            {
                                Title = $"📝 {a.Title}",
                                Description = $"{a.SubjectName} • {duration} min",
                                ItemType = "assignment"
                            });

                            current += duration;

                            // only mark done if NO chunks left
                            if (queue.Count == 0)
                                alreadyScheduled.Add(a.FirestoreId);
                        }
                    }

                    int totalUsedMinutes = blocked.Sum(b => b.end - b.start);
                    int totalDayMinutes = 14 * 60; // 7AM–9PM approx

                    if (totalUsedMinutes < totalDayMinutes - 60) // at least 1 hour free
                    {
                        items.Add(new ScheduleItem
                        {
                            TimeSlot = "Flexible",
                            Title = "🎮 Free Time",
                            Description = "Relax or catch up",
                            ItemType = "freetime"
                        });
                    }

                    // ===== REVIEW (SAME RULES) =====
                    var tomorrow = date.AddDays(1);
                    var tomorrowName = tomorrow.DayOfWeek.ToString();

                    var reviews = subjects
                        .Where(s =>
                            !hasClassToday.Contains(s.Name) &&
                            (isWeekend ||
                             (!string.IsNullOrEmpty(s.ClassDays) &&
                              s.ClassDays.Split(';').Contains(tomorrowName[..3]))))
                        .Take(2)
                        .ToList();

                    int reviewStart = 17 * 60;

                    foreach (var s in reviews)
                    {
                        AddBlock(reviewStart, 45, new ScheduleItem
                        {
                            Title = $"📖 Review {s.Name}",
                            Description = "Prep for next class",
                            ItemType = "study"
                        });

                        reviewStart += 60;
                    }

                    // ===== FREE DAY =====
                    if (!required.Any() && !hasClassToday.Any())
                    {
                        items.Add(new ScheduleItem
                        {
                            TimeSlot = "All Day",
                            Title = "🎮 Free Day",
                            Description = "No tasks scheduled",
                            ItemType = "freetime"
                        });
                    }

                    weekly.Add(new DaySchedule
                    {
                        DayLabel = date.ToString("dddd, MMM dd"),
                        Items = items.OrderBy(i => i.TimeSlot).ToList()
                    });
                }

                WeeklySchedule = weekly;
                ShowWeekly = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private static int PriorityWeight(string priority) => priority switch
        {
            "High" => 0,
            "Medium" => 1,
            _ => 2
        };

        private List<int> SplitIntoChunks(int totalMinutes, bool isUrgent)
        {
            var chunks = new List<int>();

            if (isUrgent)
            {
                chunks.Add(totalMinutes);
                return chunks;
            }

            while (totalMinutes > 0)
            {
                int chunk = Math.Min(180, totalMinutes);
                chunks.Add(chunk);
                totalMinutes -= chunk;
            }

            return chunks;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}