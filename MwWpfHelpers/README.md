# MwWpfHelpers

MwWpfHelpers は、**WPF 用 Helper / Utility の動作確認およびデモ**を目的としたプロジェクトです。

このプロジェクトは **ライブラリ配布を目的としていません**。  
各 Helper は、必要に応じて **`.cs` ファイルをコピー＆ペーストして利用する**ことを想定しています。

---

## このプロジェクトの目的

- WPF コントロール拡張用 Helper の **実装例・動作確認**
- Canvas / Image / マウス操作など、**UI イベント前提の処理を試す場**
- 実アプリで使いながら Helper をブラッシュアップし、  
  改良内容をこのプロジェクトへ還元する

いわば **「育てる Helper 置き場」**です。

---

## 基本方針

- XAML は **レイアウト定義のみ**
- イベントの紐づけ・処理は **コードビハインド側で完結**
- ViewModel や MVVM は使用しない
- **static Helper + Attach パターン**を基本とする
- DLL 参照・NuGet 配布は行わない

---

## ディレクトリ構成

```text
MwWpfHelpers
├─ Helpers/
│   ├─ CanvasZoomHelper.cs
│   ├─ ImageDropHelper.cs
│   ├─ SplineDrawHelper.cs
│   └─ （WPF コントロール拡張系 Helper）
│
├─ Utilities/
│   └─ （WPF 依存の小さな補助クラスが入る場合あり）
│
├─ MainWindow.xaml
├─ MainWindow.xaml.cs
├─ App.xaml
└─ README.md
