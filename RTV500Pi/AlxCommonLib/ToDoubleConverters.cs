using System;
using Windows.UI.Xaml.Data;

namespace AlxCommonLib
{
    public class UIntToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) { return System.Convert.ToDouble(value); }
        public object ConvertBack(object value, Type targetType, object parameter, string language) { return System.Convert.ToUInt32(value); }
    }
    public class UShortToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) { return System.Convert.ToDouble(value); }
        public object ConvertBack(object value, Type targetType, object parameter, string language) { return System.Convert.ToUInt16(value); }
    }
    public class ByteToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) { return System.Convert.ToDouble(value); }
        public object ConvertBack(object value, Type targetType, object parameter, string language) { return System.Convert.ToByte(value); }
    }
}
