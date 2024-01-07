namespace RJCP.VsSolutionSort.Parser
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The global section <c>GlobalSection(NestedProjects)</c>.
    /// </summary>
    internal class NestedProjGlobalSection : GlobalSection
    {
        private readonly Dictionary<Guid, NestedProj> m_NestedConfig = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="NestedProjGlobalSection"/> class.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="section">The section name.</param>
        /// <param name="solution">The solution load qualifier.</param>
        public NestedProjGlobalSection(string line, string section, string solution)
            : base(line, section, solution) { }

        internal bool TryAdd(NestedProj config)
        {
            // The element is expected to be only once in the configuration file.
            if (m_NestedConfig.TryAdd(config.Element, config)) {
                AddLine(config);
                return true;
            }
            return false;
        }
    }
}
