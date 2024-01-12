namespace RJCP.VsSolutionSort.Parser
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parses the line <c>Project("{GUID}") = KEY, VALUE, {GUID}</c>.
    /// </summary>
    internal class ProjectParse : IParseLine<Project>
    {
        private readonly Regex Regex =
            new(@"^\s*Project\(""\{(\S+)\}""\)\s*=\s*""(.+)""\s*,\s*""(.+)""\s*,\s*""\{(\S+)\}""\s*$",
                RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <inheritdoc/>
        public Project Parse(string line)
        {
            Match m = Regex.Match(line);
            if (!m.Success) return null;

            return new Project(line, new Guid(m.Groups[1].Value), m.Groups[2].Value, m.Groups[3].Value, new Guid(m.Groups[4].Value));
        }

        object IParseLine.Parse(string line)
        {
            return Parse(line);
        }
    }
}
