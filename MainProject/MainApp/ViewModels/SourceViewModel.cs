using System.Collections.ObjectModel;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MainApp.Contracts.Services;
using MainApp.Contracts.ViewModels;
using MainApp.Core.Contracts.Services;
using MainApp.Core.Models;
using MainApp.Core.Services;

namespace MainApp.ViewModels;

public partial class SourceViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly IPuzzleSourceService _puzzleSourceService;

    public ObservableCollection<PuzzleSource> Source { get; } = new ObservableCollection<PuzzleSource>();

    public SourceViewModel(INavigationService navigationService, IPuzzleSourceService puzzleSourceService)
    {
        _navigationService = navigationService;
        _puzzleSourceService = puzzleSourceService;
    }

    public void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        var path = @"D:\puzzle\0_source";
        var data = _puzzleSourceService.GetSourceImageFiles(path);
        foreach (var item in data)
        {
            Source.Add(item);
        }
    }

    public void OnNavigatedFrom()
    {
    }

    [RelayCommand]
    private void OnItemClick(PuzzleSource? clickedItem)
    {
        if (clickedItem != null)
        {
            _navigationService.SetListDataItemForNextConnectedAnimation(clickedItem);
            _navigationService.NavigateTo(typeof(SourceDetailViewModel).FullName!, clickedItem.Name);
        }
    }
}
