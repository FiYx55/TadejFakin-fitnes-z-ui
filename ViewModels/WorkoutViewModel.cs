using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Fitnes_ai.Models;
using Microsoft.EntityFrameworkCore;

namespace Fitnes_ai.ViewModels
{
    /// <summary>
    /// ViewModel for the Workout page that manages a specific workout and its exercises.
    /// Handles loading workout details, displaying exercises, and providing exercise management operations.
    /// Uses QueryProperty to receive WorkoutId from navigation parameters.
    /// </summary>
    [QueryProperty(nameof(WorkoutId), "WorkoutId")]
    public class WorkoutViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _context;
        private int _workoutId;
        private Workout? _selectedWorkout;
        private readonly ObservableCollection<Exercise> _exercises;
        private bool _isLoading;

        /// <summary>
        /// Initializes a new instance of the WorkoutViewModel class.
        /// Sets up the exercises collection and command bindings for exercise management.
        /// </summary>
        /// <param name="context">The database context for workout and exercise operations.</param>
        public WorkoutViewModel(AppDbContext context)
        {
            _context = context;
            _exercises = new ObservableCollection<Exercise>();
            Exercises = new ReadOnlyObservableCollection<Exercise>(_exercises);
            
            AddExerciseCommand = new Command(async () => await OnAddExercise());
            RemoveExerciseCommand = new Command<Exercise>(async (exercise) => await OnRemoveExercise(exercise));
        }

        /// <summary>
        /// Gets or sets the ID of the workout to display and manage.
        /// This property is populated via QueryProperty from navigation parameters.
        /// When set, triggers loading of workout details and associated exercises.
        /// </summary>
        public int WorkoutId
        {
            get => _workoutId;
            set
            {
                System.Diagnostics.Debug.WriteLine($"WorkoutViewModel: Setting WorkoutId to {value}");
                _workoutId = value;
                OnPropertyChanged();
                if (value > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"WorkoutViewModel: Loading workout with ID {value}");
                    _ = LoadWorkoutAndExercises();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"WorkoutViewModel: No workout ID provided, showing empty state");
                    // Handle case when no workout ID is provided
                    SelectedWorkout = null;
                    _exercises.Clear();
                    OnPropertyChanged(nameof(IsEmpty));
                    OnPropertyChanged(nameof(EmptyStateMessage));
                    OnPropertyChanged(nameof(EmptyStateSubMessage));
                }
            }
        }

        /// <summary>
        /// Gets the currently selected workout being displayed and managed.
        /// Set internally when a workout is successfully loaded from the database.
        /// </summary>
        public Workout? SelectedWorkout
        {
            get => _selectedWorkout;
            private set
            {
                _selectedWorkout = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WorkoutTitle));
            }
        }

        /// <summary>
        /// Gets a read-only observable collection of exercises in the current workout.
        /// This collection is updated when exercises are added or removed from the workout.
        /// </summary>
        public ReadOnlyObservableCollection<Exercise> Exercises { get; }
        
        /// <summary>
        /// Gets a value indicating whether the current workout has no exercises.
        /// Used for displaying empty state messages in the UI.
        /// </summary>
        public bool IsEmpty => !_isLoading && !_exercises.Any();

        /// <summary>
        /// Gets or sets a value indicating whether the ViewModel is currently loading data.
        /// Used to show loading indicators while workout and exercise data is being fetched.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

        /// <summary>
        /// Gets the title of the current workout for display in the UI.
        /// Returns "Workout" as default if no workout is selected.
        /// </summary>
        public string WorkoutTitle => SelectedWorkout?.Title ?? "Workout";
        
        /// <summary>
        /// Gets the primary empty state message based on whether a workout is selected.
        /// Shows different messages for no workout selected vs. workout with no exercises.
        /// </summary>
        public string EmptyStateMessage => WorkoutId <= 0 ? "No workout selected" : "No exercises added yet";
        
        /// <summary>
        /// Gets the secondary empty state message with actionable guidance.
        /// Provides different instructions based on the current state.
        /// </summary>
        public string EmptyStateSubMessage => WorkoutId <= 0 ? "Select a workout from the Plan tab to get started" : "Tap 'Add Exercise' to get started";

        /// <summary>
        /// Command to handle adding a new exercise to the current workout.
        /// Navigates to the exercises selection page.
        /// </summary>
        public ICommand AddExerciseCommand { get; }
        
        /// <summary>
        /// Command to handle removing an exercise from the current workout.
        /// Shows confirmation dialog before removing the exercise.
        /// </summary>
        public ICommand RemoveExerciseCommand { get; }

        /// <summary>
        /// Asynchronously loads the workout details and associated exercises from the database.
        /// Includes ExerciseData information for each exercise and updates the UI collection.
        /// </summary>
        private async Task LoadWorkoutAndExercises()
        {
            IsLoading = true;
            try
            {
                System.Diagnostics.Debug.WriteLine($"WorkoutViewModel: Loading workout with ID {WorkoutId}");
                
                // Load workout
                SelectedWorkout = await _context.Workouts
                    .FirstOrDefaultAsync(w => w.Id == WorkoutId);

                if (SelectedWorkout == null)
                {
                    System.Diagnostics.Debug.WriteLine($"WorkoutViewModel: No workout found with ID {WorkoutId}");
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Error", "Workout not found", "OK");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"WorkoutViewModel: Found workout '{SelectedWorkout.Title}'");

                // Load exercises for this workout with their ExerciseData
                var workoutExercises = await _context.Exercises
                    .Include(e => e.ExerciseData)
                    .Where(e => e.WorkoutId == WorkoutId)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"WorkoutViewModel: Found {workoutExercises.Count} exercises for workout");
                foreach (var ex in workoutExercises)
                {
                    System.Diagnostics.Debug.WriteLine($"Exercise ID: {ex.Id}, ExerciseDataId: {ex.ExerciseDataId}, ExerciseData: {(ex.ExerciseData != null ? ex.ExerciseData.Name : "NULL")}");
                }

                _exercises.Clear();
                foreach (var exercise in workoutExercises)
                {
                    System.Diagnostics.Debug.WriteLine($"WorkoutViewModel: Adding exercise - ExerciseDataId: {exercise.ExerciseDataId}, ExerciseData Name: {exercise.ExerciseData?.Name ?? "NULL"}");
                    _exercises.Add(exercise);
                }

                OnPropertyChanged(nameof(IsEmpty));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading workout and exercises: {ex.Message}");
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to load workout", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Handles the add exercise command by navigating to the exercises selection page.
        /// Validates that a workout is selected before allowing exercise addition.
        /// </summary>
        private async Task OnAddExercise()
        {
            if (WorkoutId <= 0)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("No Workout", "Please select a workout from the Plan tab first.", "OK");
                return;
            }

            try
            {
                // Navigate to exercises page with the current workout ID for exercise selection
                await Shell.Current.GoToAsync("//ExercisesPage", new Dictionary<string, object>
                {
                    { "WorkoutId", WorkoutId }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navigating to add exercise: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the remove exercise command by showing a confirmation dialog
        /// and removing the specified exercise from both database and UI collection.
        /// </summary>
        /// <param name="exercise">The exercise to remove from the current workout.</param>
        private async Task OnRemoveExercise(Exercise exercise)
        {
            if (exercise == null || Application.Current?.MainPage == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Remove Exercise", 
                $"Are you sure you want to remove '{exercise.ExerciseData?.Name}' from this workout?", 
                "Yes", "No");

            if (!confirm) return;

            try
            {
                _context.Exercises.Remove(exercise);
                await _context.SaveChangesAsync();

                _exercises.Remove(exercise);
                OnPropertyChanged(nameof(IsEmpty));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing exercise: {ex.Message}");
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to remove exercise", "OK");
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// Part of the INotifyPropertyChanged interface for data binding support.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// Uses CallerMemberName attribute to automatically get the calling property name.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed. Automatically populated by CallerMemberName.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
