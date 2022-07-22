using System;
using System.Reflection;
using System.Threading.Tasks;
using DotNetPlugin.NativeBindings;
using DotNetPlugin.NativeBindings.SDK;
using DotNetPlugin.NativeBindings.Win32;

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
        IDisposable _eventCallbackRegistrations;
        Menus _menus;

        protected PluginBase() { }

        protected object MenusSyncObj => _menus;
        public int PluginHandle { get; internal set; }
        public Win32Window HostWindow { get; private set; }

        internal bool InitInternal()
        {
            var pluginMethods = GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            _commandRegistrations = Commands.Initialize(this, pluginMethods);
            _expressionFunctionRegistrations = ExpressionFunctions.Initialize(this, pluginMethods);
            _eventCallbackRegistrations = EventCallbacks.Initialize(this, pluginMethods);
            return Init();
        }

        public virtual bool Init() => true;

        protected virtual void SetupMenu(Menus menus) { }

        internal void SetupInternal(ref Plugins.PLUG_SETUPSTRUCT setupStruct)
        {
            HostWindow = new Win32Window(setupStruct.hwndDlg);

            _menus = new Menus(PluginHandle, ref setupStruct);

            try { SetupMenu(_menus); }
            catch (MenuException ex)
            {
                LogError($"Registration of menu failed. {ex.Message}");
                _menus.Clear();
            }

            Setup(ref setupStruct);
        }

        public virtual void Setup(ref Plugins.PLUG_SETUPSTRUCT setupStruct) { }

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
                _menus.Dispose();
                _eventCallbackRegistrations.Dispose();
                _expressionFunctionRegistrations.Dispose();
                _commandRegistrations.Dispose();
            }

            return false;
        }

        public virtual Task<bool> StopAsync() => Task.FromResult(true);

        void IPlugin.OnMenuEntry(ref Plugins.PLUG_CB_MENUENTRY info)
        {
            MenuItem menuItem;
            lock (MenusSyncObj)
            {
                menuItem = _menus.GetMenuItemById(info.hEntry);
            }
            menuItem?.Handler(menuItem);
        }
    }
}
