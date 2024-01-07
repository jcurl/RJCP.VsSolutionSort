namespace RJCP.VsSolutionSort.Parser
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a collection of lines.
    /// </summary>
    internal class TextBlock : ISection
    {
        private readonly List<Line> m_Lines = new();

        /// <summary>
        /// Gets the ordered list of lines in this block.
        /// </summary>
        /// <value>The lines.</value>
        public ICollection<Line> Lines { get { return m_Lines; } }
    }
}
