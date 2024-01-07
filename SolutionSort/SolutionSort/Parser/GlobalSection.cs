namespace RJCP.VsSolutionSort.Parser
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents all sections within the keywords <c>GlobalSection</c> .. <c>EndGlobalSection</c>.
    /// </summary>
    /// <remarks>
    /// The lines that belong to this block can be obtained with <see cref="Lines"/>. Note, that this is also a line,
    /// and is the first line.
    /// </remarks>
    internal class GlobalSection : Line, ISection
    {
        private readonly List<Line> m_Lines = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalSection"/> class.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="section">The section name.</param>
        /// <param name="solution">The solution load qualifier.</param>
        public GlobalSection(string line, string section, string solution) : base(line)
        {
            Section = section;
            Solution = solution;
        }

        /// <summary>
        /// Gets the section name.
        /// </summary>
        /// <value>The section name.</value>
        public string Section { get; }

        /// <summary>
        /// Gets the solution load qualifier.
        /// </summary>
        /// <value>The solution load qualifier.</value>
        public string Solution { get; }

        /// <summary>
        /// Gets the lines that occur immediately after this line, until the end.
        /// </summary>
        /// <value>The lines that occur immediately after this line.</value>
        public IReadOnlyList<Line> Lines { get { return m_Lines; } }

        internal void AddLine(Line line)
        {
            if (line != null) m_Lines.Add(line);
        }
    }
}
