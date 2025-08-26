using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Fitnes_ai.Models;
using Fitnes_ai.Views;
using Microsoft.Maui.Storage;

namespace Fitnes_ai.ViewModels
{
    /// <summary>
    /// ViewModel for the Exercises page that manages exercise data browsing and selection.
    /// Supports two modes: general exercise browsing and workout-specific exercise selection.
    /// Provides filtering capabilities by name search and muscle group categories.
    /// </summary>
    [QueryProperty(nameof(WorkoutId), "WorkoutId")]
    public class ExercisesViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _context;
        private ObservableCollection<ExerciseData> _allExercises;
        private ObservableCollection<ExerciseData> _filteredExercises;
        private string _searchText = string.Empty;
        private string _selectedMuscleGroup = "All";
        private bool _isLoading;
        private int _workoutId;

        /// <summary>
        /// Initializes a new instance of the ExercisesViewModel class.
        /// Sets up collections, commands, and triggers initial loading of exercise data.
        /// </summary>
        /// <param name="context">The database context for exercise operations.</param>
        public ExercisesViewModel(AppDbContext context)
        {
            _context = context;
            _allExercises = new ObservableCollection<ExerciseData>();
            _filteredExercises = new ObservableCollection<ExerciseData>();
            
            ExerciseTappedCommand = new Command<ExerciseData>(OnExerciseTapped);
            
            // Load exercises asynchronously for better performance
            LoadExercisesFromDatabase();
        }

        /// <summary>
        /// Gets or sets the ID of the workout for exercise selection mode.
        /// When set to a value > 0, enables workout selection mode where tapping
        /// exercises adds them directly to the specified workout.
        /// </summary>
        public int WorkoutId
        {
            get => _workoutId;
            set
            {
                _workoutId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsInWorkoutSelectionMode));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the ViewModel is in workout selection mode.
        /// When true, tapping exercises adds them to a specific workout.
        /// When false, tapping exercises shows workout selection dialog.
        /// </summary>
        public bool IsInWorkoutSelectionMode => WorkoutId > 0;

        /// <summary>
        /// Gets or sets the collection of exercises after applying search and filter criteria.
        /// This is the collection bound to the UI for display.
        /// </summary>
        public ObservableCollection<ExerciseData> FilteredExercises
        {
            get => _filteredExercises;
            set
            {
                _filteredExercises = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the ViewModel is currently loading exercise data.
        /// Used to show loading indicators while exercise data is being fetched from database.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the search text for filtering exercises by name.
        /// When changed, automatically triggers exercise filtering.
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterExercises();
            }
        }

        /// <summary>
        /// Gets or sets the selected muscle group for filtering exercises.
        /// Defaults to "All" to show exercises from all muscle groups.
        /// When changed, automatically triggers exercise filtering.
        /// </summary>
        public string SelectedMuscleGroup
        {
            get => _selectedMuscleGroup;
            set
            {
                _selectedMuscleGroup = value;
                OnPropertyChanged();
                FilterExercises();
            }
        }

        /// <summary>
        /// Gets the list of available muscle groups for filtering.
        /// Populated from the database and includes "All" option for showing all exercises.
        /// </summary>
        public List<string> MuscleGroups { get; private set; } = new() { "All" };

        /// <summary>
        /// Command to handle exercise selection/tapping.
        /// Behavior depends on whether ViewModel is in workout selection mode.
        /// </summary>
        public ICommand ExerciseTappedCommand { get; }

        /// <summary>
        /// Handles exercise tapping by determining the appropriate action based on current mode.
        /// In workout selection mode, shows dialog to add exercise to workout.
        /// In browsing mode, shows workout selection dialog.
        /// </summary>
        /// <param name="exercise">The exercise that was tapped.</param>
        private async void OnExerciseTapped(ExerciseData exercise)
        {
            if (exercise?.Id == null) return;

            if (IsInWorkoutSelectionMode)
            {
                // Show dialog to add exercise to workout with sets, reps, and rest time
                await ShowAddExerciseDialog(exercise);
            }
            else
            {
                // Normal exercise browsing mode - show workout selection dialog
                await ShowWorkoutSelectionDialog(exercise);
            }
        }

        /// <summary>
        /// Shows a dialog for selecting which workout to add the exercise to.
        /// Used when not in workout selection mode - allows choosing from available workouts.
        /// </summary>
        /// <param name="exerciseData">The exercise to add to a workout.</param>
        private async Task ShowWorkoutSelectionDialog(ExerciseData exerciseData)
        {
            if (Application.Current?.MainPage == null) return;

            try
            {
                // Get all workouts from the current plan
                var currentPlan = GetCurrentPlan();
                if (currentPlan == null)
                {
                    await Application.Current.MainPage.DisplayAlert("No Plan", "Please create a workout plan first.", "OK");
                    return;
                }

                var workouts = _context.Workouts.Where(w => w.WorkoutPlanId == currentPlan.Id).ToList();
                if (!workouts.Any())
                {
                    await Application.Current.MainPage.DisplayAlert("No Workouts", "Please create a workout first.", "OK");
                    return;
                }

                // Show workout selection
                var workoutNames = workouts.Select(w => w.Title).ToArray();
                var selectedWorkout = await Application.Current.MainPage.DisplayActionSheet(
                    $"Add '{exerciseData.Name}' to which workout?",
                    "Cancel",
                    null,
                    workoutNames);

                if (selectedWorkout == "Cancel" || string.IsNullOrEmpty(selectedWorkout))
                    return;

                var workout = workouts.FirstOrDefault(w => w.Title == selectedWorkout);
                if (workout != null)
                {
                    // Set the workout ID temporarily and show the add dialog
                    var originalWorkoutId = WorkoutId;
                    WorkoutId = workout.Id;
                    await ShowAddExerciseDialog(exerciseData);
                    WorkoutId = originalWorkoutId;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing workout selection: {ex.Message}");
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to show workout selection", "OK");
            }
        }

        /// <summary>
        /// Gets the current active workout plan from user preferences.
        /// Falls back to the first available plan if no preference is set.
        /// </summary>
        /// <returns>The current workout plan, or null if no plans exist.</returns>
        private WorkoutPlan? GetCurrentPlan()
        {
            try
            {
                var defaultPlanId = Preferences.Get("default_plan_id", -1);
                if (defaultPlanId != -1)
                {
                    return _context.WorkoutPlans.FirstOrDefault(p => p.Id == defaultPlanId);
                }
                return _context.WorkoutPlans.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Shows a dialog series for adding an exercise to a workout with custom parameters.
        /// Prompts user for sets, reps, and rest time, then creates the exercise in the database.
        /// </summary>
        /// <param name="exerciseData">The exercise data to add to the workout.</param>
        private async Task ShowAddExerciseDialog(ExerciseData exerciseData)
        {
            if (Application.Current?.MainPage == null) return;

            try
            {
                // Create a simple input dialog for sets, reps, and rest time
                string setsInput = await Application.Current.MainPage.DisplayPromptAsync(
                    "Add Exercise", 
                    $"Adding '{exerciseData.Name}' to workout.\nEnter number of sets:", 
                    "OK", "Cancel", 
                    "3", 
                    keyboard: Keyboard.Numeric);

                if (string.IsNullOrEmpty(setsInput)) return;

                string repsInput = await Application.Current.MainPage.DisplayPromptAsync(
                    "Add Exercise", 
                    "Enter number of reps:", 
                    "OK", "Cancel", 
                    "10", 
                    keyboard: Keyboard.Numeric);

                if (string.IsNullOrEmpty(repsInput)) return;

                string restInput = await Application.Current.MainPage.DisplayPromptAsync(
                    "Add Exercise", 
                    "Enter rest time (seconds):", 
                    "OK", "Cancel", 
                    "60", 
                    keyboard: Keyboard.Numeric);

                if (string.IsNullOrEmpty(restInput)) return;

                // Parse inputs
                if (int.TryParse(setsInput, out int sets) && 
                    int.TryParse(repsInput, out int reps) && 
                    int.TryParse(restInput, out int restTime))
                {
                    // Create new exercise for the workout
                    var exercise = new Exercise
                    {
                        ExerciseDataId = exerciseData.Id,
                        WorkoutId = WorkoutId,
                        Sets = sets,
                        Reps = reps,
                        RestTime = restTime
                    };

                    System.Diagnostics.Debug.WriteLine($"ExercisesViewModel: Creating exercise with ExerciseDataId: {exerciseData.Id}, WorkoutId: {WorkoutId}");
                    
                    _context.Exercises.Add(exercise);
                    await _context.SaveChangesAsync();

                    System.Diagnostics.Debug.WriteLine($"ExercisesViewModel: Exercise saved with ID: {exercise.Id}");

                    await Application.Current.MainPage.DisplayAlert(
                        "Success", 
                        $"'{exerciseData.Name}' added to workout!", 
                        "OK");

                    // Navigate back to workout page if we came from workout selection mode
                    if (IsInWorkoutSelectionMode)
                    {
                        await Shell.Current.GoToAsync($"//PlanPage/{nameof(WorkoutPage)}", new Dictionary<string, object>
                        {
                            { "WorkoutId", WorkoutId }
                        });
                    }
                    else
                    {
                        // Clear the temporary workout ID
                        WorkoutId = 0;
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Please enter valid numbers", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding exercise to workout: {ex.Message}");
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to add exercise to workout", "OK");
            }
        }

        /// <summary>
        /// Asynchronously loads all exercise data from the database on a background thread.
        /// Updates the exercises collection and builds muscle group filter list.
        /// </summary>
        private async void LoadExercisesFromDatabase()
        {
            IsLoading = true;
            try
            {
                // Run database operation on background thread
                var exercises = await Task.Run(() =>
                {
                    return _context.ExercisesData.ToList();
                });
                
                System.Diagnostics.Debug.WriteLine($"Loaded {exercises.Count} exercises from database");
                
                // Update UI on main thread
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    _allExercises.Clear();
                    foreach (var exercise in exercises)
                    {
                        _allExercises.Add(exercise);
                    }

                    // Get unique muscle groups from the database
                    var muscleGroups = exercises.Select(e => e.MuscleGroup).Distinct().OrderBy(m => m).ToList();
                    MuscleGroups = new List<string> { "All" };
                    MuscleGroups.AddRange(muscleGroups);
                    OnPropertyChanged(nameof(MuscleGroups));

                    FilteredExercises = new ObservableCollection<ExerciseData>(_allExercises);
                    System.Diagnostics.Debug.WriteLine($"FilteredExercises count: {FilteredExercises.Count}");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading exercises: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Applies search and muscle group filters to the exercise collection.
        /// Updates FilteredExercises with exercises matching current filter criteria.
        /// </summary>
        private void FilterExercises()
        {
            var filtered = _allExercises.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(e => e.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            if (SelectedMuscleGroup != "All")
            {
                filtered = filtered.Where(e => e.MuscleGroup == SelectedMuscleGroup);
            }

            FilteredExercises = new ObservableCollection<ExerciseData>(filtered);
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
