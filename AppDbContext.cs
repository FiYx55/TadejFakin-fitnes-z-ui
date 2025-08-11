using Fitnes_ai.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace Fitnes_ai
{
    /// <summary>
    /// Entity Framework database context for the fitness application.
    /// Manages database connections, entity sets, and entity relationships.
    /// Uses SQLite as the database provider with local file storage.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the AppDbContext with the specified options.
        /// This constructor is used by the dependency injection system.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the AppDbContext with default configuration.
        /// This parameterless constructor is used for scenarios where DI is not available.
        /// </summary>
        public AppDbContext()
        {
        }

        /// <summary>
        /// Gets or sets the DbSet for workout plans.
        /// Provides access to workout plan entities in the database.
        /// </summary>
        public DbSet<WorkoutPlan> WorkoutPlans { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for workouts.
        /// Provides access to individual workout entities in the database.
        /// </summary>
        public DbSet<Workout> Workouts { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for exercise instances.
        /// Provides access to exercise entities that belong to workouts.
        /// </summary>
        public DbSet<Exercise> Exercises { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for exercise data.
        /// Provides access to the static exercise information (names, muscle groups, images).
        /// </summary>
        public DbSet<ExerciseData> ExercisesData { get; set; }

        /// <summary>
        /// Configures the database connection if not already configured.
        /// Sets up SQLite database with a local file in the app data directory.
        /// </summary>
        /// <param name="optionsBuilder">The options builder used to configure the context</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "fitnesAI.db");
                optionsBuilder.UseSqlite($"Filename={dbPath}");
            }
        }

        /// <summary>
        /// Configures entity relationships and database schema using Fluent API.
        /// Defines foreign key relationships, cascade behaviors, and entity constraints.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to configure entity relationships</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure WorkoutPlan -> Workout relationship
            // One WorkoutPlan can have many Workouts
            // Deleting a WorkoutPlan will cascade delete all its Workouts
            modelBuilder.Entity<Workout>()
                .HasOne(w => w.WorkoutPlan)
                .WithMany(p => p.Workouts)
                .HasForeignKey(w => w.WorkoutPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Workout -> Exercise relationship
            // One Workout can have many Exercises
            // Deleting a Workout will cascade delete all its Exercises
            modelBuilder.Entity<Exercise>()
                .HasOne<Workout>()
                .WithMany(w => w.Exercises)
                .HasForeignKey(e => e.WorkoutId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Exercise -> ExerciseData relationship
            // One ExerciseData can be referenced by many Exercises
            // Deleting ExerciseData is restricted if referenced by Exercises
            modelBuilder.Entity<Exercise>()
                .HasOne(e => e.ExerciseData)
                .WithMany()
                .HasForeignKey(e => e.ExerciseDataId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
