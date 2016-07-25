using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace RadioLib
{
    public sealed partial class RadioStationVIEW : UserControl
    {
        private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TextBlock tb = (TextBlock)sender;
            RadioStationVM stationRadioVM = (RadioStationVM)tb.DataContext;
            if (stationRadioVM != null)
            {
                double le = ((stationRadioVM.Frequency - 2) * 1320 / 194) - (e.NewSize.Width / 2);
                tb.Margin = new Thickness(le, stationRadioVM.DisplayRow * e.NewSize.Height, 0, 0);
            }
        }

        public RadioStationVIEW()        { this.InitializeComponent(); }
    }
}
