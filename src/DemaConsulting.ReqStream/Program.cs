// Copyright (c) 2026 DEMA Consulting
// Licensed under the MIT License

namespace DemaConsulting.ReqStream;

/// <summary>
/// Main program entry point for the ReqStream tool.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Main entry point for the application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Exit code.</returns>
    private static int Main(string[] args)
    {
        Console.WriteLine("ReqStream - Requirements Management Tool");
        Console.WriteLine("Copyright (c) 2026 DEMA Consulting");
        Console.WriteLine();
        
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: reqstream <command> [options]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  help    Show help information");
            Console.WriteLine();
            return 0;
        }
        
        Console.WriteLine($"Command: {args[0]}");
        return 0;
    }
}
