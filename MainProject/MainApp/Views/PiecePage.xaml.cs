using MainApp.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace MainApp.Views;

public sealed partial class PiecePage : Page
{
    public PieceViewModel ViewModel
    {
        get;
    }

    public PiecePage()
    {
        ViewModel = App.GetService<PieceViewModel>();
        InitializeComponent();
    }
}
