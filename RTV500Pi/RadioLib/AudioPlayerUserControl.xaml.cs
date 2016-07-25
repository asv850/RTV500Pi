using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace RadioLib
{
    public sealed partial class AudioPlayerUserControl : UserControl, INotifyPropertyChanged
    {
        private MediaPlaybackList _playbackList = new MediaPlaybackList();
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)   { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)               { DataContext = this; }
        private async void _playbackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => 
            {
                OnPropertyChanged("CurrentFileName");
                OnPropertyChanged("CurrentFileNameIndex");
            });
        }

        public string CurrentFileName
        {
            get
            {
                if (_playbackList.CurrentItem != null)
                    return _playbackList.CurrentItem.Source.CustomProperties["FileName"].ToString();
                else
                    return "";
            }
        } 
        public int FileNameCount                { get { return _playbackList.Items.Count; } }
        public uint CurrentFileNameIndex        { get { return _playbackList.CurrentItemIndex; } }
        public AudioPlayerUserControl()
        {
            this.InitializeComponent();
            _playbackList.AutoRepeatEnabled = true;
            _playbackList.CurrentItemChanged += _playbackList_CurrentItemChanged;
            MediaElementMED.SetPlaybackSource(_playbackList);
        }
        public void LoadFiles(IReadOnlyList<StorageFile> files)
        {
            _playbackList.Items.Clear();
            foreach(StorageFile file in files)
            {
                MediaSource s = MediaSource.CreateFromStorageFile(file);
                s.CustomProperties["FileName"] = file.Name;

                _playbackList.Items.Add(new MediaPlaybackItem(s));
            }
            OnPropertyChanged("FileNameCount");
        }
        public async void Play()                { await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { MediaElementMED.Play(); }); }
        public async void Pause()               { await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { MediaElementMED.Pause(); }); }

        public event PropertyChangedEventHandler PropertyChanged;        
    }
}
