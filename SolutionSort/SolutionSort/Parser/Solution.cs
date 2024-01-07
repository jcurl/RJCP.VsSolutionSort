namespace RJCP.VsSolutionSort.Parser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// A decomposed Solution File.
    /// </summary>
    internal class Solution
    {
        private readonly List<ISection> m_Sections = new();

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
        /// <returns>
        /// Returns <see langword="true"/> if parsing was successful; otherwise <see langword="false"/>.
        /// </returns>
        public async Task<bool> LoadAsync(string fileName)
        {
            using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            return await LoadAsync(fs);
        }

        /// <summary>
        /// Load the solution as an asynchronous operation.
        /// </summary>
        /// <param name="file">The stream that is the start of the solution file.</param>
        /// <returns>
        /// Returns <see langword="true"/> if parsing was successful; otherwise <see langword="false"/>.
        /// </returns>
        public async Task<bool> LoadAsync(Stream file)
        {
            using var reader = new StreamReader(file);
            return await LoadAsync(reader);
        }

        /// <summary>
        /// Load the solution as an asynchronous operation.
        /// </summary>
        /// <param name="reader">The reader that reads the solution file.</param>
        /// <returns>
        /// Returns <see langword="true"/> if parsing was successful; otherwise <see langword="false"/>.
        /// </returns>
        public async Task<bool> LoadAsync(TextReader reader)
        {
            m_Sections.Clear();
            while (true) {
                string line = await reader.ReadLineAsync();
                if (line == null) return true;

                if (IsLineType(LineType.Project, line, out Project project)) {
                    if (!await ParseProject(reader, project)) return false;
                } else if (IsLineType(LineType.Global, line, out Global global)) {
                    if (!await ParseGlobal(reader, global)) return false;
                } else if (IsLineType(LineType.Line, line, out Line text)) {
                    TextBlock block = GetSection<TextBlock>();
                    block.Lines.Add(text);
                }
            }
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
            return token != null;
        }

        /// <summary>
        /// Gets the section from the root section list if it is the last entry, or create a new section.
        /// </summary>
        /// <typeparam name="T">The type of section that is required.</typeparam>
        /// <param name="sections">The list of sections.</param>
        /// <returns>The last section if it is the last entry, or a newly created section.</returns>
        private T GetSection<T>() where T : ISection
        {
            return m_Sections.GetSection<T>();
        }

        /// <summary>
        /// Parses the <c>Project</c> to <c>EndProject</c> section.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="project">The project block to put the results into.</param>
        /// <returns>
        /// Returns <see langword="true"/> if the block was successfully parsed, <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// Other than the line <c>Project</c>, nothing else in-between must be parsed. Thus <see cref="Project.Lines"/>
        /// is the content of the entire block, minus the first line (which is the <see cref="Project"/> element
        /// itself).
        /// </remarks>
        private async Task<bool> ParseProject(TextReader reader, Project project)
        {
            while (true) {
                string line = await reader.ReadLineAsync();
                if (line == null) return false;

                if (IsLineType(LineType.EndProject, line, out Line endProject)) {
                    project.AddLine(endProject);
                    Projects projects = GetSection<Projects>();
                    return projects.TryAdd(project);
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
        /// <returns>
        /// Returns <see langword="true"/> if the block was successfully parsed, <see langword="false"/> otherwise.
        /// </returns>
        private async Task<bool> ParseGlobal(TextReader reader, Global global)
        {
            while (true) {
                string line = await reader.ReadLineAsync();
                if (line == null) return false;

                if (IsLineType(LineType.EndGlobal, line, out Line endGlobal)) {
                    TextBlock block = global.GetSection<TextBlock>();
                    block.Lines.Add(endGlobal);
                    m_Sections.Add(global);
                    return true;
                } else if (IsLineType(LineType.GlobalSection, line, out GlobalSection globalSection)) {
                    if (!await ParseGlobalSection(reader, global, globalSection)) return false;
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
        /// <returns>
        /// Returns <see langword="true"/> if the block was successfully parsed, <see langword="false"/> otherwise.
        /// </returns>
        private async Task<bool> ParseGlobalSection(TextReader reader, Global global, GlobalSection section)
        {
            switch (section.Section) {
            case "ProjectConfigurationPlatforms":
                return await ParseProjConfigGlobalSection(reader, global,
                    new ProjConfigGlobalSection(section.ToString(), section.Section, section.Solution));
            case "NestedProjects":
                return await ParseNestedProjGlobalSection(reader, global,
                    new NestedProjGlobalSection(section.ToString(), section.Section, section.Solution));
            }

            while (true) {
                string line = await reader.ReadLineAsync();
                if (line == null) return false;

                if (IsLineType(LineType.EndGlobalSection, line, out Line endGlobalSection)) {
                    section.AddLine(endGlobalSection);
                    global.AddSection(section);
                    return true;
                } else if (IsLineType(LineType.Line, line, out Line text)) {
                    section.AddLine(text);
                }
            }
        }

        private async Task<bool> ParseProjConfigGlobalSection(TextReader reader, Global global, ProjConfigGlobalSection section)
        {
            while (true) {
                string line = await reader.ReadLineAsync();
                if (line == null) return false;

                if (IsLineType(LineType.EndGlobalSection, line, out Line endGlobalSection)) {
                    section.AddLine(endGlobalSection);
                    global.AddSection(section);
                    return true;
                } else if (IsLineType(LineType.ProjectConfigurationPlatform, line, out ProjConfig config)) {
                    section.Add(config);
                } else if (IsLineType(LineType.Line, line, out Line _)) {
                    // We only expect to have the "config" type lines here.
                    return false;
                }
            }
        }

        private async Task<bool> ParseNestedProjGlobalSection(TextReader reader, Global global, NestedProjGlobalSection section)
        {
            while (true) {
                string line = await reader.ReadLineAsync();
                if (line == null) return false;

                if (IsLineType(LineType.EndGlobalSection, line, out Line endGlobalSection)) {
                    section.AddLine(endGlobalSection);
                    global.AddSection(section);
                    return true;
                } else if (IsLineType(LineType.NestedProject, line, out NestedProj config)) {
                    section.Add(config);
                } else if (IsLineType(LineType.Line, line, out Line _)) {
                    // We only expect to have the "config" type lines here.
                    return false;
                }
            }
        }
    }
}
