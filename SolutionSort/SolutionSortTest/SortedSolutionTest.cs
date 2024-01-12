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
        }

        [TestCaseSource(nameof(Solutions))]
        public async Task SortedSolutions(string solution)
        {
            string sourceSolution = Path.Combine(Deploy.TestDirectory, "TestResources", "Solutions", solution);
            string sortedSolution = Path.Combine(Deploy.TestDirectory, "TestResources", "Solutions",
                $"{Path.GetFileNameWithoutExtension(solution)}Sorted.sln");

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
        }
    }
}
