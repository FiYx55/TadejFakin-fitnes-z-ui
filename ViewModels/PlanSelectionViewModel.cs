using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Fitnes_ai.Models;
using Microsoft.EntityFrameworkCore;

namespace Fitnes_ai.ViewModels
{
    /// <summary>
    /// ViewModel for the Plan Selection page that manages workout plan CRUD operations and active plan selection.
    /// Handles creating, deleting, and selecting workout plans, with integration to local preferences
    /// for persisting the active plan across app sessions.
    /// </summary>
    public class PlanSelectionViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _context;
        private ObservableCollection<WorkoutPlan> _plans;
        private WorkoutPlan? _activePlan;
        private bool _isLoading;

        /// <summary>
        /// Initializes a new instance of the PlanSelectionViewModel class.
        /// Sets up command bindings and triggers initial loading of plans and active plan.
        /// </summary>
        /// <param name="context">The database context for workout plan operations.</param>
        public PlanSelectionViewModel(AppDbContext context)
        {
            _context = context;
            _plans = new ObservableCollection<WorkoutPlan>();
            
            CreatePlanCommand = new Command(async () => await OnCreatePlan());
            DeletePlanCommand = new Command<WorkoutPlan>(async (plan) => await OnDeletePlan(plan));
            SelectPlanCommand = new Command<WorkoutPlan>(async (plan) => await OnSelectPlan(plan));
            
            LoadPlansAndActivePlan();
        }

        /// <summary>
        /// Gets or sets the collection of all available workout plans.
        /// Updated when plans are created, deleted, or loaded from the database.
        /// </summary>
        public ObservableCollection<WorkoutPlan> Plans
        {
            get => _plans;
            set
            {
                _plans = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasPlans));
                OnPropertyChanged(nameof(HasNoPlans));
            }
        }

        /// <summary>
        /// Gets or sets the currently active workout plan.
        /// This plan is used throughout the app and persisted in user preferences.
        /// Setting this property triggers UI updates for active plan indicators.
        /// </summary>
        public WorkoutPlan? ActivePlan
        {
            get => _activePlan;
            set
            {
                _activePlan = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ActivePlanName));
                
                // Trigger UI refresh for active plan indicator
                OnPropertyChanged(nameof(Plans));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the ViewModel is currently loading data.
        /// Used to show loading indicators while plan data is being fetched from database.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasPlans));
                OnPropertyChanged(nameof(HasNoPlans));
            }
        }

        /// <summary>
        /// Gets a value indicating whether there are workout plans available.
        /// Used for conditional UI display when plans exist.
        /// </summary>
        public bool HasPlans => !IsLoading && Plans?.Count > 0;
        
        /// <summary>
        /// Gets a value indicating whether there are no workout plans available.
        /// Used for conditional UI display when showing empty state.
        /// </summary>
        public bool HasNoPlans => !IsLoading && (Plans?.Count == 0 || Plans == null);
        
        /// <summary>
        /// Gets the name of the active workout plan for display purposes.
        /// Returns "No active plan" if no plan is currently active.
        /// </summary>
        public string ActivePlanName => ActivePlan?.Name ?? "No active plan";

        /// <summary>
        /// Command to handle creating a new workout plan.
        /// Prompts user for plan name and creates the plan in the database.
        /// </summary>
        public ICommand CreatePlanCommand { get; }
        
        /// <summary>
        /// Command to handle deleting a workout plan.
        /// Shows confirmation dialog and removes plan with all associated data.
        /// </summary>
        public ICommand DeletePlanCommand { get; }
        
        /// <summary>
        /// Command to handle selecting an active workout plan.
        /// Sets the plan as active and saves to user preferences.
        /// </summary>
        public ICommand SelectPlanCommand { get; }

        /// <summary>
        /// Helper method to determine if a specific plan is the currently active plan.
        /// Used for UI styling and active plan indicators.
        /// </summary>
        /// <param name="plan">The plan to check for active status.</param>
        /// <returns>True if the plan is active, false otherwise.</returns>
        public bool IsPlanActive(WorkoutPlan plan)
        {
            return ActivePlan?.Id == plan?.Id;
        }

        /// <summary>
        /// Asynchronously loads all workout plans from the database and determines the active plan.
        /// Retrieves active plan preference from local storage and matches it with available plans.
        /// </summary>
        private async void LoadPlansAndActivePlan()
        {
            IsLoading = true;
            try
            {
                // Load all plans
                var plans = await _context.WorkoutPlans.ToListAsync();
                Plans.Clear();
                
                foreach (var plan in plans)
                {
                    Plans.Add(plan);
                }

                // Load active plan from preferences
                var activePlanId = Preferences.Get("ActivePlanId", -1);
                if (activePlanId != -1)
                {
                    ActivePlan = Plans.FirstOrDefault(p => p.Id == activePlanId);
                }

                System.Diagnostics.Debug.WriteLine($"Loaded {Plans.Count} plans, active plan: {ActivePlan?.Name ?? "None"}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading plans: {ex.Message}");
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to load workout plans", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Handles the create plan command by prompting user for plan name
        /// and creating a new workout plan in the database.
        /// Automatically sets the new plan as active if it's the first plan created.
        /// </summary>
        private async Task OnCreatePlan()
        {
            if (Application.Current?.MainPage == null) return;

            try
            {
                string planName = await Application.Current.MainPage.DisplayPromptAsync(
                    "Create Plan",
                    "Enter a name for your new workout plan:",
                    "Create",
                    "Cancel",
                    "My Workout Plan",
                    keyboard: Keyboard.Text);

                if (string.IsNullOrWhiteSpace(planName)) return;

                var newPlan = new WorkoutPlan
                {
                    Name = planName.Trim()
                };

                _context.WorkoutPlans.Add(newPlan);
                await _context.SaveChangesAsync();

                Plans.Add(newPlan);

                // If this is the first plan, make it active
                if (ActivePlan == null)
                {
                    await OnSelectPlan(newPlan);
                }

                await Application.Current.MainPage.DisplayAlert(
                    "Success",
                    $"Workout plan '{planName}' created successfully!",
                    "OK");

                System.Diagnostics.Debug.WriteLine($"Created new plan: {planName} with ID: {newPlan.Id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating plan: {ex.Message}");
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to create workout plan", "OK");
            }
        }

        /// <summary>
        /// Handles the delete plan command by showing a confirmation dialog and removing
        /// the specified plan from the database. All associated workouts and exercises
        /// are also deleted due to cascade delete behavior.
        /// </summary>
        /// <param name="plan">The workout plan to delete.</param>
        private async Task OnDeletePlan(WorkoutPlan plan)
        {
            if (plan == null || Application.Current?.MainPage == null) return;

            try
            {
                bool confirm = await Application.Current.MainPage.DisplayAlert(
                    "Delete Plan",
                    $"Are you sure you want to delete '{plan.Name}'? This will also delete all workouts and exercises in this plan.",
                    "Yes, Delete",
                    "Cancel");

                if (!confirm) return;

                // If this is the active plan, clear it
                if (ActivePlan?.Id == plan.Id)
                {
                    ActivePlan = null;
                    Preferences.Remove("ActivePlanId");
                }

                _context.WorkoutPlans.Remove(plan);
                await _context.SaveChangesAsync();

                Plans.Remove(plan);

                await Application.Current.MainPage.DisplayAlert(
                    "Success",
                    $"Workout plan '{plan.Name}' deleted successfully!",
                    "OK");

                System.Diagnostics.Debug.WriteLine($"Deleted plan: {plan.Name}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting plan: {ex.Message}");
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete workout plan", "OK");
            }
        }

        /// <summary>
        /// Handles the select plan command by setting the specified plan as the active plan
        /// and saving its ID to user preferences for persistence.
        /// </summary>
        /// <param name="plan">The workout plan to set as active.</param>
        private async Task OnSelectPlan(WorkoutPlan plan)
        {
            if (plan == null) return;

            try
            {
                ActivePlan = plan;
                Preferences.Set("ActivePlanId", plan.Id);

                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Active Plan Set",
                        $"'{plan.Name}' is now your active workout plan!",
                        "OK");
                }

                System.Diagnostics.Debug.WriteLine($"Set active plan to: {plan.Name} (ID: {plan.Id})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting active plan: {ex.Message}");
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to set active plan", "OK");
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
