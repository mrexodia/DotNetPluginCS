using System;
using System.Threading.Tasks;
using DotNetPlugin.NativeBindings;
using DotNetPlugin.NativeBindings.SDK;
using Microsoft.VisualBasic;

namespace DotNetPlugin
{
    /// <summary>
    /// Implementation of your x64dbg plugin.
    /// </summary>
    /// <remarks>
    /// If you change the namespace or name of this class, don't forget to reflect the change in <see cref="PluginSession.CreatePlugin"/> too!
    /// </remarks>
    public class Plugin : PluginBase
    {
        private const int MENU_ABOUT = 0;
        private const int MENU_DUMP = 1;

        public const string PluginLogName = "DotNet TEST";

        #region Plugin lifecycle

        public override bool Init()
        {
            Console.SetOut(PLogTextWriter.Default);

            Console.WriteLine($"[{PluginLogName}] pluginHandle: {PluginHandle}");

            if (!Plugins._plugin_registercommand(PluginHandle, "DotNetpluginTestCommand", RegisteredCommands.cbNetTestCommand, false))
                Console.WriteLine($"[{PluginLogName}] error registering the \"DotNetpluginTestCommand\" command!");
            if (!Plugins._plugin_registercommand(PluginHandle, "DotNetDumpProcess", RegisteredCommands.cbDumpProcessCommand, true))
                Console.WriteLine($"[{PluginLogName}] error registering the \"DotNetDumpProcess\" command!");
            if (!Plugins._plugin_registercommand(PluginHandle, "DotNetModuleEnum", RegisteredCommands.cbModuleEnum, true))
                Console.WriteLine($"[{PluginLogName}] error registering the \"DotNetModuleEnum\" command!");

            // You can listen to debugger events in two ways:
            // 1. by overriding the On*** methods of the base class or
            // 2. by manually registering callbacks like
            // Plugins._plugin_registercallback(PluginHandle, Plugins.CBTYPE.CB_INITDEBUG, (cbType, info) => OnInitDebug(in info.ToStructUnsafe<Plugins.PLUG_CB_INITDEBUG>()));
            // Plugins._plugin_registercallback(PluginHandle, Plugins.CBTYPE.CB_STOPDEBUG, (cbType, info) => OnStopDebug(in info.ToStructUnsafe<Plugins.PLUG_CB_STOPDEBUG>()));

            // Option 1 works using exported dll functions (see PluginMain) which can be declared only in the Stub project.
            // You can add new event types by adding the desired dll export to PluginMain, extending the IPlugin interface and implementing the necessary functions.
            // Option 2, in turn, just registers the specified callbacks directly.

            // Please note that Option 1 goes through remoting in Debug builds (where Impl assembly unloading is enabled),
            // so it may be somewhat slower than Option 2. Release builds don't use remoting, just direct calls, so in that case there should be no significant difference.
            // However, it's recommended to disable dll exports for unused/manually registered callbacks by commenting them out in PluginMain.

            return true;
        }

        public override void Setup(in Plugins.PLUG_SETUPSTRUCT setupStruct)
        {
            Plugins._plugin_menuaddentry(setupStruct.hMenu, MENU_ABOUT, "&About...");
            Plugins._plugin_menuaddentry(setupStruct.hMenu, MENU_DUMP, "&DotNetDumpProcess");
            Plugins._plugin_menuaddentry(setupStruct.hMenu, 2, "&Hex Editor");
            int hSubMenu = Plugins._plugin_menuadd(setupStruct.hMenu, "sub menu");
            Plugins._plugin_menuaddentry(hSubMenu, 3, "sub menu entry");
        }

        public override Task<bool> StopAsync()
        {
            // You must unregister debugger event callbacks registered via Plugins._plugin_registercallback (see Option 2 above) here! E.g.
            // Plugins._plugin_unregistercallback(PluginHandle, Plugins.CBTYPE.CB_INITDEBUG);
            // Plugins._plugin_unregistercallback(PluginHandle, Plugins.CBTYPE.CB_STOPDEBUG);

            Plugins._plugin_unregistercommand(PluginHandle, "DotNetpluginTestCommand");
            Plugins._plugin_unregistercommand(PluginHandle, "DotNetDumpProcess");
            Plugins._plugin_unregistercommand(PluginHandle, "DotNetModuleEnum");

            return Task.FromResult(true);
        }

        #endregion

        #region Callbacks

        public override void OnInitDebug(in Plugins.PLUG_CB_INITDEBUG info)
        {
            var szFileName = info.szFileName;
            Console.WriteLine($"[{PluginLogName}] DotNet test debugging of file {szFileName} started!");
        }

        public override void OnStopDebug(in Plugins.PLUG_CB_STOPDEBUG info)
        {
            Console.WriteLine($"[{PluginLogName}] DotNet test debugging stopped!");
        }

        public override void OnCreateProcess(in Plugins.PLUG_CB_CREATEPROCESS info)
        {
            var CreateProcessInfo = info.CreateProcessInfo;
            var modInfo = info.modInfo;
            var DebugFileName = info.DebugFileName;
            var fdProcessInfo = info.fdProcessInfo;
            Console.WriteLine($"[{PluginLogName}] Create process {info.DebugFileName}");
        }

        public override void OnLoadDll(in Plugins.PLUG_CB_LOADDLL info)
        {
            var LoadDll = info.LoadDll;
            var modInfo = info.modInfo;
            var modname = info.modname;
            Console.WriteLine($"[{PluginLogName}] Load DLL {modname}");
        }

        public override void OnMenuEntry(in Plugins.PLUG_CB_MENUENTRY info)
        {
            switch (info.hEntry)
            {
                case MENU_ABOUT:
                    Interaction.MsgBox("DotNet Plugin For x64dbg\nCoded By <your_name_here>", MsgBoxStyle.OkOnly, "Info");
                    break;
                case MENU_DUMP:
                    if (!Bridge.DbgIsDebugging())
                    {
                        Console.WriteLine("You need to be debugging to use this Command");
                        break;
                    }
                    Bridge.DbgCmdExec("DotNetDumpProcess");
                    break;
            }
        }

        #endregion
    }
}
