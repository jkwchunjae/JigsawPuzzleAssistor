using MainApp.Service;
using Microsoft.AspNetCore.Components;

namespace MainApp.Pages.Source;

public partial class SourcePage : ComponentBase
{
    [Inject] WorkspaceService WorkspaceService { get; set; } = null!;

    bool HasSourceImage()
    {
        WorkspaceService.HasWo
    }
}
