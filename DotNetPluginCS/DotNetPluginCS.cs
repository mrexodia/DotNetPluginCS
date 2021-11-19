using System;
using System.Runtime.InteropServices;
using Managed.x64dbg.SDK;
using Microsoft.VisualBasic;
using RGiesecke.DllExport;

namespace DotNetPlugin
{
    public class DotNetPluginCS
    {
        private const int MENU_ABOUT = 0;
        private const int MENU_DUMP = 1;
        private const int MENU_TEST = 2;

        public static bool PluginInit(Plugins.PLUG_INITSTRUCT initStruct)
        {
            Console.WriteLine("[DotNet TEST] pluginHandle: {0}", Plugins.pluginHandle);

            if (!Plugins._plugin_registercommand(Plugins.pluginHandle, "DotNetpluginTestCommand", RegisteredCommands.cbNetTestCommand, false))
                Console.WriteLine("[DotNet TEST] error registering the \"DotNetpluginTestCommand\" command!");
            if (!Plugins._plugin_registercommand(Plugins.pluginHandle, "DotNetDumpProcess", RegisteredCommands.cbDumpProcessCommand, true))
                Console.WriteLine("[DotNet TEST] error registering the \"DotNetDumpProcess\" command!");
            if (!Plugins._plugin_registercommand(Plugins.pluginHandle, "DotNetModuleEnum", RegisteredCommands.cbModuleEnum, true))
                Console.WriteLine("[DotNet TEST] error registering the \"DotNetModuleEnum\" command!");

            Plugins._plugin_registercallback(Plugins.pluginHandle, Plugins.CBTYPE.CB_INITDEBUG, (cbType, info) => CBINITDEBUG(cbType, in info.ToStructUnsafe<Plugins.PLUG_CB_INITDEBUG>()));
            Plugins._plugin_registercallback(Plugins.pluginHandle, Plugins.CBTYPE.CB_STOPDEBUG, (cbType, info) => CBSTOPDEBUG(cbType, in info.ToStructUnsafe<Plugins.PLUG_CB_STOPDEBUG>()));
            return true;
        }

        public static void PluginStop()
        {
            Plugins._plugin_unregistercallback(Plugins.pluginHandle, Plugins.CBTYPE.CB_INITDEBUG);
            Plugins._plugin_unregistercallback(Plugins.pluginHandle, Plugins.CBTYPE.CB_STOPDEBUG);
        }

        public static void PluginSetup(Plugins.PLUG_SETUPSTRUCT setupStruct)
        {
            Plugins._plugin_menuaddentry(setupStruct.hMenu, 0, "&About...");
            Plugins._plugin_menuaddentry(setupStruct.hMenu, 1, "&DotNetDumpProcess");
            Plugins._plugin_menuaddentry(setupStruct.hMenu, 2, "&Hex Editor");
            int hSubMenu = Plugins._plugin_menuadd(setupStruct.hMenu, "sub menu");
            Plugins._plugin_menuaddentry(hSubMenu, 3, "sub menu entry");
        }

        //[DllExport("CBINITDEBUG", CallingConvention.Cdecl)]
        public static void CBINITDEBUG(Plugins.CBTYPE cbType, in Plugins.PLUG_CB_INITDEBUG info)
        {
            var szFileName = info.szFileName;
            Console.WriteLine("[DotNet TEST] DotNet test debugging of file {0} started!", szFileName);
        }

        //[DllExport("CBSTOPDEBUG", CallingConvention.Cdecl)]
        public static void CBSTOPDEBUG(Plugins.CBTYPE cbType, in Plugins.PLUG_CB_STOPDEBUG info)
        {
            Console.WriteLine("[DotNet TEST] DotNet test debugging stopped!");
        }

        [DllExport("CBCREATEPROCESS", CallingConvention.Cdecl)]
        public static void CBCREATEPROCESS(Plugins.CBTYPE cbType, in Plugins.PLUG_CB_CREATEPROCESS info)
        {
            var CreateProcessInfo = info.CreateProcessInfo;
            var modInfo = info.modInfo;
            var DebugFileName = info.DebugFileName;
            var fdProcessInfo = info.fdProcessInfo;
            Console.WriteLine("[DotNet TEST] Create process {0}", info.DebugFileName);
        }

        [DllExport("CBLOADDLL", CallingConvention.Cdecl)]
        public static void CBLOADDLL(Plugins.CBTYPE cbType, in Plugins.PLUG_CB_LOADDLL info)
        {
            var LoadDll = info.LoadDll;
            var modInfo = info.modInfo;
            var modname = info.modname;
        }

        [DllExport("CBMENUENTRY", CallingConvention.Cdecl)]
        public static void CBMENUENTRY(Plugins.CBTYPE cbType, in Plugins.PLUG_CB_MENUENTRY info)
        {
            switch (info.hEntry)
            {
                case MENU_ABOUT:
                    Interaction.MsgBox("Test DotNet Plugins For x64dbg\nCoded By Ahmadmansoor/exetools", MsgBoxStyle.OkOnly, "Info");
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
    }
}
