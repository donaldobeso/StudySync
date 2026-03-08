using StudySync.Shared.Models;
using StudySync.Shared.Services;
using StudySync.Shared.Helpers;

namespace StudySync;

public partial class MainPage : ContentPage
{
    private readonly IAssignmentService _service = new AssignmentService();

    public MainPage()
    {
        InitializeComponent();
        RefreshList();
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
        string title = TitleEntry.Text ?? "";
        string subject = SubjectPicker.SelectedItem?.ToString() ?? "";
        string priority = PriorityPicker.SelectedItem?.ToString() ?? "Medium";

        if (!Validator.IsNotEmpty(title))
        {
            ResultLabel.TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#C62828");
            ResultLabel.Text = "⚠️ Please enter an assignment title.";
            return;
        }

        var assignment = new Assignment
        {
            Title = title,
            SubjectName = subject,
            Priority = priority,
            DueDate = DateTime.Now.AddDays(3),
            IsCompleted = CompletedSwitch.IsToggled
        };

        _service.AddAssignment(assignment);

        ResultLabel.TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#2E7D32");
        ResultLabel.Text = $"✅ '{title}' saved successfully!";

        TitleEntry.Text = string.Empty;
        SubjectPicker.SelectedIndex = -1;
        PriorityPicker.SelectedIndex = -1;
        CompletedSwitch.IsToggled = false;

        RefreshList();
    }

    private void RefreshList()
    {
        AssignmentList.ItemsSource = null;
        AssignmentList.ItemsSource = _service.GetAssignments();
    }
}