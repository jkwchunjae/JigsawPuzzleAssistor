using Common.PieceInfo;
using JkwExtensions;
using MainApp.Service;
using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace MainApp.Pages.SortedEdge;

public record EdgeInfo(string Piece1, string Piece2, float Value);

public partial class SortedEdgePage : ComponentBase
{
    [Inject] NavigationManager NavigationManager { get; set; } = null!;
    [Inject] WorkspaceService WorkspaceService { get; set; } = null!;

    WorkspaceData? workspace => WorkspaceService.CurrentWorkspace;

    ConnectInfo[] connections = Array.Empty<ConnectInfo>();
    EdgeInfo[] SortedEdges = Array.Empty<EdgeInfo>();

    protected override async Task OnInitializedAsync()
    {
        if (workspace == null)
        {
            NavigationManager.NavigateTo("/start");
            return;
        }

        var files = Directory.GetFiles(workspace!.ConnectionDir);
        var connectionInfos = await files
            .Select(x => ReadFromFile(x))
            .WhenAll();

        SortedEdges = connectionInfos
            .SelectMany(x => x.Edges.SelectMany(y => y.Connection.Select(z => new EdgeInfo(x.PieceName, z.PieceName, Value: z.Value))))
            .Select(x => x.Piece1.CompareTo(x.Piece2) < 0 ? x : new EdgeInfo(x.Piece2, x.Piece1, x.Value))
            .Distinct()
            .OrderBy(x => x.Value)
            .Take(1000)
            .ToArray();
    }

    private static async Task<ConnectInfo> ReadFromFile(string path)
    {
        var text = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<ConnectInfo>(text)!;
    }
}
