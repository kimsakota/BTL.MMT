// Bootstrap entry point only. Networking is implemented by the team in later milestones.
if (args.Length == 1 && args[0] == "--version")
{
    Console.WriteLine("ET4230.P2P 0.1.0-bootstrap");
    return 0;
}

if (args.Length == 0 || (args.Length == 1 && args[0] is "--help" or "-h"))
{
    Console.WriteLine("ET4230 De 14 - P2P C# project bootstrap");
    Console.WriteLine("Members: Linh, Minh, Cuong, Quan, Tu");
    Console.WriteLine("Available: --help, --version");
    Console.WriteLine("Tracker, peer, transfer and resume are not implemented yet.");
    Console.WriteLine("Start with README.md.");
    return 0;
}

Console.Error.WriteLine("Unsupported command in bootstrap. Run with --help.");
return 2;
