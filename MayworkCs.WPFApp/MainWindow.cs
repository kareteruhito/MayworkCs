// メインウィンドウ
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using MayworkCs.WPFLib;
using static MayworkCs.WPFLib.UI;

namespace MayworkCs.WPFApp;

public sealed class MainWindow : Window
{
    public MainWindow()
    {
        var tabs = new TabControl();

        this.Init("Simple NoXAML Samples").Content(tabs);

        // ボタンのサンプル
        var tab1 = Tab("Button",
            Col(
                Btn("Hello", () => MessageBox.Show("Clicked!"))
            )
        ).AddTo(tabs);

        // スライダーのサンプル
        var sliderValue = Lbl("50%");
        var sliderBar = Bar();
        var tab2 = Tab("Slider",
            Col(
                Row(Lbl("Value:"), sliderValue),
                sliderBar,
                // 0～100 のスライダー。目盛 10、スナップ有り。
                Sld(0, 100, 50, v => {
                    sliderValue.Text = $"{(int)v}%";
                    sliderBar.Width = v * 3; // 値に応じてバーの長さを変える（簡易プレビュー）
                }, tick:10, snap:true)                
            )
        ).AddTo(tabs);

        // グリッドのサンプル
        var grid = Grd("*", "*,Auto");
        var tab3 = Tab("Grid", grid).AddTo(tabs);
        // 左
        var left = Col(
            Btn("Open", () =>
            {
                var p = Dialogs.Open();
                if (p is not null) this.Title = p;
            }),
            Btn("Save", () => { Dialogs.SaveAs("untitled.txt"); })
        ).PlaceIn(grid, 0, 0);
        // 右
        var righ = Row(
            Btn("Prompt", () =>
            {
                var p = Dialogs.Prompt("タイトル", "メッセージ", "デフォルト");
                if (p is not null) this.Title = p;
            })
        ).PlaceIn(grid, 0, 1);

        // メニューバーとステータスバーのサンプル
        var statusText = Lbl();
        var panel = new DockPanel();
        var tab4 = Tab("Menu&StatusBar", panel).AddTo(tabs);
        // メニュー
        MAddToTop(panel,
            MiRoot("_File",
                MItem(this, "_Open", Key.O, ModifierKeys.Control, () => { statusText.Text = "Open selected"; }),
                MItem(this, "_Save", Key.S, ModifierKeys.Control, () => { statusText.Text = "Save selected"; }),
                MSep(),
                MItem("E_xit", Close)
            ),
            MiRoot("_Help",
                MItem("_About", () => { statusText.Text = "About selected"; })
            )
        );
        // ステータスバー
        // ここで生成して追加、TextBlockを保持
        var (_, st) = SBAddToBottom(panel, "Ready");
        statusText = st;
        // ダミーテキストブロック
        var tb = Lbl("ダミー").AddTo(panel);


        // イメージのサンプル
        var img = Img();
        var tab5 = Tab("Image", img).AddTo(tabs);

        // D&D
        AllowDrop = true;
        // 画像だけ受け付けて最初の1枚を表示
        Wiring.AcceptFiles(this, files =>
            {
                img.Source = BmpSrc.FromFile(files[0]);
            },
            ".png",".jpg",".jpeg",".bmp",".gif",".webp");
        
        // Ctrl+C = Copy
        Wiring.Hotkey(this, Key.C, ModifierKeys.Control,
            () =>
            {
                if (img.Source is BitmapSource bmp)
                    Clipb.SetImageFromSourceOrThrow(img.Source);
            },
            () => img.Source is not null);
    }
}