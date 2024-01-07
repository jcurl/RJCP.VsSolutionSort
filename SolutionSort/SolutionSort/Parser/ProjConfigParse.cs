namespace RJCP.VsSolutionSort.Parser
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parses a line inside of a global section <c>ProjectConfigurationPlatforms</c>.
    /// </summary>
    internal class ProjConfigParse : IParseLine<ProjConfig>
    {
        private readonly Regex Regex =
            new(@"^\s*\{(\S+)\}.+\s*$",
                RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <inheritdoc/>
        public ProjConfig Parse(string line)
        {
            Match m = Regex.Match(line);
            if (!m.Success) return null;

            return new ProjConfig(line, new Guid(m.Groups[1].Value));
        }

        object IParseLine.Parse(string line)
        {
            return Parse(line);
        }
    }
}
