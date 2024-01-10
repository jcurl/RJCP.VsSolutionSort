namespace RJCP.VsSolutionSort
{
    using System;
    using System.Threading.Tasks;

    internal static class Program
    {
        internal async static Task<int> Main(string[] args)
        {
            if (args.Length != 1) {
                Console.WriteLine("VsSolutionSort <filename.sln>");
                return 1;
            }

            var solution = new SortedSolution();
            try {
                await solution.LoadAsync(args[0]);
                await solution.WriteAsync(args[0]);
            } catch (SolutionFormatException ex) {
                Console.WriteLine($"Failed Parsing - {ex.Message}");
                return 1;
            }

            return 0;
        }
    }
}
