using System.Security;
using Maywork.WPF.Helpers;
using OpenCvSharp;

namespace OpenCvImageFilters;
public class MainWindowViewState : ViewModelBase
{
	public Mat? MatSrc { get; set; }
	private Mat? _matDst;
	public Mat? MatDst
	{
		get => _matDst;
		set
		{
			SetProperty(ref _matDst, value, _ =>
				{
					if (value != null)
					{
						IsFilterApplied = true;
					}
				});
		}
	}

	private bool _isImageLoaded = false;
	public bool IsImageLoaded
	{
		get => _isImageLoaded;
		set => SetProperty(ref _isImageLoaded, value);
	}
	bool _isFilterApplied = false;
	public bool IsFilterApplied { get => _isFilterApplied; set => SetProperty(ref _isFilterApplied, value); }
}