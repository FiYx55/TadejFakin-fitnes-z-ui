using Fitnes_ai.Views;

namespace Fitnes_ai
{
    /// <summary>
    /// Represents the main shell of the application, which defines the overall structure
    /// and navigation hierarchy. It sets up the tab bar and registers routes for page navigation.
    /// </summary>
    public partial class AppShell : Shell
    {
        /// <summary>
        /// Initializes a new instance of the AppShell class.
        /// This constructor initializes the shell components and registers all necessary
        /// navigation routes for the application's pages.
        /// </summary>
        public AppShell()
        {
            InitializeComponent();

            // Register routes for all pages to enable programmatic navigation.
            // This allows navigating to pages using their string names, which is essential
            // for decoupling navigation logic from view implementations.
            Routing.RegisterRoute(nameof(ExercisesPage), typeof(ExercisesPage));
            Routing.RegisterRoute(nameof(WorkoutPage), typeof(WorkoutPage));
            Routing.RegisterRoute(nameof(PlanPage), typeof(PlanPage));
            Routing.RegisterRoute(nameof(PlanSelectionPage), typeof(PlanSelectionPage));
        }
    }
}
