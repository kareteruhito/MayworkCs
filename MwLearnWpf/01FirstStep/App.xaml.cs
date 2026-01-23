using System.Windows;
// WPF の基本クラス（Application / Window など）が定義されている名前空間

namespace _01FirstStep;
// このアプリケーション全体の名前空間
// App.xaml の x:Class="_01FirstStep.App" と対応している

public partial class App : Application
// アプリケーション本体を表すクラス
// Application クラスを継承することで、WPF アプリとして動作する
// partial は App.xaml 側で定義された内容と結合されることを意味する
{
    // このクラスには、アプリ起動・終了などのイベント処理を記述できる
    //
    // 例:
    //  - OnStartup()   : アプリ起動時
    //  - OnExit()      : アプリ終了時
    //  - DispatcherUnhandledException : 例外の一括捕捉
    //
    // 今回は特別な処理が無いため空クラスになっている    
}

