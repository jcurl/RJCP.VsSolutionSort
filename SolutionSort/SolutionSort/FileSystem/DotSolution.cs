namespace RJCP.VsSolutionSort.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Reads and parses a file `.solutionsort`.
    /// </summary>
    internal sealed class DotSolution : IDisposable
    {
        private readonly string m_FileName;
        private readonly List<Regex> m_Include = new();
        private readonly List<Regex> m_Exclude = new();

        const int NotInitialized = 0;
        const int Initialized = 1;
        const int InitializedError = 2;
        private int m_Initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotSolution"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor always assumes that a solution file should be processed.
        /// </remarks>
        public DotSolution()
        {
            m_FileName = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotSolution"/> class.
        /// </summary>
        /// <param name="fileName">Name of the <c>.solutionsort</c> file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This file is not loaded in the constructor.
        /// </remarks>
        public DotSolution(string fileName)
        {
            ArgumentNullException.ThrowIfNull(fileName);
            m_FileName = fileName;
        }

        /// <summary>
        /// Checks if the solution file name should be processed based on the <c>.solutionsort</c> rules.
        /// </summary>
        /// <param name="solutionFile">The solution file that should be checked.</param>
        /// <returns>
        /// Returns <see langword="true"/> if the solution file should be processed, <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="solutionFile"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method lazy loads the <c>.solutionsort</c> file (as it should not be done in the constructor). It is
        /// assumed that this method can be called asynchronously with other threads at the same time. Loading the file
        /// is synchronised.
        /// </remarks>
        public async Task<bool> ShouldProcessSolutionAsync(string solutionFile)
        {
            ArgumentNullException.ThrowIfNull(solutionFile);
            if (m_FileName is null) return true;

            switch (GetInitialized()) {
            case InitializedError: return false;
            case NotInitialized:
                if (!await ReadDotFileAsync()) return false;
                break;
            }

            string solutionFileNameOnly = Path.GetFileName(solutionFile);
            if (m_Exclude.Count == 0) {
                // There are no excludes, so the result is only if there is a matching include.
                foreach (Regex r in m_Include) {
                    if (r.IsMatch(solutionFileNameOnly)) return true;
                }
                return false;
            }

            bool includeMatch = m_Include.Count == 0;
            if (!includeMatch) {
                // There are excludes. If there are includes, we filter by them first, else we assume to always include.
                foreach (Regex r in m_Include) {
                    if (r.IsMatch(solutionFileNameOnly)) {
                        includeMatch = true;
                        break;
                    }
                }
                if (!includeMatch) return false;
            }

            // Finally, if it's in the exclude list, we don't include it, and if not, it's in the include list (or there
            // was no include list) and then we allow the file to be processed.
            foreach (Regex r in m_Exclude) {
                if (r.IsMatch(solutionFileNameOnly)) return false;
            }
            return true;
        }

        private int GetInitialized() { return Thread.VolatileRead(ref m_Initialized); }

        private readonly SemaphoreSlim m_FileSemaphore = new(1);

        private async Task<bool> ReadDotFileAsync()
        {
            switch (GetInitialized()) {
            case Initialized: return true;
            case InitializedError: return false;
            }

            await m_FileSemaphore.WaitAsync();
            try {
                bool inclusion = true;
                using FileStream fs = new(m_FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                using StreamReader reader = new(fs, Encoding.UTF8);
                while (true) {
                    string line = reader.ReadLine();
                    if (line is null) break;
                    line = line.Trim();

                    // Comments must start with a hash in column 0.
                    if (line.Length > 0 && line[0] == '#') continue;

                    if (line.Equals("[include]", StringComparison.InvariantCultureIgnoreCase)) {
                        inclusion = true;
                    } else if (line.Equals("[exclude]", StringComparison.InvariantCultureIgnoreCase)) {
                        inclusion = false;
                    } else {
                        Regex r = new(line,
                            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline |
                            RegexOptions.IgnoreCase);
                        if (inclusion) {
                            m_Include.Add(r);
                        } else {
                            m_Exclude.Add(r);
                        }
                    }
                }
            } catch (ArgumentException) {
                // Regex compilation error.
                Thread.VolatileWrite(ref m_Initialized, InitializedError);
                return false;
            } catch (IOException) {
                // IOException, FielNotFoundException, DirectoryNotFoundException, PathTooLongException
                Thread.VolatileWrite(ref m_Initialized, InitializedError);
                return false;
            } catch (SecurityException) {
                // FileStream exception
                Thread.VolatileWrite(ref m_Initialized, InitializedError);
                return false;
            } catch (UnauthorizedAccessException) {
                Thread.VolatileWrite(ref m_Initialized, InitializedError);
                return false;
            } catch (NotSupportedException) {
                Thread.VolatileWrite(ref m_Initialized, InitializedError);
                return false;
            } catch (Exception) {
                Thread.VolatileWrite(ref m_Initialized, InitializedError);
                return false;
            } finally {
                Thread.VolatileWrite(ref m_Initialized, Initialized);
                m_FileSemaphore.Release();
            }
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing) {
                m_FileSemaphore.Dispose();
            }
        }
    }
}
