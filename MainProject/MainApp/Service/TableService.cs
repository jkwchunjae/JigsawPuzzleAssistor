using PuzzleTableHelperCore;
using System.Text.Json;

namespace MainApp.Service;

public class TableService
{
    private WorkspaceData workspace;
    public TableService(WorkspaceData workspace)
    {
        this.workspace = workspace;
    }

    public Task<string[]> GetTableFiles()
    {
        if (!Directory.Exists(workspace.ResultDir))
        {
            Directory.CreateDirectory(workspace.ResultDir);
        }

        var files = Directory.GetFiles(workspace.ResultDir)
            .Where(file => Path.GetExtension(file) == ".json")
            .ToArray();
        return Task.FromResult(files);
    }

    public async Task<PuzzleTable> LoadPuzzleTable(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var table = JsonSerializer.Deserialize<PuzzleTable>(json);
        return table;
    }

    public async Task CreatePuzzleTable(string fileName, PuzzleCell? initCell)
    {
        if (!Directory.Exists(workspace.ResultDir))
        {
            Directory.CreateDirectory(workspace.ResultDir);
        }
        var table = new PuzzleTable
        {
            Cells = new List<List<PuzzleCell?>>
            {
                new List<PuzzleCell?> { initCell },
            },
        };
        var json = JsonSerializer.Serialize(table, new JsonSerializerOptions
        {
            WriteIndented = true,
        });
        fileName = fileName.EndsWith(".json") ? fileName : $"{fileName}.json";
        var filePath = Path.Combine(workspace.ResultDir, fileName);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<PuzzleTableInitOption?> LoadTableInitOption(string fileName)
    {
        var filePath = workspace.TableInitOptionPath(fileName);
        if (!File.Exists(filePath))
        {
            return null;
        }
        var json = await File.ReadAllTextAsync(filePath);
        var option = JsonSerializer.Deserialize<PuzzleTableInitOption>(json);
        return option;
    }

    public async Task SaveTableInitOption(string fileName, PuzzleTableInitOption option)
    {
        var filePath = workspace.TableInitOptionPath(fileName);
        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        }

        var json = JsonSerializer.Serialize(option, new JsonSerializerOptions
        {
            WriteIndented = true,
        });
        await File.WriteAllTextAsync(filePath, json);
    }
}
