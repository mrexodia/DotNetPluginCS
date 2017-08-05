using System.Runtime.InteropServices;
using DotNetPlugin.SDK;
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
            PLog.WriteLine("[DotNet TEST] pluginHandle: {0}", Plugins.pluginHandle);
            if (!Plugins._plugin_registercommand(Plugins.pluginHandle, "DotNetpluginTestCommand", RegisteredCommands.cbNetTestCommand, false))
                PLog.WriteLine("[DotNet TEST] error registering the \"DotNetpluginTestCommand\" command!");
            if (!Plugins._plugin_registercommand(Plugins.pluginHandle, "DotNetDumpProcess", RegisteredCommands.cbDumpProcessCommand, true))
                PLog.WriteLine("[DotNet TEST] error registering the \"DotNetDumpProcess\" command!");
            if (!Plugins._plugin_registercommand(Plugins.pluginHandle, "DotNetModuleEnum", RegisteredCommands.cbModuleEnum, true))
                PLog.WriteLine("[DotNet TEST] error registering the \"DotNetModuleEnum\" command!");
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

        [DllExport("CBINITDEBUG", CallingConvention.Cdecl)]
        public static void CBINITDEBUG(Plugins.CBTYPE cbType, ref Plugins.PLUG_CB_INITDEBUG info)
        {
            var szFileName = info.szFileName.MarshalToString();
            PLog.WriteLine("[DotNet TEST] DotNet test debugging of file {0} started!", szFileName);
        }

        [DllExport("CBSTOPDEBUG", CallingConvention.Cdecl)]
        public static void CBSTOPDEBUG(Plugins.CBTYPE cbType, ref Plugins.PLUG_CB_STOPDEBUG info)
        {
            PLog.WriteLine("[DotNet TEST] DotNet test debugging stopped!");
        }

        [DllExport("CBCREATEPROCESS", CallingConvention.Cdecl)]
        public static void CBCREATEPROCESS(Plugins.CBTYPE cbType, ref Plugins.PLUG_CB_CREATEPROCESS info)
        {
            var CreateProcessInfo = info.CreateProcessInfo.ToStruct<WAPI.CREATE_PROCESS_DEBUG_INFO>();
            var modInfo = info.modInfo.ToStruct<WAPI.IMAGEHLP_MODULE64>();
            var DebugFileName = info.DebugFileName.MarshalToString();
            var fdProcessInfo = info.fdProcessInfo.ToStruct<WAPI.PROCESS_INFORMATION>();
            PLog.WriteLine("[DotNet TEST] Create process {0}", info.DebugFileName.MarshalToString());
        }

        [DllExport("CBLOADDLL", CallingConvention.Cdecl)]
        public static void CBLOADDLL(Plugins.CBTYPE cbType, ref Plugins.PLUG_CB_LOADDLL info)
        {
            var LoadDll = info.LoadDll.ToStruct<WAPI.LOAD_DLL_DEBUG_INFO>();
            var modInfo = info.modInfo.ToStruct<WAPI.IMAGEHLP_MODULE64>();
            var modname = info.modname.MarshalToString();
        }

        [DllExport("CBMENUENTRY", CallingConvention.Cdecl)]
        public static void CBMENUENTRY(Plugins.CBTYPE cbType, ref Plugins.PLUG_CB_MENUENTRY info)
        {
            switch (info.hEntry)
            {
                case MENU_ABOUT:
                    Interaction.MsgBox("Test DotNet Plugins For x64dbg\nCoded By Ahmadmansoor/exetools", MsgBoxStyle.OkOnly, "Info");
                    break;
                case MENU_DUMP:
                    if (!Bridge.DbgIsDebugging())
                    {
                        PLog.WriteLine("You need to be debugging to use this Command");
                        break;
                    }
                    Bridge.DbgCmdExec("DotNetDumpProcess");
                    break;
            }
        }
    }
}
