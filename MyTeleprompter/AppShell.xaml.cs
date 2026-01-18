namespace MyTeleprompter
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes so GoToAsync works
            Routing.RegisterRoute(nameof(Views.NoteEntryPage), typeof(Views.NoteEntryPage));
            Routing.RegisterRoute(nameof(Views.TeleprompterPage), typeof(Views.TeleprompterPage));
        }
    }
}
