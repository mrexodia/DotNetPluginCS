using System;
using DotNetPlugin.NativeBindings.SDK;

namespace DotNetPlugin
{
    partial class Plugin
    {
        public override void OnInitDebug(in Plugins.PLUG_CB_INITDEBUG info)
        {
            var szFileName = info.szFileName;
            LogInfo($"DotNet test debugging of file {szFileName} started!");
        }

        public override void OnStopDebug(in Plugins.PLUG_CB_STOPDEBUG info)
        {
            LogInfo($"DotNet test debugging stopped!");
        }

        public override void OnCreateProcess(in Plugins.PLUG_CB_CREATEPROCESS info)
        {
            var CreateProcessInfo = info.CreateProcessInfo;
            var modInfo = info.modInfo;
            var DebugFileName = info.DebugFileName;
            var fdProcessInfo = info.fdProcessInfo;
            LogInfo($"Create process {info.DebugFileName}");
        }

        public override void OnLoadDll(in Plugins.PLUG_CB_LOADDLL info)
        {
            var LoadDll = info.LoadDll;
            var modInfo = info.modInfo;
            var modname = info.modname;
            LogInfo($"Load DLL {modname}");
        }
    }
}
