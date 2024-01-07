namespace RJCP.VsSolutionSort.Parser
{
    using System;

    /// <summary>
    /// Represents a line in the Visual Studio Solution File.
    /// </summary>
    internal class Line
    {
        private readonly string m_Line;

        /// <summary>
        /// Initializes a new instance of the <see cref="Line"/> class.
        /// </summary>
        /// <param name="line">The line that was read from the Visual Studio Solution.</param>
        /// <exception cref="ArgumentNullException"><paramref name="line"/> is <see langword="null"/>.</exception>
        public Line(string line)
        {
            ArgumentNullException.ThrowIfNull(line);
            m_Line = line;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return m_Line;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return m_Line.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is string text) {
                return m_Line.Equals(text);
            } else if (obj is Line line) {
                return m_Line.Equals(line.m_Line);
            }
            return false;
        }
    }
}
