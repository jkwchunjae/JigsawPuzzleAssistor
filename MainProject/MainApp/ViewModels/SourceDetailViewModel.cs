using CommunityToolkit.Mvvm.ComponentModel;

using MainApp.Contracts.ViewModels;
using MainApp.Core.Contracts.Services;
using MainApp.Core.Models;
using MainApp.Core.Services;

namespace MainApp.ViewModels;

public partial class SourceDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly IPuzzleSourceService _puzzleSourceService;

    [ObservableProperty]
    private PuzzleSource? item;

    public SourceDetailViewModel(IPuzzleSourceService puzzleSourceService)
    {
        _puzzleSourceService = puzzleSourceService;
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is string fileName)
        {
            var path = @"D:\puzzle\0_source";
            var data = _puzzleSourceService.GetSourceImageFiles(path);
            Item = data.First(x => x.Name == fileName);
        }
    }

    public void OnNavigatedFrom()
    {
    }
}
