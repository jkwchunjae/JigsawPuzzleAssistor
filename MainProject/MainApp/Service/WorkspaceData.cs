namespace MainApp.Service;

public class WorkspaceData
{
    public required string Name { get; set; }
    public required string RootPath { get; set; }
    public string? InputRegex { get; set; }

    public string SourceDir => Path.Join(RootPath, "0_source");
    public string ResizeDir => Path.Join(RootPath, "1_resize");
    public string OutlineDir => Path.Join(RootPath, "2_outline");
    public string CornerDir => Path.Join(RootPath, "3_corner");
    public string InfoDir => Path.Join(RootPath, "4_info");
    public string ConnectionDir => Path.Join(RootPath, "5_connection");
    public string ResultDir => Path.Join(RootPath, "6_result");
    public string TempDir => Path.Join(RootPath, "9_temp");
    public string CornerErrorsPath => Path.Join(TempDir, "corner_errors.json");
}
