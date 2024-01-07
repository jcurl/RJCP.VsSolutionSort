namespace RJCP.VsSolutionSort.Parser
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A collection of <see cref="Project"/> sections, in the order that it's parsed.
    /// </summary>
    internal class Projects : ISection
    {
        private readonly List<Project> m_Projects = new();

        private readonly Dictionary<Guid, Project> m_ProjectMap = new();

        internal bool TryAdd(Project project)
        {
            if (m_ProjectMap.TryAdd(project.Element, project)) {
                m_Projects.Add(project);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the project list.
        /// </summary>
        /// <value>The project list.</value>
        public IReadOnlyList<Project> ProjectList { get { return m_Projects; } }

        /// <summary>
        /// Gets the project mapping a <see cref="Guid"/> to a <see cref="Project"/>.
        /// </summary>
        /// <value>The project map.</value>
        /// <remarks>
        /// This allows mapping a GUID to a project key and value.
        /// </remarks>
        public IReadOnlyDictionary<Guid, Project> ProjectMap { get { return m_ProjectMap; } }
    }
}
