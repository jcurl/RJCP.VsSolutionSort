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

        public async static Task<int> ProcessDirAsync(string directory, int jobs, bool dryRun)
        {
            IReadOnlyCollection<string> files;
            try {
                using SolutionPaths pathsSearch = new(directory, jobs);
                files = await pathsSearch.GetPathsAsync();
            } catch (Exception ex) {
                CmdLine.Terminal.WriteLine($"ERROR: Problem enumerating directory for files - {ex.Message}");
                return 255;
            }

            // Process the files in parallel.
            ParallelOptions options = new() {
                MaxDegreeOfParallelism = (jobs > 0) ? jobs : -1
            };

            int result = 0;
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

        private sealed class SolutionPaths : IDisposable
        {
            private readonly string m_RootDirectory;
            private readonly int m_Jobs;
            private readonly SemaphoreSlim m_JobsSema;

            public SolutionPaths(string rootDirectory, int jobs)
            {
                ArgumentNullException.ThrowIfNull(rootDirectory);
                m_RootDirectory = rootDirectory;
                m_Jobs = jobs;

                if (jobs > 0) {
                    m_JobsSema = new SemaphoreSlim(jobs);
                } else {
                    m_JobsSema = new SemaphoreSlim(Environment.ProcessorCount);
                }
            }

            public async Task<IReadOnlyCollection<string>> GetPathsAsync()
            {
                ConcurrentQueue<string> files = new();
                using DotSolution defaultDotSolution = new();
                await GetPathsAsync(m_RootDirectory, defaultDotSolution, files);
                return files;
            }

            private async Task GetPathsAsync(string directory, DotSolution solutionRule, ConcurrentQueue<string> paths)
            {
                // Don't process folders that start with '.'.
                string lastDir = Path.GetFileName(directory);
                if (lastDir.Length == 0 || lastDir.Length > 1 && lastDir[0] == '.') return;

                bool ownsDotSolution = false;
                string solutionRuleFile = Path.Combine(directory, ".solutionsort");
                if (File.Exists(solutionRuleFile)) {
                    solutionRule = new DotSolution(solutionRuleFile);
                    ownsDotSolution = true;
                }

                try {
                    await m_JobsSema.WaitAsync();
                    IEnumerable<string> dirs;
                    try {
                        IEnumerable<string> files = Directory.GetFiles(directory, "*.sln");
                        foreach (string file in files) {
                            bool process = await solutionRule.ShouldProcessSolutionAsync(file);
                            if (process) paths.Enqueue(file);
                        }

                        dirs = Directory.GetDirectories(directory);
                    } finally {
                        m_JobsSema.Release();
                    }

                    ParallelOptions options = new() {
                        MaxDegreeOfParallelism = (m_Jobs > 0) ? m_Jobs : -1
                    };
                    await Parallel.ForEachAsync(dirs, options, async (dir, token) => {
                        await GetPathsAsync(dir, solutionRule, paths);
                    });
                } finally {
                    if (ownsDotSolution) solutionRule.Dispose();
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (disposing) {
                    m_JobsSema.Dispose();
                }
            }
        }
    }
}
