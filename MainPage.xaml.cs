namespace Fitnes_ai
{
    /// <summary>
    /// Represents the main page of the application.
    /// This class contains the logic for the main page, which may be part of the default
    /// .NET MAUI template.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        int count = 0;

        /// <summary>
        /// Initializes a new instance of the MainPage class.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the click event of the counter button.
        /// Increments a counter and updates the button text to display the current count.
        /// This is often included as a sample implementation in the default template.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}
