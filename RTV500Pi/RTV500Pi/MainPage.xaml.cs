using RadioLib;
using RTV500PiLib.Hardware.Gpio.ACE128.TuneButton;
using RTV500PiLib.Hardware.Gpio.Lights;
using RTV500PiLib.Hardware.Gpio.Selectors;
using RTV500PiLib.Hardware.I2c.MCP4725.Galvanometers.TuneGalvanometers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RTV500Pi
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

            TuneGalvanometer tuneGalva = new TuneGalvanometer(0x62);
            try
            {
                await tuneGalva.Initialize();
            }
            catch (Exception)
            {
                TuneGalvaErrTBX.Visibility = Visibility.Visible;
            }
            tuneGalva.Enabled = true;
            _radio.Galva = tuneGalva;

            _radio.StereoLight = new Light(18);

            TuneButton tuneButton = new TuneButton(new byte[9] { 6, 12, 16, 20, 21, 26, 19, 13, 5 }, 75, 200);
            TuneButton_OnValueChanged(tuneButton.Value);
            tuneButton.OnValueChanged += TuneButton_OnValueChanged;

            AudioPlayerCTRL.LoadFiles(await KnownFolders.MusicLibrary.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName));

            RTV500FrontSelector frontSelector = new RTV500FrontSelector(new byte[6] { 4, 17, 27, 22, 23, 24 });
            frontSelector_OnButtonsChanged(frontSelector.PressedButtons);
            frontSelector.OnButtonsChanged += frontSelector_OnButtonsChanged;

            DataContext = this;
        }
        private void frontSelector_OnButtonsChanged(RTV500FrontSelectorButtons newValue)
        {
            if ((newValue & RTV500FrontSelectorButtons.fsbLW) != 0)
            {
                AudioPlayerCTRL.Pause();
                _radio.Band = WaveBand.wbLW;
            }
            else if ((newValue & RTV500FrontSelectorButtons.fsbMW) != 0)
            {
                AudioPlayerCTRL.Pause();
                _radio.Band = WaveBand.wbMW;
            }
            else if ((newValue & RTV500FrontSelectorButtons.fsbKW) != 0)
            {
                AudioPlayerCTRL.Pause();
                _radio.Band = WaveBand.wbKW;
            }
            else if ((newValue & RTV500FrontSelectorButtons.fsbUKW) != 0)
            {
                AudioPlayerCTRL.Pause();
                _radio.Band = WaveBand.wbUKW;
            }
            else
            {
                _radio.Band = WaveBand.wbNone;
                if ((newValue & RTV500FrontSelectorButtons.fsbTATB) != 0)
                    AudioPlayerCTRL.Play();
                else
                    AudioPlayerCTRL.Pause();
            }
        }
        private void TuneButton_OnValueChanged(byte newValue) { _radio.TunePosition = newValue; }

        public RadioVM RadioViewModel       { get; private set; }
        public MainPage()                   { this.InitializeComponent(); }        
    }
}
