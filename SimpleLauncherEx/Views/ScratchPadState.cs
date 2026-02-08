
using System.Collections.ObjectModel;

using Maywork.WPF.Helpers;

namespace SimpleLauncherEx.Views;

public class ScratchPadState : ViewModelBase
{
    public ObservableCollection<ScratchPadItem> Items = [];

    public ScratchPadItem? SelectedItem {get; set;}
    
}