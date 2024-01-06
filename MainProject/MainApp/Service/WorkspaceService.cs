using System.Text.Json;
using JkwExtensions;

namespace MainApp.Service;

public class WorkspaceData
{
    public required string Name { get; set; }
    public required string RootPath { get; set; }
    public string? InputRegex { get; set; }
}

public class WorkspaceService
{
    private ILogger logger;
    private WorkspaceData? currentWorkspace;
    public WorkspaceService(ILogger<WorkspaceService> logger)
    {
        this.logger = logger;
    }
    public async Task<IEnumerable<WorkspaceData>> GetWorkspaces()
    {
        try
        {
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var assistorConfigDir = Path.Join(localAppDataPath, "jigsaw-puzzle-assistor");

            if (!Directory.Exists(assistorConfigDir))
            {
                return Enumerable.Empty<WorkspaceData>();
            }

            var list = await Directory.GetFiles(assistorConfigDir)
                .Where(path => Path.GetFileName(path).StartsWith("workspace"))
                .Select(async path => await LoadWorkspaceData(path))
                .WhenAll();

            return list
                .Where(data => data != null)
                .Select(data => data!)
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Errorn on GetWorkspaces()");
            return Enumerable.Empty<WorkspaceData>();
        }

        async Task<WorkspaceData?> LoadWorkspaceData(string workspacePath)
        {
            var text = await File.ReadAllTextAsync(workspacePath);
            return JsonSerializer.Deserialize<WorkspaceData>(text);
        }
    }
    public async Task<WorkspaceData?> AddNewWorkspace(string name, string workspacePath)
    {
        try
        {
            var workspaceData = new WorkspaceData
            {
                Name = name,
                RootPath = workspacePath,
            };
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var assistorConfigDir = Path.Join(localAppDataPath, "jigsaw-puzzle-assistor");
            if (!Directory.Exists(assistorConfigDir))
            {
                Directory.CreateDirectory(assistorConfigDir);
            }
            var dataPath = Path.Join(assistorConfigDir, $"workspace-{name}.json");
            var json = JsonSerializer.Serialize(workspaceData, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(dataPath, json);
            return workspaceData;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Errorn on AddNewWorkspace()");
            return null;
        }
    }
    public void SelectWorkspace(WorkspaceData workspaceData)
    {
        currentWorkspace = workspaceData;
    }
}
