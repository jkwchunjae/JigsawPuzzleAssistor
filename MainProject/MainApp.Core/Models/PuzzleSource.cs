using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainApp.Core.Models;
public class PuzzleSource
{
    public string FilePath { get; set; } = string.Empty;
    public string Name => Path.GetFileNameWithoutExtension(FilePath);
}
