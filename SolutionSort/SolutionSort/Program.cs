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

            if (options.Arguments.Count != 1) {
                CmdLine.Terminal.WriteLine("ERROR: No files given.");
                CmdLine.Terminal.WriteLine();
                CmdLine.Help.PrintSimpleHelp();
                return 1;
            }

            var solution = new SortedSolution();
            try {
                await solution.LoadAsync(args[0]);
                await solution.WriteAsync(args[0]);
            } catch (SolutionFormatException ex) {
                CmdLine.Terminal.WriteLine($"Failed Parsing - {ex.Message}");
                return 1;
            }

            return 0;
        }
    }
}
