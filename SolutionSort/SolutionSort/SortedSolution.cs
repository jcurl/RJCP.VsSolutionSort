namespace RJCP.VsSolutionSort
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Parser;

    /// <summary>
    /// A decomposed Solution File, with the task of sorting the projects by name.
    /// </summary>
    public class SortedSolution
    {
        private readonly Solution m_Solution = new();
        private List<Project> m_SortedProjects;

        /// <summary>
        /// Load the solution as an asynchronous operation.
        /// </summary>
        /// <param name="fileName">Name of the solution file.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method is not thread-safe, you should not access this object until the method returns, and you should
        /// not call this more than once until it returns.
        /// </remarks>
        /// <exception cref="SolutionFormatException">There was an error while parsing the solution file.</exception>
        public async Task LoadAsync(string fileName)
        {
            if (m_Solution.Sections.Count > 0)
                throw new InvalidOperationException("Solution already loaded");

            using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            await LoadAsync(fs);
        }

        /// <summary>
        /// Load the solution as an asynchronous operation.
        /// </summary>
        /// <param name="file">The stream that is the start of the solution file.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="SolutionFormatException">There was an error while parsing the solution file.</exception>
        /// <remarks>
        /// This method is not thread-safe, you should not access this object unti the method returns, and you should
        /// not call this more than once until it returns.
        /// </remarks>
        /// <exception cref="SolutionFormatException">There was an error while parsing the solution file.</exception>
        public async Task LoadAsync(Stream file)
        {
            if (m_Solution.Sections.Count > 0)
                throw new InvalidOperationException("Solution already loaded");

            using var reader = new StreamReader(file);
            await LoadAsync(reader);
        }

        /// <summary>
        /// Load the solution as an asynchronous operation.
        /// </summary>
        /// <param name="reader">The reader that reads the solution file.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="SolutionFormatException">There was an error while parsing the solution file.</exception>
        /// <remarks>
        /// This method is not thread-safe, you should not access this object unti the method returns, and you should
        /// not call this more than once until it returns.
        /// </remarks>
        public async Task LoadAsync(TextReader reader)
        {
            if (m_Solution.Sections.Count > 0)
                throw new InvalidOperationException("Solution already loaded");

            await m_Solution.LoadAsync(reader);

            Projects projectsSection = FindProjects(m_Solution)
                ?? throw new SolutionFormatException("No projects found in the solution file");
            NestedProjGlobalSection nestedProjectSection = FindGlobalSection<NestedProjGlobalSection>(m_Solution);

            m_SortedProjects = GetSortedElements(projectsSection, nestedProjectSection);
        }

        private static Projects FindProjects(Solution solution)
        {
            Projects found = null;

            foreach (ISection section in solution.Sections) {
                if (section is Projects projects) {
                    if (found is not null)
                        throw new SolutionFormatException("Multiple Project sections found");
                    found = projects;
                }
            }
            return found;
        }

        private static T FindGlobalSection<T>(Solution solution) where T : GlobalSection
        {
            T found = null;

            foreach (ISection section in solution.Sections) {
                if (section is Global global) {
                    foreach (ISection globalSection in global.Sections) {
                        if (globalSection.GetType() == typeof(T)) {
                            if (found is not null)
                                throw new SolutionFormatException("Multiple GlobalSections found of the same type");
                            found = (T)globalSection;
                        }
                    }
                }
            }
            return found;
        }

        #region Sorting
        private sealed class ProjectComparer : IComparer<Project>
        {
            public int Compare(Project x, Project y)
            {
                if (x is null && y is null) return 0;
                if (x is null) return -1;
                if (y is null) return 1;

                if (x.ProjectType == SolutionProjectTypes.Folder && y.ProjectType != SolutionProjectTypes.Folder) return -1;
                if (x.ProjectType != SolutionProjectTypes.Folder && y.ProjectType == SolutionProjectTypes.Folder) return 1;
                return string.Compare(x.Key, y.Key, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        private readonly ProjectComparer ProjectCompare = new();

        private List<Project> GetSortedElements(Projects projects, NestedProjGlobalSection nested)
        {
            List<Project> sorted = new();

            var roots = GetRootElements(projects, nested);
            foreach (var root in roots) {
                sorted.Add(root);

                // If the 'nestedProjectSection' is empty, we sort based on the projects only.
                if (nested is not null)
                    AddSortedChildren(root, projects, nested, sorted);
            }

            if (sorted.Count == 0 && projects.ProjectList.Count != 0) {
                throw new SolutionFormatException("Error sorting projects. Nested Global Section is broken (no root projects entries found).");
            }

            if (sorted.Count != projects.ProjectList.Count) {
                // If we have a cyclic graph in the nested structure (project = parent), then a project entry points to
                // a key that eventually points back to itself and never the parent. As we enumerate from only those
                // items reachable from the parent, we'll never get into the cycle, but just find we've found less
                // projects than what is defined.
                throw new SolutionFormatException("Error sorting projects. Nested Global Section is broken (probably cycles in structure).");
            }
            return sorted;
        }

        private IEnumerable<Project> GetRootElements(Projects projects, NestedProjGlobalSection nested)
        {
            if (nested is null) return projects.ProjectList.OrderBy(i => i.Key);

            var nestedGuids =
                from item in nested.Lines
                where item is NestedProj
                select ((NestedProj)item).Element;

            // Return all the project entries that don't have an entry in the nested.Lines.
            var roots =
                from project in projects.ProjectList
                join nestedGuid in nestedGuids on project.Element equals nestedGuid into t
                from guid in t.DefaultIfEmpty()
                where guid == Guid.Empty
                select project;

            return roots.OrderBy(p => p, ProjectCompare);
        }

        private void AddSortedChildren(Project project, Projects projects, NestedProjGlobalSection nested, List<Project> sorted)
        {
            var children = GetChildren(project, projects, nested);
            foreach (var child in children) {
                sorted.Add(child);
                AddSortedChildren(child, projects, nested, sorted);
            }
        }

        private IEnumerable<Project> GetChildren(Project project, Projects projects, NestedProjGlobalSection nested)
        {
            var childrenGuids =
                from item in nested.Lines
                where (item is NestedProj nestedItem) && nestedItem.Parent == project.Element
                select ((NestedProj)item).Element;
            var children =
                from childId in childrenGuids
                join projectItem in projects.ProjectList on childId equals projectItem.Element
                select projectItem;

            return children.OrderBy(p => p, ProjectCompare);
        }
        #endregion

        public async Task WriteAsync(string fileName)
        {
            if (m_Solution.Sections.Count == 0)
                throw new InvalidOperationException("Solution not loaded");

            using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            await WriteAsync(fs);
        }

        public async Task WriteAsync(Stream file)
        {
            if (m_Solution.Sections.Count == 0)
                throw new InvalidOperationException("Solution not loaded");

            // Emit a Byte Order Mark.
            Encoding encoding = new UTF8Encoding(true);
            using var writer = new StreamWriter(file, encoding);
            await WriteAsync(writer);
        }

        public async Task WriteAsync(TextWriter writer)
        {
            if (m_Solution.Sections.Count == 0)
                throw new InvalidOperationException("Solution not loaded");

            foreach (ISection section in m_Solution.Sections) {
                if (section is TextBlock textBlock) {
                    foreach (Line line in textBlock.Lines) {
                        await writer.WriteLineAsync(line.ToString());
                    }
                } else if (section is Projects) {
                    // Instead of `projects.ProjectList`, we dump our sorted list
                    foreach (Project project in m_SortedProjects) {
                        await writer.WriteLineAsync(project.ToString());
                        foreach (Line line in project.Lines) {
                            await writer.WriteLineAsync(line.ToString());
                        }
                    }
                } else if (section is Global global) {
                    await writer.WriteLineAsync(global.ToString());
                    foreach (ISection globalSection in global.Sections) {
                        if (globalSection is TextBlock globalTextBlock) {
                            foreach (Line line in globalTextBlock.Lines) {
                                await writer.WriteLineAsync(line.ToString());
                            }
                        } else if (globalSection is ProjConfigGlobalSection configSection) {
                            await writer.WriteLineAsync(configSection.ToString());
                            foreach (Project project in m_SortedProjects) {
                                if (configSection.ProjectMap.TryGetValue(project.Element,
                                    out IReadOnlyList<ProjConfig> configLines)) {
                                    // If it doesn't exist in the config, it's not a project that is configurable.
                                    foreach (var line in configLines) {
                                        await writer.WriteLineAsync(line.ToString());
                                    }
                                }
                            }

                            // Write all lines that aren't for the configuration at the end. This contains the
                            // `EndGlobalSection`.
                            foreach (var x in configSection.Lines.Where(l => l is not ProjConfig)) {
                                await writer.WriteLineAsync(x.ToString());
                            }
                        } else if (globalSection is NestedProjGlobalSection nestedSection) {
                            await writer.WriteLineAsync(nestedSection.ToString());
                            foreach (Project project in m_SortedProjects) {
                                if (nestedSection.ProjectMap.TryGetValue(project.Element, out NestedProj nestedProj)) {
                                    // If it doesn't exist in the nested projects, it's a root element (has no parent)
                                    await writer.WriteLineAsync(nestedProj.ToString());
                                }
                            }

                            // Write all lines that aren't for the configuration at the end. This contains the
                            // `EndGlobalSection`.
                            foreach (var x in nestedSection.Lines.Where(l => l is not NestedProj)) {
                                await writer.WriteLineAsync(x.ToString());
                            }
                        } else if (globalSection is GlobalSection genericSection) {
                            await writer.WriteLineAsync(genericSection.ToString());
                            foreach (Line line in genericSection.Lines) {
                                await writer.WriteLineAsync(line.ToString());
                            }
                        }
                    }
                }
            }
        }
    }
}
