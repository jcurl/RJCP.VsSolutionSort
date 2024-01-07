namespace RJCP.VsSolutionSort.Parser
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Useful extensions for sections.
    /// </summary>
    internal static class SectionList
    {
        /// <summary>
        /// Gets the section if it is the last entry, or create a new section.
        /// </summary>
        /// <typeparam name="T">The type of section that is required.</typeparam>
        /// <param name="sections">The list of sections.</param>
        /// <returns>The last section if it is the last entry, or a newly created section.</returns>
        public static T GetSection<T>(this List<ISection> sections) where T : ISection
        {
            if (sections.Count == 0 || sections[^1].GetType() != typeof(T)) {
                T section = Activator.CreateInstance<T>();
                sections.Add(section);
                return section;
            }
            return (T)sections[^1];
        }
    }
}
