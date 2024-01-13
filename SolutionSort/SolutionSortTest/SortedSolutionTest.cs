namespace RJCP.VsSolutionSort
{
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using RJCP.CodeQuality.NUnitExtensions;

    [TestFixture]
    internal class SortedSolutionTest
    {
        private static IEnumerable Solutions()
        {
            yield return new TestCaseData("SingleProject.sln").SetName("SingleProject");
            yield return new TestCaseData("SingleProjectWithFolder.sln").SetName("SingleProjectWithFolder");
            yield return new TestCaseData("ProjectFolderWithSpace.sln").SetName("ProjectFolderWithSpace");
            yield return new TestCaseData("SingleProjectNoConfig.sln").SetName("SingleProjectNoConfig");
            yield return new TestCaseData("SingleProjectWithFolderNoConfig.sln").SetName("SingleProjectWithFolderNoConfig");
            yield return new TestCaseData("ProjectSection.sln").SetName("ProjectSection");
            yield return new TestCaseData("WithUnicodeNames.sln").SetName("WithUnicodeNames");
            yield return new TestCaseData("Roslyn.sln").SetName("Roslyn");
            yield return new TestCaseData("MissingNestedEntries.sln").SetName("MissingNestedEntries");
            yield return new TestCaseData("EmptyGlobal.sln").SetName("EmptyGlobal");
            yield return new TestCaseData("OnlyProjects.sln").SetName("OnlyProjects");
            yield return new TestCaseData("UnknownGlobalSectionText.sln").SetName("UnknownGlobalSectionText");
            yield return new TestCaseData("SingleProjectWithFolderProjectAtEnd.sln").SetName("SingleProjectWithFolderProjectAtEnd");
        }

        [TestCaseSource(nameof(Solutions))]
        public async Task SortedSolutions(string solution)
        {
            string sourceSolution = Path.Combine(Deploy.TestDirectory, "TestResources", "Solutions", solution);
            string sortedSolution = Path.Combine(Deploy.TestDirectory, "TestResources", "Solutions",
                $"{Path.GetFileNameWithoutExtension(solution)}Sorted.sln");

            // For cases when there is no change, just don't have a `xxxSorted.sln`.
            if (!File.Exists(sortedSolution)) sortedSolution = sourceSolution;

            // Read in the solution and write the result to a string (instead of writing to disk)
            SortedSolution source = new();
            await source.LoadAsync(sourceSolution);

            StringBuilder sb = new();
            using (StringWriter writer = new(sb)) {
                await source.WriteAsync(writer);
            }

            using StreamReader reader = new(sortedSolution);
            string sorted = await reader.ReadToEndAsync();

            Assert.That(sb.ToString(), Is.EqualTo(sorted));

            await CheckNoChangeAsync(sb.ToString());
        }

        private static async Task CheckNoChangeAsync(string solutionText)
        {
            using StringReader readerIn = new(solutionText);
            SortedSolution source = new();
            await source.LoadAsync(readerIn);

            StringBuilder sb = new();
            using (StringWriter writer = new(sb)) {
                await source.WriteAsync(writer);
            }

            Assert.That(sb.ToString(), Is.EqualTo(solutionText));
        }

        private static void TestErrorInSolution(string solution)
        {
            string fullSolution = Path.Combine(Deploy.TestDirectory, "TestResources", "Solutions", solution);
            SortedSolution source = new();
            Assert.That(async () => {
                await source.LoadAsync(fullSolution);
            }, Throws.TypeOf<SolutionFormatException>());
        }

        [Test]
        public void ErrorCyclicNestedGraphNoRoots()
        {
            TestErrorInSolution("Error_CyclicNestedNoRoots.sln");
        }

        [Test]
        public void ErrorCyclicNestedGraph()
        {
            // This section is a cyclic graph {2C468A6B-04E9-4CAE-9B6F-BE0A72B80CB4} =
            // {23449741-D725-4ADB-A5BB-B723B54A53EF} -> "RJCP.Diagnostics.Log" parent "Nested"
            // {13449741-D725-4ADB-A5BB-B723B54A53EF} = {2C468A6B-04E9-4CAE-9B6F-BE0A72B80CB4} -> "Apps" parent
            // ""RJCP.Diagnostics.Log {BF569315-F0E9-48E6-954E-57CD601B7A71} = {13449741-D725-4ADB-A5BB-B723B54A53EF} ->
            // "DltDump" parent "Apps" {3BBF6C35-E46E-43BC-950D-187277D2AD1D} = {BF569315-F0E9-48E6-954E-57CD601B7A71}
            // {886EB9CD-1958-4E85-BEB8-A44A3CC45D0B} = {BF569315-F0E9-48E6-954E-57CD601B7A71}
            // {9E11CDD3-C81C-472D-8919-C14FB9FB5139} = {BF569315-F0E9-48E6-954E-57CD601B7A71}
            // {23449741-D725-4ADB-A5BB-B723B54A53EF} = {BF569315-F0E9-48E6-954E-57CD601B7A71} -> "Nested" parent
            // "DltDump"
            TestErrorInSolution("Error_CyclicNested.sln");
        }

        [Test]
        public void ErrorNestedEntryTwice()
        {
            // An entry in the nested section has the key twice. VS2022 doesn't load the project.
            TestErrorInSolution("Error_NestedEntryTwice.sln");
        }

        [Test]
        public void ErrorNestedSectionEof()
        {
            // The file is truncated so that `GlobalSection(NestedProjects)` doesn't have an `EndGlobalSection`.
            TestErrorInSolution("Error_NestedTruncated.sln");
        }

        [Test]
        public void ErrorNestedInvalidLine()
        {
            // The file has an unparsable line between `GlobalSection(NestedProjects)` and `EndGlobalSection`.
            TestErrorInSolution("Error_NestedEntryInvalidLine.sln");
        }

        [Test]
        public void ErrorConfigInvalidLine()
        {
            // The file has an unparsable line between `GlobalSection(NestedProjects)` and `EndGlobalSection`.
            TestErrorInSolution("Error_ConfigEntryInvalidLine.sln");
        }

        [Test]
        public void ErrorGenericGlobalSectionEof()
        {
            // The file is truncated so that `GlobalSection(ExtensibilityGlobals)` doesn't have an `EndGlobalSection`.
            TestErrorInSolution("Error_NestedTruncated.sln");
        }

        [Test]
        public void ErrorProjectConfigurationSectionEof()
        {
            // The file is truncated so that `GlobalSection(ProjectConfigurationPlatforms)` doesn't have an
            // `EndGlobalSection`.
            TestErrorInSolution("Error_ProjConfigTruncated.sln");
        }

        [Test]
        public void ErrorGlobalEof()
        {
            // The file is truncated so that `Global` doesn't have an `EndGlobal`.
            TestErrorInSolution("Error_GlobalTruncated.sln");
        }

        [Test]
        public void ErrorTwoSolutions()
        {
            // There are two separate solutions concatenated.
            TestErrorInSolution("Error_TwoBlocks.sln");
        }

        [Test]
        public void ErrorTwoGlobalBlocks()
        {
            // The project part is all at the top, but now split into multiple `Global` .. `EndGlobal` sections.
            TestErrorInSolution("Error_TwoGlobalBlocks.sln");
        }

        [Test]
        public void ErrorTwoNestedSections()
        {
            // The project part is all at the top, but now split into multiple `Global` .. `EndGlobal` sections.
            TestErrorInSolution("Error_TwoNestedSections.sln");
        }

        [Test]
        public void ErrorTwoConfigSections()
        {
            // The project part is all at the top, but now split into multiple `Global` .. `EndGlobal` sections.
            TestErrorInSolution("Error_TwoConfigSections.sln");
        }

        [Test]
        public void ErrorTextFile()
        {
            // This isn't a solution file, and so we don't have anything.
            TestErrorInSolution("Error_NotASolutionFile.sln");
        }

        [Test]
        public void ErrorEmptyFile()
        {
            // This isn't a solution file, and so we don't have anything.
            TestErrorInSolution("Error_EmptyFile.sln");
        }

        [Test]
        public void ErrorNoProjects()
        {
            // There are no `Project`..`EndProject` blocks.
            TestErrorInSolution("Error_NoProjects.sln");
        }

        [Test]
        public void ErrorNoProjectsInvalidLine()
        {
            // There are project blocks, but have some characters before them.
            TestErrorInSolution("Error_NoProjectsCommentedOut.sln");
        }

        [Test]
        public void ErrorDuplicateProjectGuid()
        {
            // There are two `Project`..`EndProject` and they have the same GUID.
            TestErrorInSolution("Error_DuplicateProjectGuid.sln");
        }

        [Test]
        public void ErrorDuplicateProjectPath()
        {
            // There are two `Project`..`EndProject` and they have the same value (path).
            TestErrorInSolution("Error_DuplicateProjectPath.sln");
        }
    }
}
