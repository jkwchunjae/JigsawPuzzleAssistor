using MainApp.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PuzzleTableHelperCore;

namespace MainApp.Pages.Table;

public partial class PuzzleTablePage : ComponentBase
{
    [Inject] NavigationManager NavigationManager { get; set; } = null!;
    [Inject] WorkspaceService WorkspaceService { get; set; } = null!;
    [Inject] IJSRuntime Js { get; set; } = null!;

    WorkspaceData? workspace => WorkspaceService.CurrentWorkspace;

    TableService service;

    string[] tables = Array.Empty<string>();
    string selectedTablePath = string.Empty;

    PuzzleTableServiceOption? puzzleTableServiceOption = null;

    protected override async Task OnInitializedAsync()
    {
        if (workspace == null)
        {
            NavigationManager.NavigateTo("/start");
            return;
        }
        else if (!WorkspaceService.HasCroppedImage())
        {
            NavigationManager.NavigateTo("/outline");
            return;
        }
        else if (!WorkspaceService.ReadyToTable())
        {
            NavigationManager.NavigateTo("/info");
            return;
        }

        service = new TableService(workspace!);

        await LoadTables();
    }

    async Task LoadTables()
    {
        tables = await service.GetTableFiles();
    }

    async Task TableChanged(string path)
    {
        selectedTablePath = path;
        var tableServiceOption = new PuzzleTableServiceOption
        {
            PieceInfoDirectory = workspace!.InfoDir,
            ConnectInfoDirectory = workspace!.ConnectionDir,
            PuzzleTableFilePath = path,
        };
        puzzleTableServiceOption = tableServiceOption;
    }
}
