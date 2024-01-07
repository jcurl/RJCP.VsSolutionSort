namespace RJCP.VsSolutionSort.Parser
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parses a line inside of a global section <c>NestedProjects</c>.
    /// </summary>
    internal class NestedProjParse : IParseLine<NestedProj>
    {
        private readonly Regex Regex =
            new(@"^\s*\{(\S+)\}\s*=\s*\{(\S+)\}\s*$",
                RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <inheritdoc/>
        public NestedProj Parse(string line)
        {
            Match m = Regex.Match(line);
            if (!m.Success) return null;

            return new NestedProj(line, new Guid(m.Groups[1].Value), new Guid(m.Groups[2].Value));
        }

        object IParseLine.Parse(string line)
        {
            return Parse(line);
        }
    }
}
