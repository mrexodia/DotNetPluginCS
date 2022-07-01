using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
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
        IPlugin, IDisposable
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
            var implAssemblyName = typeof(PluginMain).Assembly.GetName().Name + ".Impl";
            var pluginTypeName = typeof(PluginMain).Namespace + ".Plugin";
            var pluginType = GetPluginType(pluginTypeName, implAssemblyName);
            return (PluginBase)Activator.CreateInstance(pluginType);
        }

        private volatile PluginBase _plugin;

        private PluginSession(PluginBase plugin)
        {
            _plugin = plugin;
        }

        public PluginSession() : this(CreatePlugin()) { }

        public void Dispose() => Stop();

#if ALLOW_UNLOADING
        // https://stackoverflow.com/questions/2410221/appdomain-and-marshalbyrefobject-life-time-how-to-avoid-remotingexception
        public override object InitializeLifetimeService() => null;
#endif

        public int PluginVersion => _plugin.PluginVersion;
        public string PluginName => _plugin.PluginName;
        public int PluginHandle
        {
            get => _plugin.PluginHandle;
            set => _plugin.PluginHandle = value;
        }

        public bool Init() => _plugin.Init();
        public void Setup(in Plugins.PLUG_SETUPSTRUCT setupStruct) => _plugin.Setup(setupStruct);
        public bool Stop()
        {
            var plugin = Interlocked.Exchange(ref _plugin, PluginBase.Null);

            if (plugin == PluginBase.Null)
                return true;

            return plugin.Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnInitDebug(in Plugins.PLUG_CB_INITDEBUG info) => _plugin.OnInitDebug(in info);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnStopDebug(in Plugins.PLUG_CB_STOPDEBUG info) => _plugin.OnStopDebug(in info);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCreateProcess(in Plugins.PLUG_CB_CREATEPROCESS info) => _plugin.OnCreateProcess(in info);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnLoadDll(in Plugins.PLUG_CB_LOADDLL info) => _plugin.OnLoadDll(in info);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnMenuEntry(in Plugins.PLUG_CB_MENUENTRY info) => _plugin.OnMenuEntry(in info);
    }
}
