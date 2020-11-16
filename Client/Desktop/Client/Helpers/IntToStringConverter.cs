using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace ClientModule.Helpers
{
    class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ( ((int)value) == -1)
                return null;
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int Value;
            try
            {
                Value = Int32.Parse((string)value);
            }
            catch(Exception exc)
            {
                return -1;
            }
            return Value;
        }
    }

}
