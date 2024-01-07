namespace RJCP.VsSolutionSort.Parser
{
    using System;

    /// <summary>
    /// A single line, inside the <c>GlobalSection(ProjectConfigurationPlatforms)</c>.
    /// </summary>
    internal class ProjConfig : Line
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjConfig"/> class.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="element">The project GUID element.</param>
        public ProjConfig(string line, Guid element) : base(line)
        {
            Element = element;
        }

        /// <summary>
        /// Gets the element.
        /// </summary>
        /// <value>The project GUID element.</value>
        public Guid Element { get; }
    }
}
