namespace RJCP.VsSolutionSort.Parser
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents all sections within the keywords <c>Global</c> .. <c>EndGlobal</c>.
    /// </summary>
    /// <remarks>
    /// Each section (either a global section, or a text block) is in the <see cref="Sections"/>.
    /// </remarks>
    internal class Global : Line, ISection
    {
        private readonly List<ISection> m_Sections = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Global"/> class.
        /// </summary>
        /// <param name="line">The line that was read from the Visual Studio Solution.</param>
        public Global(string line) : base(line) { }

        /// <summary>
        /// The list of sections in order.
        /// </summary>
        public IReadOnlyList<ISection> Sections { get { return m_Sections; } }

        internal T GetSection<T>() where T : ISection
        {
            return m_Sections.GetSection<T>();
        }

        internal void AddSection(ISection section)
        {
            m_Sections.Add(section);
        }
    }
}
