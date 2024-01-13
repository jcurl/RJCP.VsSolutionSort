namespace RJCP.VsSolutionSort.CmdLine
{
    using System;
    using Resources;

    internal static class Help
    {
        const string ShortOptionSymbol = "-";
        const string LongOptionSymbol = "--";
        const string AssignmentSymbol = "=";

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
            Write();
            Write(HelpResource.Help400_Input);
            Write();
            Write(2, 4, HelpResource.Help410_InputSolution);
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
            Terminal.WriteLine(indent, hangingIndent, line, ShortOptionSymbol, LongOptionSymbol, AssignmentSymbol);
        }
    }
}
