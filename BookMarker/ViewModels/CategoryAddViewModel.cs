using BookMarker.Helpers;

namespace BookMarker.ViewModels;

public class CategoryAddViewModel : ViewModelBase
{
    private string _categoryName;
    public string CategoryName
    {
        get => _categoryName;
        set
        {
            if (_categoryName == value) return;
            _categoryName = value;
            OnPropertyChanged();
        }
    }
    public CategoryAddViewModel(string? initialName)
    {
        _categoryName = initialName ?? "";
    }

}
