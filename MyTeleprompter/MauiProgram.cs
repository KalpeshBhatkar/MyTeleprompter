using Camera.MAUI;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace MyTeleprompter
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit() // Initialize Toolkit
                .UseMauiCameraView() // Initialize Camera
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            #if DEBUG
    		builder.Logging.AddDebug();
            #endif
            // Register Services
            builder.Services.AddSingleton<Services.DatabaseService>();

            // ViewModels
            builder.Services.AddTransient<ViewModels.NotesListViewModel>();
            builder.Services.AddTransient<ViewModels.NoteEntryViewModel>();
            builder.Services.AddTransient<ViewModels.TeleprompterViewModel>();

            // Pages
            builder.Services.AddTransient<Views.NotesListPage>();
            builder.Services.AddTransient<Views.NoteEntryPage>();
            builder.Services.AddTransient<Views.TeleprompterPage>();

            return builder.Build();
        }
    }
}
