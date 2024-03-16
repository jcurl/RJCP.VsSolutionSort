namespace RJCP.VsSolutionSort.CmdLine
{
    using System;
    using System.Reflection;
    using Resources;
    using RJCP.Core.Terminal;

    internal class Version
    {
        private readonly ITerminal m_Terminal;
        private static readonly object Lock = new();
        private static string AssemblyVersion;

        public Version(ITerminal terminal)
        {
            ArgumentNullException.ThrowIfNull(terminal);
            m_Terminal = terminal;
        }

        public void PrintVersion()
        {
            m_Terminal.StdOut.WriteLine(GetVersion());
        }

        public static string GetVersion()
        {
            if (AssemblyVersion is null) {
                lock (Lock) {
                    if (AssemblyVersion is null) {
                        string copyright = GetAssemblyCopyright(typeof(Program));
                        string version = GetAssemblyVersion(typeof(Program));
                        if (string.IsNullOrWhiteSpace(copyright)) {
                            AssemblyVersion = string.Format(HelpResource.Version, version);
                        } else {
                            AssemblyVersion = string.Format(HelpResource.VersionCopyright, version, copyright);
                        }
                    }
                }
            }
            return AssemblyVersion;
        }

        public static string GetAssemblyVersion(Type type)
        {
            Assembly assembly = type.Assembly;
            if (Attribute.GetCustomAttribute(assembly,
                typeof(AssemblyInformationalVersionAttribute)) is AssemblyInformationalVersionAttribute infoVersion) {
                return infoVersion.InformationalVersion;
            }
            return assembly.GetName().Version?.ToString() ?? string.Empty;
        }

        public static string GetAssemblyCopyright(Type type)
        {
            Assembly assembly = type.Assembly;
            if (Attribute.GetCustomAttribute(assembly,
                typeof(AssemblyCopyrightAttribute)) is AssemblyCopyrightAttribute copyright) {
                return copyright.Copyright;
            }
            return string.Empty;
        }
    }
}
