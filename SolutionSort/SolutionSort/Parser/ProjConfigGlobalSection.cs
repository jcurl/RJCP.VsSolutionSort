namespace RJCP.VsSolutionSort.Parser
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The global section <c>GlobalSection(ProjectConfigurationPlatforms)</c>.
    /// </summary>
    internal class ProjConfigGlobalSection : GlobalSection
    {
        private readonly Dictionary<Guid, IReadOnlyList<ProjConfig>> m_KeyConfigs = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjConfigGlobalSection"/> class.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="section">The section name.</param>
        /// <param name="solution">The solution load qualifier.</param>
        public ProjConfigGlobalSection(string line, string section, string solution)
            : base(line, section, solution) { }

        internal void Add(ProjConfig config)
        {
            if (m_KeyConfigs.TryGetValue(config.Element, out IReadOnlyList<ProjConfig> lines)) {
                if (lines is null) {
                    // This shouldn't happen, unless the element was somehow added without new initialisation, which the
                    // 'else' statement does.
                    lines = new List<ProjConfig>();
                    m_KeyConfigs[config.Element] = lines;
                }
            } else {
                lines = new List<ProjConfig>();
                m_KeyConfigs.Add(config.Element, lines);
            }
            ((List<ProjConfig>)lines).Add(config);
            AddLine(config);
        }

        /// <summary>
        /// Gets the project mapping a <see cref="Guid"/> to a read only list of <see cref="ProjConfig"/>.
        /// </summary>
        /// <value>The project map.</value>
        /// <remarks>
        /// This allows mapping a GUID to the configuration lines in the solution file.
        /// </remarks>
        public IReadOnlyDictionary<Guid, IReadOnlyList<ProjConfig>> ProjectMap { get { return m_KeyConfigs; } }
    }
}
