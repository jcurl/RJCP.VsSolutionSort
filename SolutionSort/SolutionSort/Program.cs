namespace RJCP.VsSolutionSort
{
    using System;
    using System.Threading.Tasks;

    internal static class Program
    {
        internal async static Task<int> Main(string[] args)
        {
            var solution = new SortedSolution();
            try {
                await solution.LoadAsync(args[0]);
            } catch (SolutionFormatException ex) {
                Console.WriteLine($"Failed - {ex.Message}");
                return 1;
            }

            solution.WriteAsync(null);

            return 0;
        }
    }
}
