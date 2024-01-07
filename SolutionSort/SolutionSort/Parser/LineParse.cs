namespace RJCP.VsSolutionSort.Parser
{
    /// <summary>
    /// Always returns an object <see cref="Line"/>.
    /// </summary>
    internal class LineParse : IParseLine<Line>
    {
        /// <inheritdoc/>
        public Line Parse(string line)
        {
            if (line == null) return null;
            return new Line(line);
        }

        object IParseLine.Parse(string line)
        {
            return Parse(line);
        }
    }
}
