using System;
using System.Windows.Data;
using System.Windows.Media;

namespace LangSharper.Resources
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class IntToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Brush))
                throw new InvalidOperationException("The target must be a brush");

            if ((int) value == (int) ExtendedWord.AnswerType.Correct)
                return Brushes.DarkSeaGreen;

            if ((int) value == (int)ExtendedWord.AnswerType.Incorrect)
                return Brushes.IndianRed;

            return Brushes.WhiteSmoke;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
