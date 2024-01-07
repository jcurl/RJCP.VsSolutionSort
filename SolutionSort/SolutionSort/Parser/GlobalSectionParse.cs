namespace RJCP.VsSolutionSort.Parser
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parses for <c>GlobalSection(name) = keyword</c>.
    /// </summary>
    internal class GlobalSectionParse : IParseLine<GlobalSection>
    {
        private readonly Regex Regex =
            new(@"^\s*GlobalSection\(\s*(\S+)\s*\)\s*=\s*(\S+)\s*$",
                RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <inheritdoc/>
        public GlobalSection Parse(string line)
        {
            Match m = Regex.Match(line);
            if (!m.Success) return null;

            return new GlobalSection(line, m.Groups[1].Value, m.Groups[2].Value);
        }

        object IParseLine.Parse(string line)
        {
            return Parse(line);
        }
    }
}
