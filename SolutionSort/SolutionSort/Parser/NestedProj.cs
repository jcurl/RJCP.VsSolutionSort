namespace RJCP.VsSolutionSort.Parser
{
    using System;

    /// <summary>
    /// A single line, inside the <c>GlobalSection(NestedProjects)</c>.
    /// </summary>
    internal class NestedProj : Line
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NestedProj"/> class.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="element">The project GUID element.</param>
        /// <param name="parent">The parent for this project GUID element.</param>
        public NestedProj(string line, Guid element, Guid parent) : base(line)
        {
            Element = element;
            Parent = parent;
        }

        /// <summary>
        /// Gets the project GUID element.
        /// </summary>
        /// <value>The project GUID element.</value>
        public Guid Element { get; }

        /// <summary>
        /// Gets the parent for this project GUID element.
        /// </summary>
        /// <value>The parent for this project GUID element.</value>
        public Guid Parent { get; }
    }
}
