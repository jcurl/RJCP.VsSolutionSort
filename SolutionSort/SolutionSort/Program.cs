namespace RJCP.VsSolutionSort
{
    using System.Threading.Tasks;
    using RJCP.Core.CommandLine;

    internal static class Program
    {
        internal async static Task<int> Main(string[] args)
        {
            CmdLine.SolutionOptions options = new();
            try {
                Options.Parse(options, args, OptionsStyle.Unix);
            } catch (OptionException ex) {
                CmdLine.Terminal.WriteLine($"ERROR: {ex.Message}");
                CmdLine.Terminal.WriteLine();
                CmdLine.Help.PrintSimpleHelp();
                return 1;
            }

            if (options.Version) {
                CmdLine.Version.PrintVersion();
                return 0;
            }

            if (options.Help) {
                CmdLine.Help.PrintHelp();
                return 0;
            }

            if (options.Recurse) {
                string dir = ".";
                if (options.Arguments.Count == 1) {
                    dir = options.Arguments[0];
                } else if (options.Arguments.Count > 1) {
                    CmdLine.Terminal.WriteLine("ERROR: A single directory should be given to start scanning *.sln files from.");
                    CmdLine.Terminal.WriteLine();
                    CmdLine.Help.PrintSimpleHelp();
                    return 1;
                }

                return await FileSystem.SolutionScan.ProcessDirAsync(dir, options.Jobs, options.DryRun);
            } else {
                if (options.Arguments.Count != 1) {
                    CmdLine.Terminal.WriteLine("ERROR: Exactly one solution file should be provided on the command line.");
                    CmdLine.Terminal.WriteLine();
                    CmdLine.Help.PrintSimpleHelp();
                    return 1;
                }

                return await FileSystem.SolutionScan.ProcessFileAsync(options.Arguments[0], options.DryRun);
            }
        }
    }
}
