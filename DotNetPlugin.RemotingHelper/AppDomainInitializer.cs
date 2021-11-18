using System;
using System.IO;
using System.Reflection;

namespace DotNetPlugin
{
    /// <summary>
    /// A helper class which enables the Stub assembly to be resolved in a separate app domain.
    /// </summary>
    /// <remarks>
    /// It's inevitable to place this class into a separate assembly because of an issue of the remoting activator:
    /// if this type resided in the Stub assembly, the activator would want to load that assembly in the app domain upon initialization,
    /// which would fail because the activator looks for a dll but x64dbg plugins must have a custom extension (dp32/dp64)...
    /// </remarks>
    public static class AppDomainInitializer
    {
        private const string DllExtension =
#if AMD64
            ".dp64";
#else
            ".dp32";
#endif

        public static void Initialize(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                var assemblyName = new AssemblyName(e.Name);
                var pluginAssemblyName = typeof(AppDomainInitializer).Assembly.GetName().Name;

                if (pluginAssemblyName.StartsWith(assemblyName.Name, StringComparison.OrdinalIgnoreCase) &&
                    pluginAssemblyName.Substring(assemblyName.Name.Length).Equals(".RemotingHelper", StringComparison.OrdinalIgnoreCase))
                {
                    var location = typeof(AppDomainInitializer).Assembly.Location;
                    var pluginBasePath = Path.GetDirectoryName(location);
                    var dllPath = Path.Combine(pluginBasePath, assemblyName.Name + DllExtension);

                    return Assembly.LoadFile(dllPath);
                }

                return null;
            };
        }
    }
}
