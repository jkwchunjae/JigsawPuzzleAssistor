using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MainApp.Core.Models;

namespace MainApp.Core.Services;

public interface IPuzzleSourceService
{
    IEnumerable<PuzzleSource> GetSourceImageFiles(string folderPath);
}
public class PuzzleSourceService : IPuzzleSourceService
{
    public IEnumerable<PuzzleSource> GetSourceImageFiles(string folderPath)
    {
        return Directory.GetFiles(folderPath)
            .Select(path => new PuzzleSource
            {
                FilePath = path
            })
            .ToArray();
    }
}
