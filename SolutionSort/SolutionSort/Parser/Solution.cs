namespace RJCP.VsSolutionSort.Parser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// A decomposed Solution File.
    /// </summary>
    internal class Solution
    {
        private readonly List<ISection> m_Sections = new();
        private bool m_HasGlobal;
        private bool m_HasGlobalSectionNested;
        private bool m_HasGlobalSectionProjConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="Solution"/> class.
        /// </summary>
        public Solution()
        {
            RegisterParser<LineParse, Line>(LineType.Line);
            RegisterParser<ProjectParse, Project>(LineType.Project);
            RegisterParser<EndProjectParse, Line>(LineType.EndProject);
            RegisterParser<GlobalParse, Global>(LineType.Global);
            RegisterParser<EndGlobalParse, Line>(LineType.EndGlobal);
            RegisterParser<GlobalSectionParse, GlobalSection>(LineType.GlobalSection);
            RegisterParser<EndGlobalSectionParse, Line>(LineType.EndGlobalSection);
            RegisterParser<ProjConfigParse, ProjConfig>(LineType.ProjectConfigurationPlatform);
            RegisterParser<NestedProjParse, NestedProj>(LineType.NestedProject);
        }

        private readonly Dictionary<LineType, IParseLine> m_Parsers = new();

        private void RegisterParser<TP, T>(LineType lineType) where TP : IParseLine<T>
        {
            if (!m_Parsers.ContainsKey(lineType)) {
                m_Parsers.Add(lineType, Activator.CreateInstance<TP>());
            }
        }

        /// <summary>
        /// Gets the sections that have been parsed.
        /// </summary>
        /// <value>The sections that have been parsed.</value>
        public IReadOnlyList<ISection> Sections { get { return m_Sections; } }

        /// <summary>
        /// Load the solution as an asynchronous operation.
        /// </summary>
        /// <param name="fileName">Name of the solution file.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="SolutionFormatException">There was an error while parsing the solution file.</exception>
        /// <remarks>
        /// This method is not thread-safe, you should not access this object unti the method returns, and you should
        /// not call this more than once until it returns.
        /// </remarks>
        public async Task LoadAsync(string fileName)
        {
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
            m_Sections.Clear();
            while (true) {
                string line = await reader.ReadLineAsync();
                if (line is null) {
                    CheckForDuplicateProjectValues(m_Sections);
                    return;
                }

                if (IsLineType(LineType.Project, line, out Project project)) {
                    await ParseProject(reader, project);
                } else if (IsLineType(LineType.Global, line, out Global global)) {
                    if (m_HasGlobal)
                        throw new SolutionFormatException("Only one Global section is allowed.");
                    m_HasGlobal = true;

                    await ParseGlobal(reader, global);
                } else if (IsLineType(LineType.Line, line, out Line text)) {
                    TextBlock block = m_Sections.GetSection<TextBlock>();
                    block.Lines.Add(text);
                }
            }
        }

        private static void CheckForDuplicateProjectValues(List<ISection> sections)
        {
            // This isn't an error in the Solution File, but Visual Studio won't open the file none-the-less.
            HashSet<string> path = new();

            var projects = GetProjects(sections);
            if (projects is null) return;

            foreach (Project project in projects) {
                if (path.Contains(project.Value))
                    throw new SolutionFormatException("Multiple projects of the same solution path found");
                path.Add(project.Value);
            }
        }

        private static IEnumerable<Project> GetProjects(List<ISection> sections)
        {
            Projects projects = null;

            var projectsList =
                from section in sections
                where section is Projects
                select (Projects)section;
            switch (projectsList.Count()) {
            case 0: return null;
            case 1:
                projects = projectsList.First();
                break;
            default:
                throw new SolutionFormatException("Multiple Project sections found");
            }

            // Ignore folders.
            return
                from project in projects.ProjectList
                where project.ProjectType != SolutionProjectTypes.Folder
                select project;
        }

        /// <summary>
        /// Checks if the <paramref name="line"/> can be parsed by <see cref="LineType"/>.
        /// </summary>
        /// <typeparam name="T">The type of the output token.</typeparam>
        /// <param name="lineType">Type of the line token that is expected.</param>
        /// <param name="line">The line to parse.</param>
        /// <param name="token">The token if parsing was successful.</param>
        /// <returns>
        /// Returns <see langword="true"/> if parsing was successful; otherwise, <see langword="false"/>.
        /// </returns>
        private bool IsLineType<T>(LineType lineType, string line, out T token) where T : class
        {
            token = null;

            if (!m_Parsers.TryGetValue(lineType, out IParseLine parser))
                return false;

            token = (T)parser.Parse(line);
            return token is not null;
        }

        /// <summary>
        /// Parses the <c>Project</c> to <c>EndProject</c> section.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="project">The project block to put the results into.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="SolutionFormatException">
        /// There was an error parsing the solution file. The same project is detected multiple times.
        /// </exception>
        /// <remarks>
        /// Other than the line <c>Project</c>, nothing else in-between must be parsed. Thus <see cref="Project.Lines"/>
        /// is the content of the entire block, minus the first line (which is the <see cref="Project"/> element
        /// itself).
        /// </remarks>
        private async Task ParseProject(TextReader reader, Project project)
        {
            while (true) {
                string line = await reader.ReadLineAsync();
                if (line is null) return;

                if (IsLineType(LineType.EndProject, line, out Line endProject)) {
                    project.AddLine(endProject);
                    Projects projects = m_Sections.GetSection<Projects>();
                    if (!projects.TryAdd(project))
                        throw new SolutionFormatException($"Project {project.Key} with GUID {project.Element} listed more than once.");
                    return;
                } else if (IsLineType(LineType.Line, line, out Line text)) {
                    project.AddLine(text);
                }
            }
        }

        /// <summary>
        /// Parses the <c>Global</c> to <c>EndGlobal</c> section.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="global">The global block to put the results into.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="SolutionFormatException">There was an error parsing the solution file.</exception>
        private async Task ParseGlobal(TextReader reader, Global global)
        {
            while (true) {
                string line = await reader.ReadLineAsync()
                    ?? throw new SolutionFormatException($"Solution file reached EOF unexpectedly");
                if (IsLineType(LineType.EndGlobal, line, out Line endGlobal)) {
                    TextBlock block = global.GetSection<TextBlock>();
                    block.Lines.Add(endGlobal);
                    m_Sections.Add(global);
                    return;
                } else if (IsLineType(LineType.GlobalSection, line, out GlobalSection globalSection)) {
                    await ParseGlobalSection(reader, global, globalSection);
                } else if (IsLineType(LineType.Line, line, out Line text)) {
                    TextBlock block = global.GetSection<TextBlock>();
                    block.Lines.Add(text);
                }
            }
        }

        /// <summary>
        /// Parses the <c>GlobalSection</c> to <c>EndGlobalSection</c> section.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="global">The global block to put the result into when finished.</param>
        /// <param name="section">The global section that was parsed.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="SolutionFormatException">There was an error parsing the solution file.</exception>
        private async Task ParseGlobalSection(TextReader reader, Global global, GlobalSection section)
        {
            switch (section.Section) {
            case "ProjectConfigurationPlatforms":
                if (m_HasGlobalSectionProjConfig)
                    throw new SolutionFormatException("Only one GlobalSection(ProjectConfigurationPlatforms) is allowed.");
                m_HasGlobalSectionProjConfig = true;

                await ParseProjConfigGlobalSection(reader, global,
                    new ProjConfigGlobalSection(section.ToString(), section.Section, section.Solution));
                return;
            case "NestedProjects":
                if (m_HasGlobalSectionNested)
                    throw new SolutionFormatException("Only one GlobalSection(NestedProjects) is allowed.");
                m_HasGlobalSectionNested = true;

                await ParseNestedProjGlobalSection(reader, global,
                    new NestedProjGlobalSection(section.ToString(), section.Section, section.Solution));
                return;
            }

            while (true) {
                string line = await reader.ReadLineAsync()
                    ?? throw new SolutionFormatException($"Solution file reached EOF unexpectedly");
                if (IsLineType(LineType.EndGlobalSection, line, out Line endGlobalSection)) {
                    section.AddLine(endGlobalSection);
                    global.AddSection(section);
                    return;
                } else if (IsLineType(LineType.Line, line, out Line text)) {
                    section.AddLine(text);
                }
            }
        }

        /// <summary>
        /// Parses the <c>GlobalSection()</c> to <c>EndGlobalSection</c> section.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="global">The global block to put the result into when finished.</param>
        /// <param name="section">The global section that was parsed.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="SolutionFormatException">There was an error parsing the solution file.</exception>
        private async Task ParseProjConfigGlobalSection(TextReader reader, Global global, ProjConfigGlobalSection section)
        {
            while (true) {
                string line = await reader.ReadLineAsync()
                    ?? throw new SolutionFormatException($"Solution file reached EOF unexpectedly");
                if (IsLineType(LineType.EndGlobalSection, line, out Line endGlobalSection)) {
                    section.AddLine(endGlobalSection);
                    global.AddSection(section);
                    return;
                } else if (IsLineType(LineType.ProjectConfigurationPlatform, line, out ProjConfig config)) {
                    section.Add(config);
                } else if (IsLineType(LineType.Line, line, out Line _)) {
                    // We only expect to have the "config" type lines here.
                    throw new SolutionFormatException($"Unexpected line in GlobalSection({section.Section})");
                }
            }
        }

        /// <summary>
        /// Parses the <c>GlobalSection()</c> to <c>EndGlobalSection</c> section.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="global">The global block to put the result into when finished.</param>
        /// <param name="section">The global section that was parsed.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="SolutionFormatException">There was an error parsing the solution file.</exception>
        private async Task ParseNestedProjGlobalSection(TextReader reader, Global global, NestedProjGlobalSection section)
        {
            while (true) {
                string line = await reader.ReadLineAsync()
                    ?? throw new SolutionFormatException($"Solution file reached EOF unexpectedly");
                if (IsLineType(LineType.EndGlobalSection, line, out Line endGlobalSection)) {
                    section.AddLine(endGlobalSection);
                    global.AddSection(section);
                    return;
                } else if (IsLineType(LineType.NestedProject, line, out NestedProj config)) {
                    if (!section.TryAdd(config))
                        throw new SolutionFormatException($"Duplicate project identifier {config.Element} in GlobalSection({section.Section})");
                } else if (IsLineType(LineType.Line, line, out Line _)) {
                    // We only expect to have the "config" type lines here.
                    throw new SolutionFormatException($"Unexpected line in GlobalSection({section.Section})");
                }
            }
        }
    }
}
