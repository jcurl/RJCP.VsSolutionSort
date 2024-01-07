namespace RJCP.VsSolutionSort.Parser
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines a project <c>Project</c> line.
    /// </summary>
    internal class Project : Line
    {
        private readonly List<Line> m_Lines = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="projectType">Type of the project.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="element">The project GUID element.</param>
        public Project(string line, Guid projectType, string key, string value, Guid element) : base(line)
        {
            ProjectType = projectType;
            Element = element;
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Gets the type of the project.
        /// </summary>
        /// <value>The type of the project.</value>
        public Guid ProjectType { get; }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>The key.</value>
        public string Key { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; }

        /// <summary>
        /// Gets the project GUID element.
        /// </summary>
        /// <value>The project GUID element.</value>
        public Guid Element { get; }

        internal void AddLine(Line line)
        {
            if (line != null) m_Lines.Add(line);
        }

        /// <summary>
        /// Gets the lines of this section.
        /// </summary>
        /// <value>The lines of this section.</value>
        /// <remarks>
        /// Contains all the lines in this section in order (not including the first line, which is reprsented by this
        /// object, <see cref="Project"/>).
        /// </remarks>
        public IReadOnlyList<Line> Lines { get { return m_Lines; } }
    }
}
