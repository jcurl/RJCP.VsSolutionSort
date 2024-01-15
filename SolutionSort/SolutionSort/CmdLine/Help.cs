namespace RJCP.VsSolutionSort.CmdLine
{
    using System;
    using System.IO;
    using Resources;

    internal static class Help
    {
        const string ShortOptionSymbol = "-";
        const string LongOptionSymbol = "--";
        const string AssignmentSymbol = "=";

        private static readonly object ExeNameLock = new ();
        private static string s_ExeName;

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

        public static void PrintHelp()
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

        public static void PrintSimpleHelp()
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

        private static void Write()
        {
            Console.WriteLine();
        }

        private static void Write(string line)
        {
            Write(0, 0, line);
        }

        private static void Write(int indent, int hangingIndent, string line)
        {
            Terminal.WriteLine(indent, hangingIndent, line, ShortOptionSymbol, LongOptionSymbol, AssignmentSymbol, ExeName);
        }
    }
}
