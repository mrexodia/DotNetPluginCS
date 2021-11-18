using System;
using System.Reflection;
using System.Threading.Tasks;
using DotNetPlugin.Bindings.SDK;

namespace DotNetPlugin
{
    /// <summary>
    /// Provides a base class from which the Plugin class must derive in the Impl assembly.
    /// </summary>
    public class PluginBase : IPlugin
    {
        internal static PluginBase Null = new PluginBase();

        private static readonly string DefaultPluginName =
            ((AssemblyTitleAttribute)typeof(PluginMain).Assembly.GetCustomAttribute(typeof(AssemblyTitleAttribute)))?.Title ??
            typeof(PluginMain).Assembly.GetName().Name;

        private static readonly int DefaultPluginVersion = typeof(PluginMain).Assembly.GetName().Version.Major;

        protected PluginBase() { }

        public virtual int PluginVersion => DefaultPluginVersion;
        public virtual string PluginName => DefaultPluginName;

        public int PluginHandle { get; internal set; }

        public virtual bool Init() => true;

        public virtual void Setup(in Plugins.PLUG_SETUPSTRUCT setupStruct) { }

        public bool Stop()
        {
            try
            {
                var stopTask = StopAsync();

                if (Task.WhenAny(stopTask, Task.Delay(5000)).GetAwaiter().GetResult() == stopTask)
                    return stopTask.GetAwaiter().GetResult(); // also unwraps potential exceptions
            }
            catch (Exception ex)
            {
                PluginMain.LogUnhandledException(ex);
            }

            return false;
        }

        public virtual Task<bool> StopAsync() => Task.FromResult(true);

        public virtual void OnInitDebug(in Plugins.PLUG_CB_INITDEBUG info) { }
        public virtual void OnStopDebug(in Plugins.PLUG_CB_STOPDEBUG info) { }
        public virtual void OnCreateProcess(in Plugins.PLUG_CB_CREATEPROCESS info) { }
        public virtual void OnLoadDll(in Plugins.PLUG_CB_LOADDLL info) { }
        public virtual void OnMenuEntry(in Plugins.PLUG_CB_MENUENTRY info) { }
    }
}
