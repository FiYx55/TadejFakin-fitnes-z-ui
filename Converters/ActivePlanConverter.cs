using System.Globalization;

namespace Fitnes_ai.Converters
{
    /// <summary>
    /// XAML multi-value converter that determines if a workout plan is the currently active plan.
    /// Used to show/hide UI elements based on whether a plan matches the active plan.
    /// Compares two integer IDs to determine if they represent the same plan.
    /// </summary>
    public class ActivePlanConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts two plan ID values to a boolean indicating if they represent the same (active) plan.
        /// </summary>
        /// <param name="values">
        /// Array containing two integer values:
        /// [0] = The current plan's ID
        /// [1] = The active plan's ID
        /// </param>
        /// <param name="targetType">The target type (bool)</param>
        /// <param name="parameter">Optional parameter (not used)</param>
        /// <param name="culture">The culture info (not used)</param>
        /// <returns>
        /// True if both IDs are integers and equal (indicating this is the active plan).
        /// False if the IDs don't match or if the input is invalid.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is int planId && values[1] is int activePlanId)
            {
                return planId == activePlanId;
            }
            return false; // Default to false for invalid input
        }

        /// <summary>
        /// Converts back from boolean to plan IDs.
        /// This operation is not supported as it would require generating ID values.
        /// </summary>
        /// <param name="value">The boolean value</param>
        /// <param name="targetTypes">The target types (int[])</param>
        /// <param name="parameter">Optional parameter (not used)</param>
        /// <param name="culture">The culture info (not used)</param>
        /// <returns>Always throws NotImplementedException</returns>
        /// <exception cref="NotImplementedException">This operation is not supported</exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Converting from boolean to plan IDs is not supported.");
        }
    }
}
