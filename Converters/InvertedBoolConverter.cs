using System.Globalization;

namespace Fitnes_ai.Converters
{
    /// <summary>
    /// XAML value converter that inverts boolean values.
    /// Useful for binding scenarios where the UI needs the opposite of a property's boolean value.
    /// For example, showing elements when IsLoading is false, or hiding elements when HasItems is true.
    /// </summary>
    public class InvertedBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to its inverse.
        /// </summary>
        /// <param name="value">The boolean value to invert</param>
        /// <param name="targetType">The target type (bool)</param>
        /// <param name="parameter">Optional parameter (not used)</param>
        /// <param name="culture">The culture info (not used)</param>
        /// <returns>
        /// The inverted boolean value: true becomes false, false becomes true.
        /// Returns true for non-boolean inputs as a default behavior.
        /// </returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true; // Default to true for non-boolean values
        }

        /// <summary>
        /// Converts back from inverted boolean to original boolean.
        /// Simply inverts the value again to restore the original state.
        /// </summary>
        /// <param name="value">The inverted boolean value</param>
        /// <param name="targetType">The target type (bool)</param>
        /// <param name="parameter">Optional parameter (not used)</param>
        /// <param name="culture">The culture info (not used)</param>
        /// <returns>
        /// The re-inverted boolean value (original value).
        /// Returns false for non-boolean inputs.
        /// </returns>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false; // Default to false for non-boolean values
        }
    }
}
