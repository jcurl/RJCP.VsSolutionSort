namespace RJCP.VsSolutionSort
{
    using System;
    using System.Threading.Tasks;
    using Parser;

    internal static class Program
    {
        internal async static Task<int> Main(string[] args)
        {
            var solution = new Solution();
            try {
                await solution.LoadAsync(args[0]);
            } catch (SolutionFormatException ex) {
                Console.WriteLine($"Failed - {ex.Message}");
                return 1;
            }

            // Dump the solution
            foreach (ISection section in solution.Sections) {
                if (section is TextBlock textBlock) {
                    Console.WriteLine("-> Text Block");
                    foreach (Line line in textBlock.Lines) {
                        Console.WriteLine(line.ToString());
                    }
                } else if (section is Projects projects) {
                    // TODO: Here we would now use our sorted list.
                    Console.WriteLine("-> Project Block");
                    foreach (Project project in projects.ProjectList) {
                        Console.WriteLine(project.ToString());
                        foreach (Line line in project.Lines) {
                            Console.WriteLine(line);
                        }
                    }
                } else if (section is Global global) {
                    Console.WriteLine("-> Global Block");
                    Console.WriteLine(global.ToString());
                    foreach (ISection globalSection in global.Sections) {
                        if (globalSection is TextBlock globalTextBlock) {
                            Console.WriteLine("---> Global Text Block");
                            foreach (Line line in globalTextBlock.Lines) {
                                Console.WriteLine(line.ToString());
                            }
                        } else if (globalSection is ProjConfigGlobalSection configSection) {
                            Console.WriteLine("---> Global Section Block PROJECT CONFIG");
                            Console.WriteLine(configSection.ToString());
                            foreach (Line line in configSection.Lines) {
                                Console.WriteLine(line.ToString());
                            }
                        } else if (globalSection is NestedProjGlobalSection nestedSection) {
                            Console.WriteLine("---> Global Section Block NESTED CONFIG");
                            Console.WriteLine(nestedSection.ToString());
                            foreach (Line line in nestedSection.Lines) {
                                Console.WriteLine(line.ToString());
                            }
                        } else if (globalSection is GlobalSection genericSection) {
                            Console.WriteLine("---> Global Section Block");
                            Console.WriteLine(genericSection.ToString());
                            foreach (Line line in genericSection.Lines) {
                                Console.WriteLine(line.ToString());
                            }
                        }
                    }
                }
            }

            return 0;
        }
    }
}
