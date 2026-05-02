using StudySync.ViewModels;
using StudySync.Shared.Models;
using StudySync.Shared.Services;

namespace StudySync;

public partial class SubjectsPage : ContentPage
{
    private readonly SubjectsViewModel _viewModel;

    public SubjectsPage()
    {
        InitializeComponent();
        var services = Application.Current?.Handler?.MauiContext?.Services;
        _viewModel = services?.GetService<SubjectsViewModel>() ?? new SubjectsViewModel(
            services?.GetService<ISubjectService>()!,
            services?.GetService<IAuthService>()!);
        BindingContext = _viewModel;
    }

    public SubjectsPage(SubjectsViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSubjectsAsync();
    }

    private async void OnAddSubjectClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("addsubject");
    }

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is Subject subject)
        {
            var name = await DisplayPromptAsync("Edit Subject", "Subject Name:", initialValue: subject.Name);
            if (string.IsNullOrEmpty(name)) return;

            var instructor = await DisplayPromptAsync("Edit Subject", "Instructor:", initialValue: subject.Instructor);
            var room = await DisplayPromptAsync("Edit Subject", "Room:", initialValue: subject.Room);
            var schedule = await DisplayPromptAsync("Edit Subject", "Schedule:", initialValue: subject.Schedule);

            subject.Name = name;
            subject.Instructor = instructor ?? subject.Instructor;
            subject.Room = room ?? subject.Room;
            subject.Schedule = schedule ?? subject.Schedule;

            try
            {
                await _viewModel.UpdateSubjectAsync(subject);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is Subject subject)
        {
            bool confirm = await DisplayAlertAsync("Delete Subject",
                $"Are you sure you want to delete '{subject.Name}'?", "Yes", "No");

            if (confirm)
            {
                try
                {
                    await _viewModel.DeleteSubjectAsync(subject);
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync("Error", ex.Message, "OK");
                }
            }
        }
    }
}