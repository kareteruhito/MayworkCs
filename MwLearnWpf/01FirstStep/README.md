# 1.はじめの一歩
---

## 内容

本章では、WPF アプリ開発の最初の一歩として、以下の内容を扱います。

- WPF アプリのプロジェクト作成  
- スケルトンコードの解説  
- プロジェクトの実行方法  
- プロジェクトのデプロイ方法  
- NuGet パッケージの追加  


---
## 必要ツールのインストール

ブログ記事参照
[.NET 10 SDKとVSCodeで作る軽量C#開発環境](https://maywork.net/computer/net10sdk-vscode-install/)
---

## WPFアプリのプロジェクトの作成

PowerShellで以下のコマンドを実行

```
cd (mkdir プロジェクト名)
dotnet new wpf -f net10.0
```
---

## スケルトンコードの解説

### 01FirstStep.csproj

プロジェクトの設定を記述するファイルです。
XML 形式で記述され、ファイル名は以下の形式になります。
```
プロジェクト名.csproj
```
ターゲットフレームワークや使用ライブラリなどが定義されています。

このプロジェクトはディレクトリ名が数値で始まる関係で、先頭に_が付与されています。
アルファベット始まりのディレクトリ名の場合_は付与されません。

### App.xaml
WPF アプリ全体の起点となる設定（起動・リソース・イベント）を定義するファイルです。

- 起動時のウィンドウ指定
- アプリ共通リソースの定義
- アプリケーションイベントの設定

などを担当します。

### App.xaml.cs
WPF アプリ起動時や終了時などアプリ全体の処理をコードで制御するクラスです。

- アプリ起動時の処理
- アプリ終了時の処理
- 例外処理の集中管理

などを記述できます。

### MainWindow.xaml
アプリ起動時に表示されるメイン画面のレイアウトとUI構造を定義するファイルです。

- 画面レイアウト
- UI 構造
- コントロール配置

を XAML で定義します。

### MainWindow.xaml.cs
MainWindow のイベント処理や画面ロジックを記述するコードビハインドです。

- ボタン押下などのイベント処理
- 画面制御ロジック

を記述します。

### 起動の流れ
```
App 起動
  ↓
MainWindow コンストラクタ実行
  ↓
InitializeComponent()
  ↓
XAML 読み込み
  ↓
Grid / Button / TextBox 生成(あれば)
  ↓
画面表示

```


---

## プロジェクトの実行

ビルド→実行の流れを自動で行うコマンドです。
```
dotnet run
```

---

## プロジェクトのデプロイ

### リリース版でビルドする
```
dotnet build -c Release -o 出力先のディレクトリ
```
開発環境向けの実行ファイルを作成します。

---

### .NET ランタイム同梱（Self-contained）デプロイ。
```
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o 出力先のディレクトリ
```

特徴：

- .NET 未インストール環境でも実行可能

- 出力先ディレクトリを丸ごとコピーするだけで動作

- ZIP 配布などに便利


---

## NuGet パッケージの追加

OpenCVSharpのパッケージを追加

```
dotnet add package OpenCvSharp4
dotnet add package OpenCvSharp4.runtime.win
```

実行後csprojに以下の設定が追加されます。
```
<ItemGroup>
  <PackageReference Include="OpenCvSharp4" Version="4.11.0.20250507" />
  <PackageReference Include="OpenCvSharp4.runtime.win" Version="4.11.0.20250507" />
</ItemGroup>
```