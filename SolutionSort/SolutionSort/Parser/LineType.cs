namespace RJCP.VsSolutionSort.Parser
{
    /// <summary>
    /// A type of line to parse in the Visual Studio project.
    /// </summary>
    /// <remarks>
    /// This enumeration is used in the <see cref="Solution"/> for mapping expected lines and parsers of type
    /// <see cref="IParseLine"/>.
    /// </remarks>
    internal enum LineType
    {
        /// <summary>
        /// An unstructured line.
        /// </summary>
        Line,
        /// <summary>
        /// A line of type <c>Project(...)</c>.
        /// </summary>
        Project,

        /// <summary>
        /// A line of type <c>EndProject</c>.
        /// </summary>
        EndProject,

        /// <summary>
        /// A line of type <c>Global</c>.
        /// </summary>
        Global,

        /// <summary>
        /// A line of type <c>EndGlobal</c>.
        /// </summary>
        EndGlobal,

        /// <summary>
        /// A line of type <c>GlobalSection(...) = ...</c>.
        /// </summary>
        GlobalSection,

        /// <summary>
        /// A line of type <c>EndGlobalSection</c>.
        /// </summary>
        EndGlobalSection,

        /// <summary>
        /// A line of type <c>GUID.config.x.y = value</c>.
        /// </summary>
        /// <remarks>This is used inside of <c>GlobalSection(ProjectConfigurationPlatform) = postSolution</c>.</remarks>
        ProjectConfigurationPlatform,

        /// <summary>
        /// A line of type <c>GUID = GUID</c>.
        /// </summary>
        /// <remarks>This is used inside of <c>GlobalSection(NestedProject) = preSolution</c>.</remarks>
        NestedProject
    }
}
