namespace RJCP.VsSolutionSort
{
    using System.Threading.Tasks;
    using RJCP.Core.CommandLine;
    using RJCP.Core.Terminal;

    internal static class Program
    {
        private const OptionsStyle optionsStyle = OptionsStyle.Unix;
        private static readonly ITerminal Terminal = new ConsoleTerminal();

        internal async static Task<int> Main(string[] args)
        {
            Options cmdLine;
            CmdLine.SolutionOptions options = new();
            try {
                cmdLine = Options.Parse(options, args, optionsStyle);
            } catch (OptionException ex) {
                Terminal.StdOut.WrapLine($"ERROR: {ex.Message}");
                Terminal.StdOut.WriteLine();
                CmdLine.Help help = new(new Options(optionsStyle), Terminal);
                help.PrintSimpleHelp();
                return 1;
            }

            if (options.Version) {
                CmdLine.Version version = new(Terminal);
                version.PrintVersion();
                return 0;
            }

            if (options.Help) {
                CmdLine.Help help = new(cmdLine, Terminal);
                help.PrintHelp();
                return 0;
            }

            if (options.Recurse) {
                string dir = ".";
                if (options.Arguments.Count == 1) {
                    dir = options.Arguments[0];
                } else if (options.Arguments.Count > 1) {
                    Terminal.StdOut.WrapLine("ERROR: A single directory should be given to start scanning *.sln files from.");
                    Terminal.StdOut.WriteLine();
                    CmdLine.Help help = new(cmdLine, Terminal);
                    help.PrintSimpleHelp();
                    return 1;
                }

                FileSystem.SolutionScan scan = new(Terminal);
                return await scan.ProcessDirAsync(dir, options.Jobs, options.DryRun);
            } else {
                if (options.Arguments.Count != 1) {
                    Terminal.StdOut.WrapLine("ERROR: Exactly one solution file should be provided on the command line.");
                    Terminal.StdOut.WriteLine();
                    CmdLine.Help help = new(cmdLine, Terminal);
                    help.PrintSimpleHelp();
                    return 1;
                }

                FileSystem.SolutionScan scan = new(Terminal);
                return await scan.ProcessFileAsync(options.Arguments[0], options.DryRun);
            }
        }
    }
}
