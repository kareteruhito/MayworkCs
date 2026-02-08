
using System.Collections.ObjectModel;

using Maywork.WPF.Helpers;

namespace SimpleLauncherEx.Views;

public class BookMakerState : ViewModelBase
{
    public ObservableCollection<BookMakerItem> Items = [];    
}