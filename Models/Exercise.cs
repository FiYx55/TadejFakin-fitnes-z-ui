using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Fitnes_ai.Models
{
    /// <summary>
    /// Represents a specific exercise instance within a workout.
    /// This model connects exercise data (name, muscle group, image) with workout-specific parameters (sets, reps, rest time).
    /// Each exercise belongs to a workout and references an ExerciseData for its basic information.
    /// </summary>
    public class Exercise
    {
        /// <summary>
        /// Gets or sets the unique identifier for this exercise instance.
        /// This is the primary key used by Entity Framework for database operations.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the foreign key that references the parent workout.
        /// This establishes the relationship between exercises and workouts in the database.
        /// </summary>
        public int WorkoutId { get; set; }

        /// <summary>
        /// Gets or sets the foreign key that references the exercise data.
        /// This links to the ExerciseData table which contains the exercise name, muscle group, and image.
        /// </summary>
        public int ExerciseDataId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the exercise data.
        /// This allows Entity Framework to load related exercise information (name, muscle group, image).
        /// Can be null if the navigation property is not loaded via Include().
        /// </summary>
        public ExerciseData? ExerciseData { get; set; }

        /// <summary>
        /// Gets or sets the number of sets to be performed for this exercise in the workout.
        /// This is a workout-specific parameter that can vary for the same exercise in different workouts.
        /// </summary>
        public int Sets { get; set; }

        /// <summary>
        /// Gets or sets the number of repetitions to be performed per set.
        /// This is a workout-specific parameter that defines the intensity and volume of the exercise.
        /// </summary>
        public int Reps { get; set; }

        /// <summary>
        /// Gets or sets the rest time between sets in seconds.
        /// This parameter helps users manage their workout timing and recovery between sets.
        /// </summary>
        public int RestTime { get; set; } // in seconds

        /// <summary>
        /// Gets or sets an optional image specific to this exercise instance.
        /// This is currently not used but could be used for custom exercise variations or progress photos.
        /// The main exercise image comes from the ExerciseData navigation property.
        /// </summary>
        public byte[]? Image { get; set; }
    }
}
