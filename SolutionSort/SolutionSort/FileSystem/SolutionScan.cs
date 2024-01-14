namespace RJCP.VsSolutionSort.FileSystem
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class SolutionScan
    {
        public async static Task<int> ProcessFileAsync(string solutionFile, bool dryRun)
        {
            var solution = new SortedSolution();
            try {
                if (dryRun)
                    Console.WriteLine($"Loading: {solutionFile}");
                await solution.LoadAsync(solutionFile);

                if (!dryRun)
                    await solution.WriteAsync(solutionFile);
            } catch (SolutionFormatException ex) {
                CmdLine.Terminal.WriteLine($"ERROR: Failed Parsing '{solutionFile}' - {ex.Message}");
                return 1;
            } catch (Exception ex) {
                CmdLine.Terminal.WriteLine($"ERROR: {ex.Message}");
                return 255;
            }
            return 0;
        }

        public async static Task<int> ProcessDirAsync(string directory, bool dryRun)
        {
            ConcurrentQueue<string> files = new();
            if (!await GetPathsAsync(directory, new DotSolution(), files)) {
                CmdLine.Terminal.WriteLine("Couldn't parse files");
                return 255;
            }

            // Process the files in parallel.
            int result = 0;
            ParallelOptions options = new() { MaxDegreeOfParallelism = 4 };
            await Parallel.ForEachAsync(files, options, async (file, token) => {
                int processResult = await ProcessFileAsync(file, dryRun);

                // Atomically update the result to be the highest value.
                int initialValue, newValue;
                do {
                    initialValue = result;
                    newValue = Math.Max(initialValue, processResult);
                } while (initialValue != Interlocked.CompareExchange(ref result, newValue, initialValue));
            });

            return result;
        }

        private static async Task<bool> GetPathsAsync(string directory, DotSolution solutionRule, ConcurrentQueue<string> paths)
        {
            string solutionRuleFile = Path.Combine(directory, ".solutionsort");
            if (File.Exists(solutionRuleFile)) {
                solutionRule = new DotSolution(solutionRuleFile);
            }

            IEnumerable<string> files = Directory.GetFiles(directory, "*.sln");
            foreach (string file in files) {
                bool process = await solutionRule.ShouldProcessSolutionAsync(file);
                if (process) paths.Enqueue(file);
            }

            IEnumerable<string> dirs = Directory.GetDirectories(directory);
            bool result = true;
            foreach (string dir in dirs) {
                if (!await GetPathsAsync(dir, solutionRule, paths)) {
                    result = false;
                }
            }

            return result;
        }
    }
}
