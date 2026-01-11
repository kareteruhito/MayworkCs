using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using BookMarker.ViewModels;

namespace BookMarker.Views;

public partial class CategoryUpdateDialog : Window
{
    readonly CategoryUpdateViewModel _vm;
    public string CategoryName { get => _vm.CategoryName; }
    public CategoryUpdateDialog(string oldCategory)
    {
        InitializeComponent();

        _vm = new CategoryUpdateViewModel(oldCategory);
        DataContext = _vm;

        Loaded += (_, __) =>
        {
            NameTextBox.Focus();
            NameTextBox.SelectAll();
        };
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        var name = (_vm.CategoryName ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show(this, "カテゴリ名を入力してください。", "入力エラー",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 返却値の確定（呼び出し側は DialogResult==true を見て CategoryName を読む）
        DialogResult = true; // これでウィンドウは自動的に閉じる
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}

