using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using DotNetPlugin.NativeBindings;
using DotNetPlugin.NativeBindings.SDK;

namespace DotNetPlugin
{
    /// <summary>
    /// Manages the lifecycle of a plugin instance and forward calls to it while it's alive.
    /// </summary>
    internal sealed class PluginSession :
#if ALLOW_UNLOADING
        MarshalByRefObject, IPluginSession
#else
        IPlugin
#endif
    {
        internal static PluginSession Null = new PluginSession(PluginBase.Null);

        private static Type GetPluginType(string pluginTypeName, string implAssemblyName)
        {
#if ALLOW_UNLOADING
            if (PluginMain.ImplAssemblyLocation != null)
            {
                Assembly implAssembly;

                try
                {
                    var rawAssembly = File.ReadAllBytes(PluginMain.ImplAssemblyLocation);
                    implAssembly = Assembly.Load(rawAssembly);
                }
                catch { implAssembly = null; }

                if (implAssembly != null)
                    return implAssembly.GetType(pluginTypeName, throwOnError: true);
            }
#endif

            return Type.GetType(pluginTypeName + ", " + implAssemblyName, throwOnError: true);
        }

        private static PluginBase CreatePlugin()
        {
            var implAssemblyName = typeof(PluginMain).Assembly.GetName().Name;
#if ALLOW_UNLOADING
            implAssemblyName += ".Impl";
#endif
            var pluginTypeName = typeof(PluginMain).Namespace + ".Plugin";
            var pluginType = GetPluginType(pluginTypeName, implAssemblyName);
            return (PluginBase)Activator.CreateInstance(pluginType);
        }

#if ALLOW_UNLOADING
        private volatile PluginBase _plugin;
#else
        private readonly PluginBase _plugin;
#endif

        private PluginSession(PluginBase plugin)
        {
            _plugin = plugin;
        }

        public PluginSession() : this(CreatePlugin()) { }

#if ALLOW_UNLOADING
        public void Dispose() => Stop();

        // https://stackoverflow.com/questions/2410221/appdomain-and-marshalbyrefobject-life-time-how-to-avoid-remotingexception
        public override object InitializeLifetimeService() => null;
#endif

        public int PluginHandle
        {
            get => _plugin.PluginHandle;
            set => _plugin.PluginHandle = value;
        }

        public bool Init() => _plugin.InitInternal();
        public void Setup(ref Plugins.PLUG_SETUPSTRUCT setupStruct) => _plugin.SetupInternal(ref setupStruct);
        public bool Stop()
        {
#if ALLOW_UNLOADING
            var plugin = Interlocked.Exchange(ref _plugin, PluginBase.Null);

            if (plugin == PluginBase.Null)
                return true;
#else
            var plugin = _plugin;
#endif

            return plugin.Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnMenuEntry(ref Plugins.PLUG_CB_MENUENTRY info) => ((IPlugin)_plugin).OnMenuEntry(ref info);
    }
}
