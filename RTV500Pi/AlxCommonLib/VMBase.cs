using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace AlxCommonLib
{
    public class VMBase : INotifyPropertyChanged
    {
        protected readonly static CoreDispatcher _dispatcher = Window.Current.Dispatcher;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
