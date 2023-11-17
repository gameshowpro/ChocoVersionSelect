namespace ChocoVersionSelect.Model;

internal static class CommandLineParser
{
    internal const string Syntax = "<packageName> [--source=<sourceName>]";
    private const string SourceOption = "--source=";
    /// <summary>
    /// Parse the commandline arguments into a strongly-typed record. 
    /// If combination is invalid, return null.
    /// </summary>
    internal static CommandLineArgs? Parse(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0])) return null;
        if (args.Length == 1)
        {
            return new(args[0], null);
        }
        if (args.Length == 2 && args[1].StartsWith(SourceOption))
        {
            return new(args[0], args[1][SourceOption.Length..]);
        }
        return null;
    }
}
