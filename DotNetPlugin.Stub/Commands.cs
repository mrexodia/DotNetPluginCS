using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using DotNetPlugin.NativeBindings.SDK;

namespace DotNetPlugin
{
    /// <summary>
    /// Attribute for automatically registering commands in x64Dbg.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public string Name { get; }

        public bool DebugOnly { get; set; }

        public CommandAttribute() { }

        public CommandAttribute(string name)
        {
            Name = name;
        }
    }

    internal static class Commands
    {
        private static Plugins.CBPLUGINCOMMAND BuildCallback(PluginBase plugin, MethodInfo method)
        {
            return (Plugins.CBPLUGINCOMMAND)Delegate.CreateDelegate(typeof(Plugins.CBPLUGINCOMMAND), method.IsStatic ? null : plugin, method, throwOnBindFailure: true);
        }

        public static IDisposable Initialize(PluginBase plugin, MethodInfo[] pluginMethods)
        {
            // command names are case-insensitive
            var registeredNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var methods = pluginMethods
                .SelectMany(method => method.GetCustomAttributes<CommandAttribute>().Select(attribute => (method, attribute)));

            foreach (var (method, attribute) in methods)
            {
                var name = attribute.Name ?? method.Name;

                if (method.ReturnType != typeof(bool))
                {
                    PluginBase.LogError($"Registration of command '{name}' is skipped. Method '{method.Name}' has an invalid return type.");
                    continue;
                }

                var methodParams = method.GetParameters();

                if (methodParams.Length == 1 ? methodParams[0].ParameterType != typeof(string[]) :
                    methodParams.Length == 2 && (methodParams[0].ParameterType != typeof(int) || methodParams[1].ParameterType != typeof(string[])))
                {
                    PluginBase.LogError($"Registration of command '{name}' is skipped. Method '{method.Name}' has an invalid signature.");
                    continue;
                }

                if (registeredNames.Contains(name) ||
                    !Plugins._plugin_registercommand(plugin.PluginHandle, name, BuildCallback(plugin, method), attribute.DebugOnly))
                {
                    PluginBase.LogError($"Registration of command '{name}' failed.");
                    continue;
                }

                registeredNames.Add(name);
            }

            return new Registrations(plugin, registeredNames);
        }

        private sealed class Registrations : IDisposable
        {
            private PluginBase _plugin;
            private HashSet<string> _registeredNames;

            public Registrations(PluginBase plugin, HashSet<string> registeredNames)
            {
                _plugin = plugin;
                _registeredNames = registeredNames;
            }

            public void Dispose()
            {
                var plugin = Interlocked.Exchange(ref _plugin, null);

                if (plugin != null)
                {
                    foreach (var name in _registeredNames)
                    {
                        if (!Plugins._plugin_unregistercommand(plugin.PluginHandle, name))
                            PluginBase.LogError($"Unregistration of command '{name}' failed.");
                    }

                    _registeredNames = null;
                }
            }
        }
    }
}
