using Maywork.WPF.Helpers;
using Cv=OpenCvSharp;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenCvFilterMaker2;
public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
{
	#region
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    private CompositeDisposable Disposable { get; } = [];
	public void Dispose() => Disposable.Dispose();

    #endregion

    // フィールド
    

	// キャンセルトークン
	private CancellationTokenSource? _cts;

    // プロパティ
    private ReactivePropertySlim<Cv.Mat?> CurrentMat { get; set; }

    public ReactivePropertySlim<bool> IsProcessing { get; private set; }
	public ReactivePropertySlim<string> Title { get; private set; }
	public ReactivePropertySlim<BitmapSource?> ImagePreview { get; private set; }
    public ObservableCollection<CvFilterBase> FilterMenus { get; private set; }
    public ObservableCollection<CvFilterBase> Filters { get; private set; }
    public ReactivePropertySlim<CvFilterBase?> SelectedFilter { get; private set; }
    public ReactivePropertySlim<bool> IsAutoSaveEnabled { get; private set; }

    // コマンド
    public AsyncReactiveCommand<string[]> FileDropCommand { get; }
    public ReactiveCommandSlim LoadPipelineCommand { get; }
    public ReactiveCommandSlim SavePipelineCommand { get; }
    public ReactiveCommandSlim RemoveFilterCommand { get; }
    public ReactiveCommandSlim MoveUpCommand { get; }
    public ReactiveCommandSlim MoveDownCommand { get; }
    public ReactiveCommandSlim ExecuteFiltersCommand { get; }
    public ReactiveCommandSlim CancelCommand { get; }
    public ReactiveCommandSlim ResetFiltersCommand { get; }
    public ReactiveCommandSlim ClearFiltersCommand { get; }
    public ReactiveCommandSlim CopyResultToClipboardCommand { get; }
    public ReactiveCommandSlim OpenFilteredFolderCommand { get; }

    // コンストラクタ
    public MainWindowViewModel()
	{

        // Mat
        CurrentMat = new ReactivePropertySlim<Cv.Mat?>()
            .AddTo(Disposable);

        // 実行中フラグ
        IsProcessing = new ReactivePropertySlim<bool>(false)
			.AddTo(Disposable);
        // タイトル
        Title = new ReactivePropertySlim<string>("OpenCvFilterMaker2")
            .AddTo(Disposable);
		// プレビュー画像オブジェクト
		ImagePreview = new ReactivePropertySlim<BitmapSource?>()
            .AddTo(Disposable);
        // フィルターメニュー
        FilterMenus = new ReactiveCollection<CvFilterBase>()
            .AddTo(Disposable);
        // ドラックアンドドロップ
        FileDropCommand = IsProcessing
			.Inverse()
			.ToAsyncReactiveCommand<string[]>()
			.WithSubscribe(async files => await LoadImagesAsync(files))
			.AddTo(Disposable);

        // フィルターメニューの初期化
        InitFilterMenus();


        // フィルターパイプライン
        Filters = new ReactiveCollection<CvFilterBase>()
            .AddTo(Disposable);
        // 選択中のフィルター
        SelectedFilter = new ReactivePropertySlim<CvFilterBase?>()
            .AddTo(Disposable);


        // パイプライン読み込み
        LoadPipelineCommand = new ReactiveCommandSlim()
            .WithSubscribe(() => LoadPipelineFromFile())
            .AddTo(Disposable);

        // パイプライン保存
        SavePipelineCommand = new ReactiveCommandSlim()
            .WithSubscribe(() => SavePipelineToFile())
            .AddTo(Disposable);

        // フィルターの削除
        RemoveFilterCommand = IsProcessing
            .Inverse()
            .ToReactiveCommandSlim()
            .WithSubscribe(() =>
            {
                if (SelectedFilter.Value is CvFilterBase f) Filters.Remove(f);
            })
            .AddTo(Disposable);

        // フィルターの実行順を上げる
        MoveUpCommand = IsProcessing
            .Inverse()
            .ToReactiveCommandSlim()
            .WithSubscribe(() => FiltesPosMode(-1))
            .AddTo(Disposable);

        // フィルターの実行順を下げる
        MoveDownCommand = IsProcessing
            .Inverse()
            .ToReactiveCommandSlim()
            .WithSubscribe(() => FiltesPosMode(+1))
            .AddTo(Disposable);

        // フィルター実行の可否
        var canExecute =
            Observable.CombineLatest(
                IsProcessing,
                CurrentMat,
                Filters.CollectionChangedAsObservable()
                    .Select(_ => Filters.Count)
                    .StartWith(Filters.Count),
                (p, m, c) => !p && m != null && c > 0
            );

        // フィルターの実行
        ExecuteFiltersCommand = canExecute
            .ToReactiveCommandSlim()
            .WithSubscribe(() => ExecuteFilters())
            .AddTo(Disposable);

        // フィルターのキャンセル
        CancelCommand = IsProcessing
            .ToReactiveCommandSlim()
            .WithSubscribe(()=> _cts?.Cancel())
            .AddTo(Disposable);

        // フィルターのリセット
        var canReset =
            Observable.CombineLatest(
                IsProcessing,
                CurrentMat,
                (p, m) => !p && m != null
            );
        ResetFiltersCommand = canReset
            .ToReactiveCommandSlim()
            .WithSubscribe(() =>
            {
                if (CurrentMat.Value is null) return;
                var result = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(CurrentMat.Value);
                result.Freeze(); // UIスレッド安全化
                ImagePreview.Value = ImageHelper.To96Dpi(result);
            })
            .AddTo(Disposable);

        // フィルターのクリア
        var canClear =
            Observable.CombineLatest(
                IsProcessing,
                Filters.CollectionChangedAsObservable()
                    .Select(_ => Filters.Count)
                    .StartWith(Filters.Count),
                (p, c) => !p &&  c > 0
            );
        ClearFiltersCommand = canClear
            .ToReactiveCommandSlim()
            .WithSubscribe(() => Filters.Clear())
            .AddTo(Disposable);

        // 結果のコピー
        var canCopy =
            Observable.CombineLatest(
                IsProcessing,
                ImagePreview,
                (p, i) => !p && i is not null
            );
        CopyResultToClipboardCommand = canCopy
            .ToReactiveCommandSlim()
            .WithSubscribe(() =>
            {
                if (ImagePreview.Value is null)
                    return;

                var img = ImagePreview.Value;
                // Freezeされていない場合は安全のためFreeze
                if (img.CanFreeze && !img.IsFrozen)
                    img.Freeze();

                Clipboard.SetImage(img);
            })
            .AddTo(Disposable);

        // 自動保存フラグ
        IsAutoSaveEnabled = new ReactivePropertySlim<bool>(false)
            .AddTo(Disposable);

        // 自動保存先フォルダを開く
        OpenFilteredFolderCommand = new ReactiveCommandSlim()
            .WithSubscribe(() =>
            {
                string folder = GetTodayFolder();

                if (!Directory.Exists(folder))
                    return;

                Process.Start(new ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true
                });
            })
            .AddTo(Disposable);

    }
    // フィルターの実行
    private async void ExecuteFilters()
    {
        if (CurrentMat.Value == null || IsProcessing.Value)
            return;

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        IsProcessing.Value = true;

        try
        {
            var result = await Task.Run(() =>
            {
                Cv.Mat current = CurrentMat.Value.Clone();

                foreach (var filter in Filters)
                {
                    token.ThrowIfCancellationRequested();

                    var next = filter.Execute(current);
                    current.Dispose();
                    current = next;
                }

                return current;
            }, token);

            if (IsAutoSaveEnabled.Value)
                SaveResultImage(result);

            var bmp = OpenCvSharp.WpfExtensions
                .BitmapSourceConverter.ToBitmapSource(result);

            bmp.Freeze();
            ImagePreview.Value = ImageHelper.To96Dpi(bmp);

            result.Dispose();
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("Processing canceled.");
        }
        finally
        {
            IsProcessing.Value = false;
            _cts.Dispose();  _cts = null;
        }
    }
    // フィルターの実行順の変更
    void FiltesPosMode(int delta)
    {
        if (SelectedFilter?.Value is not { } filter) return;

        int index = Filters.IndexOf(filter);
        int newIndex = index + delta;

        if (newIndex < 0 || newIndex >= Filters.Count) return;

        Filters.Move(index, newIndex);
    }
    // 画像の複数ロード
    async Task LoadImagesAsync(string[] files)
	{
        string[] images = files
            .Where(file => ImageHelper.IsSupportedImage(file))
            .ToArray();
        if (images.Length == 0) return;

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        IsProcessing.Value = true;

        try
        {
            foreach (var file in images)
            {
                await LoadImageAsync(file, token);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("Processing canceled.");
        }
        finally
        {
            IsProcessing.Value = false;
            _cts.Dispose(); _cts = null;
        }

    }

    // 画像ファイルのロード
    async Task LoadImageAsync(string file, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

        CurrentMat.Value?.Dispose();

        CurrentMat.Value = await Task.Run(()=>OpenCvHelper.Load(file), token);

		token.ThrowIfCancellationRequested();

		var bmp = await Task.Run(()=>
		{
			var b = OpenCvSharp.WpfExtensions
				.BitmapSourceConverter.ToBitmapSource(CurrentMat.Value);
			b = ImageHelper.To96Dpi(b);
			b.Freeze();
			return b;
		}, token);

		ImagePreview.Value = bmp;
	}
    // メニューの初期化
    void InitFilterMenus()
    {
        var types = typeof(CvFilterBase).Assembly
            .GetTypes()
            .Where(t =>
                typeof(CvFilterBase).IsAssignableFrom(t) &&
                !t.IsAbstract);
        foreach (var type in types)
        {
            var obj = (CvFilterBase)Activator.CreateInstance(type)!;

            obj.MenuCommand = new ReactiveCommand()
                .WithSubscribe(() =>
                {
                    //MessageBox.Show($"{type.FullName}");
                    var x = (CvFilterBase)Activator.CreateInstance(type)!;
                    Filters?.Add(x);
                    SelectedFilter?.Value = x;
                })
                .AddTo(Disposable);
            FilterMenus.Add(obj);
        }
    }
    // パイプラインの保存
    public void SavePipeline(string path)
    {
        var list = Filters
            .Select(f => FilterDto.ToDto(f))
            .ToList();

        var json = JsonSerializer.Serialize(
            list,
            new JsonSerializerOptions { WriteIndented = true });

        System.IO.File.WriteAllText(path, json);
    }
    private void SavePipelineToFile()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON Files (*.json)|*.json",
            DefaultExt = ".json",
            FileName = "pipeline.json"
        };

        if (dialog.ShowDialog() != true)
            return;

        SavePipeline(dialog.FileName);
    }
    // パイプラインの読み込み
    public void LoadPipeline(string path)
    {
        var json = System.IO.File.ReadAllText(path);

        var list = JsonSerializer
            .Deserialize<List<FilterDto>>(json);


        Filters.Clear();
        foreach (var dto in list!)
        {
            Filters.Add(FilterDto.FromDto(dto));
        }
    }
    private void LoadPipelineFromFile()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON Files (*.json)|*.json",
            DefaultExt = ".json"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            LoadPipeline(dialog.FileName);
        }
        catch (Exception ex)
        {

            Debug.Print($"`{ex.Message}");
            throw;
        }
    }

    // 結果画像を保存
    private static void SaveResultImage(Cv.Mat mat)
    {
        string folder = GetTodayFolder();
        string path = GetNextSequentialFilePath(folder);

        Cv.Cv2.ImWrite(path, mat);
    }
    private static string GetTodayFolder()
    {
        string pictures = Environment.GetFolderPath(
            Environment.SpecialFolder.MyPictures);

        string baseFolder = Path.Combine(pictures, "Filtered");

        if (!Directory.Exists(baseFolder))
            Directory.CreateDirectory(baseFolder);

        string todayFolder = Path.Combine(
            baseFolder,
            DateTime.Now.ToString("yyyyMMdd"));

        if (!Directory.Exists(todayFolder))
            Directory.CreateDirectory(todayFolder);

        return todayFolder;
    }
    private static string GetNextSequentialFilePath(string folder)
    {
        var files = Directory
            .GetFiles(folder, "*.png")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => int.TryParse(name, out var n) ? n : -1)
            .Where(n => n >= 0)
            .OrderBy(n => n)
            .ToList();

        int nextNumber = files.Count == 0
            ? 1
            : files.Last() + 1;

        string fileName = $"{nextNumber:000}.png";

        return Path.Combine(folder, fileName);
    }
}