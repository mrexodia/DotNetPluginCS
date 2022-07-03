using System;
using System.Threading.Tasks;
using DotNetPlugin.NativeBindings;
using DotNetPlugin.NativeBindings.SDK;

namespace DotNetPlugin
{
    /// <summary>
    /// Implementation of your x64dbg plugin.
    /// </summary>
    /// <remarks>
    /// If you change the namespace or name of this class, don't forget to reflect the change in <see cref="PluginSession.CreatePlugin"/> too!
    /// </remarks>
    public partial class Plugin : PluginBase
    {
        public override bool Init()
        {
            Console.SetOut(PLogTextWriter.Default);
            Console.SetError(PLogTextWriter.Default);

            LogInfo($"PluginHandle: {PluginHandle}");

            // You can listen to debugger events in two ways:
            // 1. by declaring dll exports in the Stub project (see PluginMain), then adding the corresponding methods to the IPlugin interface,
            //    finally implementing them as required to propagate the call to the Plugin class or
            // 2. by registering callbacks using the EventCallback attribute (see Plugin.EventCallbacks.cs).

            // Please note that Option 1 goes through remoting in Debug builds (where Impl assembly unloading is enabled),
            // so it may be somewhat slower than Option 2. Release builds don't use remoting, just direct calls, so in that case there should be no significant difference.

            // Commands and function expressions are discovered and registered automatically. See Plugin.Commands.cs and Plugin.ExpressionFunctions.cs.

            // Menus can be registered by overriding the SetupMenu method. See Plugin.Menus.cs.

            return true;
        }

        public override void Setup(in Plugins.PLUG_SETUPSTRUCT setupStruct)
        {
            // Do additional UI setup (apart from menus) here.
        }

        public override Task<bool> StopAsync()
        {
            // Do additional cleanup here.

            return Task.FromResult(true);
        }
    }
}
