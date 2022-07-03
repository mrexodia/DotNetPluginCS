using DotNetPlugin.NativeBindings.SDK;

namespace DotNetPlugin
{
    /// <summary>
    /// Defines an API to interact with x64dbg.
    /// </summary>
    internal interface IPlugin
    {
        int PluginHandle { get; }

        bool Init();
        void Setup(in Plugins.PLUG_SETUPSTRUCT setupStruct);
        bool Stop();

        void OnMenuEntry(in Plugins.PLUG_CB_MENUENTRY info);
    }
}
