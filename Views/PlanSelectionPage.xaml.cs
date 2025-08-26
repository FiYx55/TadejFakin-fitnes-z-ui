using Fitnes_ai.ViewModels;

namespace Fitnes_ai.Views;

public partial class PlanSelectionPage : ContentPage
{
	public PlanSelectionPage(PlanSelectionViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
