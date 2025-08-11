using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Fitnes_ai.Models;
using Fitnes_ai.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;

namespace Fitnes_ai.ViewModels
{
    /// <summary>
    /// ViewModel for the Plan page that manages workout plans and their associated workouts.
    /// Handles displaying the currently active plan, managing workouts within that plan,
    /// and providing CRUD operations for workout management.
    /// </summary>
    public class PlanViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _context;
        private WorkoutPlan? _currentPlan;
        private ObservableCollection<Workout> _workouts;
        private bool _isLoading;

        /// <summary>
        /// Initializes a new instance of the PlanViewModel class.
        /// Sets up command bindings and triggers the initial plan loading process.
        /// </summary>
        /// <param name="context">The database context for workout plan operations.</param>
        public PlanViewModel(AppDbContext context)
        {
            _context = context;
            _workouts = new ObservableCollection<Workout>();
            
            AddWorkoutCommand = new Command(OnAddWorkout);
            DeleteWorkoutCommand = new Command<Workout>(OnDeleteWorkout);
            WorkoutTappedCommand = new Command<Workout>(OnWorkoutTapped);
            
            LoadOrCreatePlanAsync();
        }

        /// <summary>
        /// Gets or sets the currently active workout plan.
        /// This represents the plan that the user is currently viewing and managing.
        /// </summary>
        public WorkoutPlan? CurrentPlan
        {
            get => _currentPlan;
            set
            {
                _currentPlan = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the collection of workouts in the current plan.
        /// This observable collection automatically updates the UI when workouts are added or removed.
        /// Also triggers updates for HasWorkouts and HasNoWorkouts properties.
        /// </summary>
        public ObservableCollection<Workout> Workouts
        {
            get => _workouts;
            set
            {
                _workouts = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasWorkouts));
                OnPropertyChanged(nameof(HasNoWorkouts));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the ViewModel is currently loading data.
        /// Used to show loading indicators in the UI while plan and workout data is being fetched.
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
        /// Gets a value indicating whether the current plan has any workouts.
        /// Used for conditional UI display when workouts exist.
        /// </summary>
        public bool HasWorkouts => !IsLoading && Workouts?.Count > 0;
        
        /// <summary>
        /// Gets a value indicating whether the current plan has no workouts.
        /// Used for conditional UI display when no workouts exist (empty state).
        /// </summary>
        public bool HasNoWorkouts => !IsLoading && (Workouts?.Count == 0 || Workouts == null);

        /// <summary>
        /// Command to handle adding a new workout to the current plan.
        /// Prompts the user for a workout name and creates a new workout in the database.
        /// </summary>
        public ICommand AddWorkoutCommand { get; }
        
        /// <summary>
        /// Command to handle deleting a workout from the current plan.
        /// Shows a confirmation dialog before permanently removing the workout.
        /// </summary>
        public ICommand DeleteWorkoutCommand { get; }
        
        /// <summary>
        /// Command to handle navigation when a workout is tapped.
        /// Navigates to the WorkoutPage with the selected workout's ID.
        /// </summary>
        public ICommand WorkoutTappedCommand { get; }

        /// <summary>
        /// Asynchronously loads the active workout plan or creates a default one if none exists.
        /// First attempts to load the plan saved in user preferences, then falls back to the first
        /// available plan, and finally creates a new default plan if no plans exist.
        /// </summary>
        private async void LoadOrCreatePlanAsync()
        {
            IsLoading = true;
            try
            {
                int activePlanId = Preferences.Get("ActivePlanId", -1);
                WorkoutPlan? plan = null;

                if (activePlanId != -1)
                {
                    plan = _context.WorkoutPlans.FirstOrDefault(p => p.Id == activePlanId);
                }

                if (plan == null)
                {
                    plan = _context.WorkoutPlans.FirstOrDefault();
                }

                if (plan == null)
                {
                    plan = new WorkoutPlan { Name = "My First Plan" };
                    _context.WorkoutPlans.Add(plan);
                    await _context.SaveChangesAsync();
                    
                    // Set as active plan
                    Preferences.Set("ActivePlanId", plan.Id);
                }

                CurrentPlan = plan;

                // Load workouts for the current plan with their exercises
                var workoutsInPlan = _context.Workouts
                    .Include(w => w.Exercises)
                    .Where(w => w.WorkoutPlanId == CurrentPlan.Id)
                    .ToList();
                Workouts.Clear();
                foreach (var workout in workoutsInPlan)
                {
                    Workouts.Add(workout);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading plan: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Handles the add workout command by prompting the user for a workout name
        /// and creating a new workout in the current plan. Updates both the database
        /// and the UI collection upon successful creation.
        /// </summary>
        private async void OnAddWorkout()
        {
            if (CurrentPlan == null) return;
            if (Application.Current?.MainPage == null) return;

            // Prompt user for workout name
            string workoutName = await Application.Current.MainPage.DisplayPromptAsync(
                "New Workout", 
                "Enter a name for your workout:", 
                "Create", 
                "Cancel", 
                $"Workout {Workouts.Count + 1}",
                maxLength: 50);

            if (string.IsNullOrWhiteSpace(workoutName)) return;

            try
            {
                // Create a new workout
                var newWorkout = new Workout
                {
                    Title = workoutName.Trim(),
                    WorkoutPlanId = CurrentPlan.Id
                };

                _context.Workouts.Add(newWorkout);
                await _context.SaveChangesAsync();

                // Add to collection
                Workouts.Add(newWorkout);
                OnPropertyChanged(nameof(HasWorkouts));
                OnPropertyChanged(nameof(HasNoWorkouts));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding workout: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the delete workout command by showing a confirmation dialog
        /// and removing the specified workout from both the database and UI collection.
        /// </summary>
        /// <param name="workout">The workout to delete from the current plan.</param>
        private async void OnDeleteWorkout(Workout workout)
        {
            if (workout == null || Application.Current?.MainPage == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Delete Workout", 
                $"Are you sure you want to delete '{workout.Title}'?", 
                "Yes", "No");

            if (!confirm) return;

            try
            {
                var workoutToDelete = _context.Workouts.FirstOrDefault(w => w.Id == workout.Id);
                if (workoutToDelete != null)
                {
                    _context.Workouts.Remove(workoutToDelete);
                    await _context.SaveChangesAsync();

                    // Remove from collection
                    Workouts.Remove(workout);
                    OnPropertyChanged(nameof(HasWorkouts));
                    OnPropertyChanged(nameof(HasNoWorkouts));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting workout: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles navigation when a workout is tapped by navigating to the WorkoutPage
        /// with the selected workout's ID passed as a parameter.
        /// </summary>
        /// <param name="workout">The workout that was tapped for detailed viewing/editing.</param>
        private async void OnWorkoutTapped(Workout workout)
        {
            if (workout == null) return;

            try
            {
                // Navigate to WorkoutPage with the workout ID using Dictionary parameters
                await Shell.Current.GoToAsync($"//PlanPage/{nameof(WorkoutPage)}", new Dictionary<string, object>
                {
                    { "WorkoutId", workout.Id }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navigating to workout: {ex.Message}");
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
