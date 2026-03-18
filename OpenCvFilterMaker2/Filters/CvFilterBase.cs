using OpenCvSharp;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Cv = OpenCvSharp;

namespace OpenCvFilterMaker2;

public abstract class CvFilterBase : INotifyPropertyChanged
{
    #region
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    protected CompositeDisposable Disposable { get; } = [];
    public void Dispose() => Disposable.Dispose();

    #endregion
    public string MenuHeader { get; set; } = string.Empty;
    public ICommand? MenuCommand { get; set; }
    
    public ReactivePropertySlim<bool> IsEnabled { get; set; } = new(true);
    public ReactivePropertySlim<string> Name { get; set; } = new("");

    public Cv.Mat Execute(Cv.Mat input)
    {
        if (input == null || input.Empty())
            throw new ArgumentException("Input Mat is null or empty");

        try
        {
            if (!IsEnabled.Value)
                return input.Clone();

            return Apply(input);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"Type={input.Type()} Depth={input.Depth()} Ch={input.Channels()}");
            throw new Exception($"Filter '{Name}' failed.", ex);
        }
    }

    protected abstract Cv.Mat Apply(Cv.Mat input);
}
