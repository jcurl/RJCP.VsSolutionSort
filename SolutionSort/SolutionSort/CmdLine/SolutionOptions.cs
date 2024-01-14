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

        private int m_Jobs = 0;

        [Option('j', "jobs")]
        public int Jobs
        {
            get { return m_Jobs; }
            private set
            {
                if (value < 1)
                    throw new OptionException("Number of jobs must be 1 or more.");
                if (value > 255)
                    throw new OptionException("Maximum number of jobs is 255.");
                m_Jobs = value;
            }
        }

        [OptionArguments]
        public readonly List<string> Arguments = new();
    }
}
