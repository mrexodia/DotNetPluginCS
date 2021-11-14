using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotNetPlugin
{
    // we need the module initializer feature of C# 9 because setting up assembly resolution in PluginMain is too late
    // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/module-initializers
    public class PluginModule
    {
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var location = typeof(PluginMain).Assembly.Location;
            var logPath = Path.ChangeExtension(location, ".log");

            File.AppendAllText(logPath, e.ExceptionObject.ToString());
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);

            if (assemblyName.Name == typeof(PluginMain).Assembly.GetName().Name)
                return typeof(PluginMain).Assembly;

            var location = typeof(PluginMain).Assembly.Location;
            var pluginBasePath = Path.GetDirectoryName(location);
            var dllPath = Path.Combine(pluginBasePath, assemblyName.Name + ".dll");

            return Assembly.LoadFile(dllPath);
        }

        [ModuleInitializer]
        public static void Initialize()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // makes sure that dependencies are resolved from the directory in which the main plugin dll resides
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
    }
}
