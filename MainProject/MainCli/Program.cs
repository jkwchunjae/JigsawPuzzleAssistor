using JkwExtensions;
using MainCli;

var commands = new List<(string Command, IMainRunner Runner)>
{
    ("Crop", new PuzzleCrop()),
    ("DebugCorner", new DebugCorner()),
    ("PieceInfo", new PieceInfoJson()),
    ("ConnecInfo", new ConnectInfoJson()),
    ("FindTargetTest", new FindTargetTest()),
};

Console.WriteLine(
    commands.Select((cmd, i) => $"[{i}]: {cmd.Command}")
        .StringJoin(Environment.NewLine)
    );

var input = Console.ReadLine();

if (int.TryParse(input, out var index) && index < commands.Count)
{
    var runner = commands[index].Runner;
    Console.WriteLine($"{index}: {commands[index].Command} !!");
    await runner.Run();
}
else
{
    Console.WriteLine(index + " is not valid index");
}
