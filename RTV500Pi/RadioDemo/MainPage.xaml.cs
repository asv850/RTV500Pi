using RadioLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RadioDemo
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Radio _radio = null;
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _radio = new Radio();
            Radio.AddParisRadios(_radio, WaveBand.wbUKW);
            Radio.AddAddictRadios(_radio, WaveBand.wbKW);
            Radio.AddFrenchLWRadios(_radio);
            await _radio.Initialize(new string[4] { "74934_radio-static-short-wave-choppy.wav", "74260_am-radio-noise.wav", "316808_am-static-1.wav", "pink-noise.wav" });
            RadioViewModel = new RadioVM(_radio);

            WaveBandSelector selector = new WaveBandSelector();
            selector.OnSelectorChanged += Selector_OnSelectorChanged;
            SelectorViewModel = new WaveBandSelectorVM(selector);

            AudioPlayerCTRL.LoadFiles(await KnownFolders.MusicLibrary.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName));

            DataContext = this;
        }
        private void Selector_OnSelectorChanged(WaveBand newValue) { _radio.Band = newValue; }
        private void ToggleButton_Checked(object sender, RoutedEventArgs e) { AudioPlayerCTRL.Play(); }
        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e) { AudioPlayerCTRL.Pause(); }

        public RadioVM RadioViewModel { get; private set; }
        public WaveBandSelectorVM SelectorViewModel { get; private set; }
        public MainPage() { this.InitializeComponent(); }
    }
}
