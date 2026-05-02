using StudySync.Shared.Services;

namespace StudySync
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Only register pages that are NOT already defined in AppShell.xaml
            Routing.RegisterRoute("addsubject", typeof(AddSubjectPage));
            Routing.RegisterRoute("addassignment", typeof(AddAssignmentPage));
        }
    }
}