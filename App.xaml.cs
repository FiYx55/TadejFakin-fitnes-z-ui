using Fitnes_ai.Models;
using System.Text.Json;

namespace Fitnes_ai
{
    /// <summary>
    /// Main application class for the Fitnes-ai app.
    /// Handles application startup, initialization, and database setup.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the App class.
        /// Sets up the main page and initializes the database.
        /// </summary>
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();

            InitializeDatabase();
        }

        /// <summary>
        /// Initializes the database by ensuring it is created and seeding it with
        /// exercise data if it's empty. This prevents overwriting user data on subsequent launches.
        /// </summary>
        private async void InitializeDatabase()
        {
            using var db = new AppDbContext();

            if (db.Database.EnsureCreated())
            {
                // Database was created (first time) - seed exercises
                System.Diagnostics.Debug.WriteLine("DB created — seeding exercises...");
                await SeedExercisesFromJson(db);
                System.Diagnostics.Debug.WriteLine("Exercise seeding completed");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("DB already exists — checking if exercises need seeding");
                
                // Database exists - only seed exercises if ExercisesData table is empty
                if (!db.ExercisesData.Any())
                {
                    System.Diagnostics.Debug.WriteLine("No exercises found, seeding exercises...");
                    await SeedExercisesFromJson(db);
                    System.Diagnostics.Debug.WriteLine("Exercise seeding completed");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Database has {db.ExercisesData.Count()} exercises, skipping seeding");
                }
            }
        }

        /// <summary>
        /// Seeds the database with exercise data from the embedded 'exercises.json' file.
        /// This method is called only when the database is first created or if the
        /// ExercisesData table is empty, ensuring data integrity.
        /// </summary>
        /// <param name="db">The AppDbContext instance to use for database operations.</param>
        private static async Task SeedExercisesFromJson(AppDbContext db)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting exercise seeding...");
                using var stream = await FileSystem.OpenAppPackageFileAsync("exercises.json");
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();

                System.Diagnostics.Debug.WriteLine($"JSON loaded: {json.Length} characters");

                var data = JsonSerializer.Deserialize<List<ExerciseDataJson>>(json);
                System.Diagnostics.Debug.WriteLine($"Deserialized {data?.Count} exercises");

                if (data != null && data.Count > 0)
                {
                    var exercises = data.Select(x => new ExerciseData
                    {
                        Name = x.Name,
                        MuscleGroup = x.MuscleGroup,
                        Image = !string.IsNullOrEmpty(x.Image) ? Convert.FromBase64String(x.Image) : null
                    }).ToList();

                    System.Diagnostics.Debug.WriteLine($"Converting {exercises.Count} exercises to ExerciseData objects");

                    db.ExercisesData.AddRange(exercises);
                    var savedCount = await db.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"Saved {savedCount} exercises to DB successfully");
                    
                    // Verify data was saved
                    var verifyCount = db.ExercisesData.Count();
                    System.Diagnostics.Debug.WriteLine($"Verification: {verifyCount} exercises now in database");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No exercise data to seed");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Seeding failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Represents the structure of exercise data in the JSON file for deserialization.
        /// Used as an intermediate object before creating ExerciseData entities.
        /// </summary>
        public class ExerciseDataJson
        {
            /// <summary>
            /// Gets or sets the name of the exercise.
            /// </summary>
            public string Name { get; set; } = string.Empty;
            
            /// <summary>
            /// Gets or sets the primary muscle group targeted by the exercise.
            /// </summary>
            public string MuscleGroup { get; set; } = string.Empty;
            
            /// <summary>
            /// Gets or sets the Base64 encoded string representation of the exercise image.
            /// </summary>
            public string Image { get; set; } = string.Empty; // Base64
        }
    }
}
