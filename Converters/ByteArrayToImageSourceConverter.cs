using System.Globalization;

namespace Fitnes_ai.Converters
{
    /// <summary>
    /// XAML value converter that converts byte arrays to ImageSource objects.
    /// Used in data binding to display images stored as byte arrays in the database.
    /// Commonly used for exercise images that are stored as binary data.
    /// </summary>
    public class ByteArrayToImageSourceConverter : IValueConverter
    {
        /// <summary>
        /// Converts a byte array to an ImageSource for display in XAML Image controls.
        /// </summary>
        /// <param name="value">The byte array containing image data</param>
        /// <param name="targetType">The target type (ImageSource)</param>
        /// <param name="parameter">Optional parameter (not used)</param>
        /// <param name="culture">The culture info (not used)</param>
        /// <returns>
        /// An ImageSource created from the byte array, or null if the input is invalid.
        /// Returns null for null/empty byte arrays to prevent binding errors.
        /// </returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is byte[] imageBytes && imageBytes.Length > 0)
            {
                return ImageSource.FromStream(() => new MemoryStream(imageBytes));
            }
            
            // Return null for invalid/empty data to prevent binding errors
            return null;
        }

        /// <summary>
        /// Converts back from ImageSource to byte array.
        /// This operation is not supported as it's typically not needed for UI binding.
        /// </summary>
        /// <param name="value">The ImageSource value</param>
        /// <param name="targetType">The target type (byte[])</param>
        /// <param name="parameter">Optional parameter (not used)</param>
        /// <param name="culture">The culture info (not used)</param>
        /// <returns>Always throws NotImplementedException</returns>
        /// <exception cref="NotImplementedException">This operation is not supported</exception>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Converting from ImageSource to byte array is not supported.");
        }
    }
}
