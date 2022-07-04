using System;
using DotNetPlugin.NativeBindings;
using DotNetPlugin.NativeBindings.SDK;

namespace DotNetPlugin
{
    partial class Plugin
    {
        [EventCallback(Plugins.CBTYPE.CB_INITDEBUG)]
        public static void OnInitDebug(in Plugins.PLUG_CB_INITDEBUG info)
        {
            var szFileName = info.szFileName;
            LogInfo($"DotNet test debugging of file {szFileName} started!");
        }

        [EventCallback(Plugins.CBTYPE.CB_STOPDEBUG)]
        public static void OnStopDebug(in Plugins.PLUG_CB_STOPDEBUG info)
        {
            LogInfo($"DotNet test debugging stopped!");
        }

        [EventCallback(Plugins.CBTYPE.CB_CREATEPROCESS)]
        public static void OnCreateProcess(IntPtr infoPtr)
        {
            // info can also be cast manually
            var info = infoPtr.ToStructUnsafe<Plugins.PLUG_CB_CREATEPROCESS>();

            var CreateProcessInfo = info.CreateProcessInfo;
            var modInfo = info.modInfo;
            string DebugFileName = info.DebugFileName;
            var fdProcessInfo = info.fdProcessInfo;
            LogInfo($"Create process {info.DebugFileName}");
        }

        [EventCallback(Plugins.CBTYPE.CB_LOADDLL)]
        public static void OnLoadDll(in Plugins.PLUG_CB_LOADDLL info)
        {
            var LoadDll = info.LoadDll;
            var modInfo = info.modInfo;
            string modname = info.modname;
            LogInfo($"Load DLL {modname}");
        }
    }
}
