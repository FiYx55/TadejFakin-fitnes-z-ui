using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitnes_ai.Models
{
    /// <summary>
    /// Represents a workout plan that contains multiple workouts.
    /// A workout plan is the top-level container for organizing fitness routines.
    /// </summary>
    public class WorkoutPlan
    {
        /// <summary>
        /// Gets or sets the unique identifier for the workout plan.
        /// This is the primary key used by Entity Framework for database operations.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the workout plan.
        /// This is displayed in the UI and helps users identify different plans.
        /// Default value is "Workout plan".
        /// </summary>
        public string Name { get; set; } = "Workout plan";

        /// <summary>
        /// Gets or sets the collection of workouts that belong to this plan.
        /// This navigation property allows Entity Framework to load related workouts.
        /// Each workout in the list belongs to this specific plan.
        /// </summary>
        public List<Workout> Workouts { get; set; } = new();
    }
}
