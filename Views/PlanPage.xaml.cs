using Fitnes_ai.ViewModels;

namespace Fitnes_ai.Views;

public partial class PlanPage : ContentPage
{
	public PlanPage(PlanViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
