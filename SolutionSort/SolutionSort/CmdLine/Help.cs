namespace RJCP.VsSolutionSort.CmdLine
{
    using System;
    using System.IO;
    using Resources;
    using RJCP.Core.CommandLine;
    using RJCP.Core.Terminal;

    internal class Help
    {
        private static readonly object ExeNameLock = new();
        private static string s_ExeName;

        private readonly Options m_Options;
        private readonly ITerminal m_Terminal;

        public Help(Options options, ITerminal terminal)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(terminal);
            m_Options = options;
            m_Terminal = terminal;
        }

        private string ShortOptionSymbol { get { return m_Options.ShortOptionPrefix; } }

        private string LongOptionSymbol { get { return m_Options.LongOptionPrefix; } }

        private string AssignmentSymbol { get { return m_Options.AssignmentSymbol; } }

        private static string ExeName
        {
            get
            {
                if (s_ExeName is null) {
                    lock (ExeNameLock) {
                        if (s_ExeName is null) {
                            string exeName =
                                Path.GetFileNameWithoutExtension(Environment.ProcessPath);
                            if (exeName is null) {
                                s_ExeName = "VsSolutionSort";
                            } else if (exeName.StartsWith("dotnet-", StringComparison.InvariantCultureIgnoreCase)) {
                                s_ExeName = $"dotnet {exeName[7..]}";
                            } else {
                                s_ExeName = exeName;
                            }
                        }
                    }
                }
                return s_ExeName;
            }
        }

        public void PrintHelp()
        {
            Write(HelpResource.Help100_Description);
            Write();
            Write(HelpResource.Help200_Usage);
            Write();
            Write(2, 4, HelpResource.Help210_UsageInfo);
            Write();
            Write(HelpResource.Help300_Options);
            Write();
            Write(2, 4, HelpResource.Help310_HelpOption);
            Write(2, 4, HelpResource.Help311_VersionOption);
            Write(2, 4, HelpResource.Help315_DryRun);
            Write(2, 4, HelpResource.Help320_Recurse);
            Write(2, 4, HelpResource.Help325_Jobs);
            Write();
            Write(HelpResource.Help400_Input);
            Write();
            Write(2, 4, HelpResource.Help410_InputSolution);
            Write(2, 4, HelpResource.Help415_InputDir);
            Write();
            Write(HelpResource.Help800_ExitCodes);
            Write();
            Write(2, 2, HelpResource.Help801_ExitDescription);
            Write();
            Write(2, 4, HelpResource.Help810_Success);
            Write(2, 4, HelpResource.Help811_SolutionError);
            Write(2, 4, HelpResource.Help899_UnknownError);
            Write();
        }

        public void PrintSimpleHelp()
        {
            Write(HelpResource.Help200_Usage);
            Write();
            Write(2, 4, HelpResource.Help210_UsageInfo);
            Write();
            Write(HelpResource.Help300_Options);
            Write();
            Write(2, 4, HelpResource.Help310_HelpOption);
            Write(2, 4, HelpResource.Help311_VersionOption);
            Write();
        }

        private void Write()
        {
            m_Terminal.StdOut.WriteLine();
        }

        private void Write(string message)
        {
            Write(0, 0, message);
        }

        private void Write(int indent, int hangingIndent, string message)
        {
            m_Terminal.StdOut.WrapLine(indent, hangingIndent - indent,
                message, ShortOptionSymbol, LongOptionSymbol, AssignmentSymbol, ExeName);
        }
    }
}
