using JkwExtensions;
using MainCli;

var commands = new string[]
{
    "DebugCorner",
    "PieceInfo",
};

Console.WriteLine(
    commands.Select((cmd, i) => $"[{i}]: {cmd}")
        .StringJoin(Environment.NewLine)
    );

var input = Console.ReadLine();

if (int.TryParse(input, out var index))
{
    var cmd = commands[index];
    if (cmd == "DebugCorner")
    {
        Console.WriteLine("Debug Corners !!");
        await new DebugCorner().Run();
        return;
    }
    else if (cmd == "PieceInfo")
    {
        Console.WriteLine("Piece Info !!");
        await new PieceInfoJson().Run();
        return;
    }
}
else
{
    Console.WriteLine(index + " is not valid index");
}

