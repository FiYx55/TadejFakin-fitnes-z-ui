using Fitnes_ai.ViewModels;

namespace Fitnes_ai.Views;

public partial class ExercisesPage : ContentPage
{
	public ExercisesPage(ExercisesViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
