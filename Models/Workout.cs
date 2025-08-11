using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Fitnes_ai.Models
{
    /// <summary>
    /// Represents an individual workout session that contains multiple exercises.
    /// A workout belongs to a workout plan and consists of a collection of exercises with their specific parameters.
    /// </summary>
    public class Workout
    {
        /// <summary>
        /// Gets or sets the unique identifier for the workout.
        /// This is the primary key used by Entity Framework for database operations.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the title/name of the workout.
        /// This is displayed in the UI to help users identify different workouts within a plan.
        /// Default value is "Workout".
        /// </summary>
        public string Title { get; set; } = "Workout";

        /// <summary>
        /// Gets or sets the collection of exercises that belong to this workout.
        /// This navigation property allows Entity Framework to load related exercises.
        /// Each exercise contains specific sets, reps, and rest time parameters.
        /// </summary>
        public List<Exercise> Exercises { get; set; } = new();

        /// <summary>
        /// Gets or sets the foreign key that references the parent workout plan.
        /// This establishes the relationship between workouts and workout plans in the database.
        /// </summary>
        public int WorkoutPlanId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the parent workout plan.
        /// This allows Entity Framework to load the related workout plan when needed.
        /// Can be null if the navigation property is not loaded.
        /// </summary>
        public WorkoutPlan? WorkoutPlan { get; set; }
    }
}
