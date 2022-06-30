using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using DotNetPlugin.Bindings;
using DotNetPlugin.Bindings.SDK;
using RGiesecke.DllExport;

namespace DotNetPlugin
{
    /// <summary>
    /// Contains entry points for plugin lifecycle and debugger event callbacks.
    /// </summary>
    internal class PluginMain
    {
#if ALLOW_UNLOADING
        private static readonly Lazy<IPluginSession> NullSession = new Lazy<IPluginSession>(() => PluginSession.Null, LazyThreadSafetyMode.PublicationOnly);
        private static volatile Lazy<IPluginSession> s_session = NullSession;
        private static IPluginSession Session => s_session.Value;
#else
        private static PluginSession Session = PluginSession.Null;
#endif

        private static readonly string s_controlCommand = typeof(PluginMain).Assembly.GetName().Name.Replace(' ', '_');
        private static int s_pluginHandle;

        static PluginMain()
        {
            if (AppDomain.CurrentDomain.IsDefaultAppDomain())
            {
                AppDomain.CurrentDomain.UnhandledException += (s, e) => LogUnhandledException(e.ExceptionObject);

                // by default the runtime will look for referenced assemblies in the directory of the host application,
                // not in the plugin's dictionary, so we need to customize assembly resolving to fix this
                AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
                {
                    var assemblyName = new AssemblyName(e.Name);

                    if (assemblyName.Name == typeof(PluginMain).Assembly.GetName().Name)
                        return typeof(PluginMain).Assembly;

                    var location = typeof(PluginMain).Assembly.Location;
                    var pluginBasePath = Path.GetDirectoryName(location);
                    var dllPath = Path.Combine(pluginBasePath, assemblyName.Name + ".dll");

                    return Assembly.LoadFile(dllPath);
                };
            }
        }

        public static void LogUnhandledException(object exceptionObject)
        {
            var location = typeof(PluginMain).Assembly.Location;
            var logPath = Path.ChangeExtension(location, ".log");

            var errorMessage = exceptionObject?.ToString();
            if (errorMessage != null)
            {
                errorMessage += Environment.NewLine;
                File.AppendAllText(logPath, errorMessage);
                PLogTextWriter.Default.WriteLine(errorMessage);
            }
        }

        private static bool TryLoadPlugin(bool isInitial)
        {
#if ALLOW_UNLOADING
            var newSession = new Lazy<IPluginSession>(() => new PluginSessionProxy(), LazyThreadSafetyMode.ExecutionAndPublication);
            var originalSession = Interlocked.CompareExchange(ref s_session, newSession, NullSession);
            if (originalSession == NullSession)
            {
                _ = newSession.Value; // forces creation of session
                return true;
            }
#else
            if (isInitial)
            {
                Session = new PluginSession();
                return true;
            }
#endif

            return false;
        }

        private static bool TryUnloadPlugin()
        {
#if ALLOW_UNLOADING
            var originalSession = Interlocked.Exchange(ref s_session, NullSession);
            if (originalSession != NullSession)
            {
                originalSession.Value.Dispose();
                return true;
            }
#endif

            return false;
        }

        [DllExport("pluginit", CallingConvention.Cdecl)]
        public static bool pluginit(ref Plugins.PLUG_INITSTRUCT initStruct)
        {
            if (!TryLoadPlugin(isInitial: true))
                return false;

            initStruct.sdkVersion = Plugins.PLUG_SDKVERSION;
            initStruct.pluginVersion = Session.PluginVersion;
            initStruct.pluginName = Session.PluginName;
            Session.PluginHandle = s_pluginHandle = initStruct.pluginHandle;

#if ALLOW_UNLOADING
            if (!Plugins._plugin_registercommand(s_pluginHandle, s_controlCommand, ControlCommand, false))
            {
                PLogTextWriter.Default.WriteLine($"[{initStruct.pluginName}] Failed to register the \"'{s_controlCommand}'\" command.");
                TryUnloadPlugin();
                return false;
            }
#endif

            if (!Session.Init())
            {
                PLogTextWriter.Default.WriteLine($"[{Session.PluginName}] Failed to initialize the implementation library.");
                TryUnloadPlugin();
                return false;
            }

            return true;
        }

        [DllExport("plugstop", CallingConvention.Cdecl)]
        private static bool plugstop()
        {
            var success = Session.Stop();

#if ALLOW_UNLOADING
            Plugins._plugin_unregistercommand(s_pluginHandle, s_controlCommand);
#endif

            return success;
        }

        [DllExport("plugsetup", CallingConvention.Cdecl)]
        private static void plugsetup(ref Plugins.PLUG_SETUPSTRUCT setupStruct)
        {
            Session.Setup(in setupStruct);
        }

        private static bool ControlCommand(int argc, string[] argv)
        {
            if (argc > 1)
            {
                if ("load".Equals(argv[1], StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryLoadPlugin(isInitial: false))
                    {
                        PLogTextWriter.Default.WriteLine($"[{Session.PluginName}] Failed to load the implementation library.");
                        return false;
                    }

                    Session.PluginHandle = s_pluginHandle;

                    if (!Session.Init())
                    {
                        PLogTextWriter.Default.WriteLine($"[{Session.PluginName}] Failed to initialize the implementation library.");
                        TryUnloadPlugin();
                        return false;
                    }

                    PLogTextWriter.Default.WriteLine($"[{Session.PluginName}] Successfully loaded the implementation library.");
                    return true;
                }
                else if ("unload".Equals(argv[1], StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryUnloadPlugin())
                    {
                        PLogTextWriter.Default.WriteLine($"[{Session.PluginName}] Failed to unload the implementation library.");
                        return false;
                    }

                    PLogTextWriter.Default.WriteLine($"[{Session.PluginName}] Successfully unloaded the implementation library.");
                    return true;
                }
            }

            PLogTextWriter.Default.WriteLine($"[{Session.PluginName}] Invalid syntax. Usage: {s_controlCommand} [load|unload]");
            return false;
        }

        [DllExport("CBINITDEBUG", CallingConvention.Cdecl)]
        public static void CBINITDEBUG(Plugins.CBTYPE cbType, in Plugins.PLUG_CB_INITDEBUG info)
        {
            Session.OnInitDebug(in info);
        }

        [DllExport("CBSTOPDEBUG", CallingConvention.Cdecl)]
        public static void CBSTOPDEBUG(Plugins.CBTYPE cbType, in Plugins.PLUG_CB_STOPDEBUG info)
        {
            Session.OnStopDebug(in info);
        }

        [DllExport("CBCREATEPROCESS", CallingConvention.Cdecl)]
        public static void CBCREATEPROCESS(Plugins.CBTYPE cbType, in Plugins.PLUG_CB_CREATEPROCESS info)
        {
            Session.OnCreateProcess(in info);
        }

        [DllExport("CBLOADDLL", CallingConvention.Cdecl)]
        public static void CBLOADDLL(Plugins.CBTYPE cbType, in Plugins.PLUG_CB_LOADDLL info)
        {
            Session.OnLoadDll(in info);
        }

        [DllExport("CBMENUENTRY", CallingConvention.Cdecl)]
        public static void CBMENUENTRY(Plugins.CBTYPE cbType, in Plugins.PLUG_CB_MENUENTRY info)
        {
            Session.OnMenuEntry(in info);
        }
    }
}
