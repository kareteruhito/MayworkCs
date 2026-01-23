using System.Windows;
// WPF の基本クラス（Window / Application など）が定義されている名前空間

namespace _01FirstStep;
// このクラスが属する名前空間
// MainWindow.xaml の x:Class="_01FirstStep.MainWindow" と対応している

public partial class MainWindow : Window
// メインウィンドウを表すクラス
// Window クラスを継承することで「画面」として動作する
// partial は XAML 側で定義された内容と結合されることを意味する
{
    public MainWindow()
    {
        // コンストラクタ（ウィンドウ生成時に最初に呼ばれる）

        InitializeComponent();
        // MainWindow.xaml に記述された XAML を読み込み、
        // コントロールを生成して画面に配置する自動生成メソッド
        //
        // 内部的には以下のような処理が行われている：
        //
        //  - XAML の解析
        //  - Button / Grid などのインスタンス生成
        //  - Name 指定されたコントロールのフィールド生成
        //  - イベントハンドラの接続
        //
        // この呼び出しを削除すると、画面は空の Window になります。
    }
}
