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

        [OptionArguments]
        public readonly List<string> Arguments = new();
    }
}
