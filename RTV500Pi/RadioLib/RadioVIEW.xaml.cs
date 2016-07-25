using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace RadioLib
{
    public class WaveBandToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            WaveBand b = (WaveBand)value;
            return (int)b;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            int b = (int)value;
            return (WaveBand)b;
        }
    }
    public class FrequencyToCurseurTranslationXConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)        { return ((byte)value-3) * 1320 / 194; }
        public object ConvertBack(object value, Type targetType, object parameter, string language)    { throw new NotImplementedException(); }
    }
    public class FrequencyToGridMarginConverter : DependencyObject, IValueConverter
    {
        public UserControl CurrentControl
        {
            get { return (UserControl)GetValue(CurrentControlProperty); }
            set { SetValue(CurrentControlProperty, value); }
        }
        public static readonly DependencyProperty CurrentControlProperty = DependencyProperty.Register("CurrentControl", typeof(UserControl), typeof(FrequencyToGridMarginConverter), new PropertyMetadata(null));

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Thickness res = new Thickness(0);

            double Trx = ((byte)value - 3) * 1360 / 194;
            if (CurrentControl.ActualWidth < 1360)
            { 
                double Grx = -(Trx - CurrentControl.ActualWidth / 2);
                res.Left = Math.Max(Math.Min(Grx, 0), -(1360-CurrentControl.ActualWidth));
            }
            return res;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)    { throw new NotImplementedException(); }
    }

    public sealed partial class RadioVIEW : UserControl
    {     
        public RadioVIEW()        { this.InitializeComponent(); }

        private void ControlUCTRL_SizeChanged(object sender, SizeChangedEventArgs e)        { ((RadioVM)DataContext)?.UpdateTunePosition(); }
    }
}
