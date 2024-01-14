namespace RJCP.VsSolutionSort.CmdLine
{
    using System.Collections.Generic;
    using RJCP.Core.CommandLine;

    internal class SolutionOptions
    {
        [Option('v', "version")]
        public bool Version { get; private set; }

        [Option('?', "help")]
        public bool Help { get; private set; }

        [Option('R', "recurse")]
        public bool Recurse { get; private set; }

        [Option('d', "dryrun")]
        public bool DryRun { get; private set; }

        [OptionArguments]
        public readonly List<string> Arguments = new();
    }
}
