using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DotNetPlugin.NativeBindings;
using DotNetPlugin.NativeBindings.SDK;

namespace DotNetPlugin
{
    /// <summary>
    /// Provides a base class from which the Plugin class must derive in the Impl assembly.
    /// </summary>
    public class PluginBase : IPlugin
    {
        internal static PluginBase Null = new PluginBase();

        public static readonly string PluginName =
            typeof(PluginMain).Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ??
            typeof(PluginMain).Assembly.GetName().Name;

        public static readonly int PluginVersion = typeof(PluginMain).Assembly.GetName().Version.Major;

        private static readonly string PluginLogPrefix = $"[PLUGIN, {PluginName}]";

        public static void LogInfo(string message) => PLogTextWriter.Default.WriteLine(PluginLogPrefix + " " + message);
        public static void LogError(string message) => LogInfo(message);

        IDisposable _commandRegistrations;
        IDisposable _expressionFunctionRegistrations;

        protected PluginBase() { }

        public int PluginHandle { get; internal set; }

        internal bool InitInternal()
        {
            var pluginMethods = GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            _commandRegistrations = Commands.Initialize(this, pluginMethods);
            _expressionFunctionRegistrations = ExpressionFunctions.Initialize(this, pluginMethods);

            return Init();
        }

        public virtual bool Init() => true;

        public virtual void Setup(in Plugins.PLUG_SETUPSTRUCT setupStruct) { }

        public bool Stop()
        {
            try
            {
                var stopTask = StopAsync();

                if (Task.WhenAny(stopTask, Task.Delay(5000)).GetAwaiter().GetResult() == stopTask)
                {
                    if (!stopTask.IsCanceled)
                        return stopTask.ConfigureAwait(false).GetAwaiter().GetResult(); // also unwraps potential exception
                }
            }
            catch (Exception ex)
            {
                PluginMain.LogUnhandledException(ex);
            }
            finally
            {
                _commandRegistrations.Dispose();
                _expressionFunctionRegistrations.Dispose();
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
