using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Fitnes_ai.Models
{
    /// <summary>
    /// Represents the static data for an exercise type.
    /// This model contains the base information about exercises that doesn't change between workouts,
    /// such as the exercise name, target muscle group, and demonstration image.
    /// This data is typically seeded from a JSON file and shared across all exercise instances.
    /// </summary>
    public class ExerciseData
    {
        /// <summary>
        /// Gets or sets the unique identifier for the exercise data.
        /// This is the primary key used by Entity Framework and referenced by Exercise.ExerciseDataId.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the exercise.
        /// This is the display name shown in the UI (e.g., "Push-ups", "Bench Press", "Squats").
        /// Default value is empty string.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the primary muscle group targeted by this exercise.
        /// Used for filtering and categorizing exercises in the UI.
        /// Examples: "Chest", "Back", "Legs", "Arms", "Core".
        /// Default value is empty string.
        /// </summary>
        public string MuscleGroup { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the exercise demonstration image as a byte array.
        /// This image is typically loaded from base64 data in the exercises.json file.
        /// Used to help users understand proper exercise form and technique.
        /// Can be null if no image is available.
        /// </summary>
        public byte[]? Image { get; set; }
    }
}
