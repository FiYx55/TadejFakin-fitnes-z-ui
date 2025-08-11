using Microsoft.Extensions.Logging;
using Fitnes_ai.Views;
using Fitnes_ai.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Fitnes_ai
{
    /// <summary>
    /// Main entry point for configuring and creating the .NET MAUI application.
    /// This class handles the setup of services, fonts, and other application-level configurations.
    /// </summary>
    public static class MauiProgram
    {
        /// <summary>
        /// Creates and configures the MauiApp instance.
        /// This method sets up dependency injection for database contexts, ViewModels, and Views,
        /// configures fonts, and enables debug logging.
        /// </summary>
        /// <returns>A configured MauiApp instance ready to run.</returns>
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register database context with a specific path in the app's data directory.
            // Using AddDbContext ensures that the AppDbContext is available for dependency injection
            // throughout the application with a singleton-like lifetime per scope.
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "fitnesAI.db");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Filename={dbPath}"));

            // Register ViewModels as transient services. This means a new instance is created
            // each time a ViewModel is requested, which is suitable for page-specific ViewModels.
            builder.Services.AddTransient<ExercisesViewModel>();
            builder.Services.AddTransient<PlanViewModel>();
            builder.Services.AddTransient<WorkoutViewModel>();
            builder.Services.AddTransient<PlanSelectionViewModel>();

            // Register Views (Pages) as transient services. This ensures that a new page instance
            // is created upon each navigation, which is a common pattern in MAUI.
            builder.Services.AddTransient<ExercisesPage>();
            builder.Services.AddTransient<PlanPage>();
            builder.Services.AddTransient<WorkoutPage>();
            builder.Services.AddTransient<PlanSelectionPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
