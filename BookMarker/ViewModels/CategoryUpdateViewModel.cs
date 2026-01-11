using BookMarker.Helpers;

namespace BookMarker.ViewModels;

public class CategoryUpdateViewModel : ViewModelBase
{
    private string _oldCategory;
    public string OldCategory
    {
        get => _oldCategory;
    }
    private string _categoryName = "";
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
    public CategoryUpdateViewModel(string oldCategory)
    {
        _oldCategory = oldCategory;
    }

}
