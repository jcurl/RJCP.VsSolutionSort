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
            try {
                using DotSolution defaultDotSolution = new();
                await GetPathsAsync(directory, defaultDotSolution, files);
            } catch (Exception ex) {
                CmdLine.Terminal.WriteLine($"ERROR: Problem enumerating directory for files - {ex.Message}");
                return 255;
            }

            // Process the files in parallel.
            int result = 0;
            await Parallel.ForEachAsync(files, async (file, token) => {
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

        private static async Task GetPathsAsync(string directory, DotSolution solutionRule, ConcurrentQueue<string> paths)
        {
            bool ownsDotSolution = false;
            string solutionRuleFile = Path.Combine(directory, ".solutionsort");
            if (File.Exists(solutionRuleFile)) {
                solutionRule = new DotSolution(solutionRuleFile);
                ownsDotSolution = true;
            }

            try {
                IEnumerable<string> files = Directory.GetFiles(directory, "*.sln");
                foreach (string file in files) {
                    bool process = await solutionRule.ShouldProcessSolutionAsync(file);
                    if (process) paths.Enqueue(file);
                }

                IEnumerable<string> dirs = Directory.GetDirectories(directory);
                await Parallel.ForEachAsync(dirs, async (dir, token) => {
                    await GetPathsAsync(dir, solutionRule, paths);
                });
            } finally {
                if (ownsDotSolution) solutionRule.Dispose();
            }
        }
    }
}
