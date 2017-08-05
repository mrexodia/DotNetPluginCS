using System.Runtime.InteropServices;
using DotNetPlugin.SDK;
using RGiesecke.DllExport;

namespace DotNetPlugin
{
    public static class PluginMain
    {
        private const string plugin_name = "DotNetPlugin";
        private const int plugin_version = 1;

        [DllExport("pluginit", CallingConvention.Cdecl)]
        public static bool pluginit(ref Plugins.PLUG_INITSTRUCT initStruct)
        {
            Plugins.pluginHandle = initStruct.pluginHandle;
            initStruct.sdkVersion = Plugins.PLUG_SDKVERSION;
            initStruct.pluginVersion = plugin_version;
            initStruct.pluginName = plugin_name;
            return DotNetPluginCS.PluginInit(initStruct);
        }

        [DllExport("plugstop", CallingConvention.Cdecl)]
        private static bool plugstop()
        {
            DotNetPluginCS.PluginStop();
            return true;
        }

        [DllExport("plugsetup", CallingConvention.Cdecl)]
        private static void plugsetup(ref Plugins.PLUG_SETUPSTRUCT setupStruct)
        {
            DotNetPluginCS.PluginSetup(setupStruct);
        }
    }
}
