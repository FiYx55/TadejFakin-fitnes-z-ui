using Fitnes_ai.ViewModels;

namespace Fitnes_ai.Views;

public partial class WorkoutPage : ContentPage
{
	public WorkoutPage(WorkoutViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
