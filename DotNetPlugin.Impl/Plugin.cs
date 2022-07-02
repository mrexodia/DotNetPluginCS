﻿using System;
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
            // 1. by overriding the On*** methods of the base class or
            // 2. by manually registering callbacks (see RegisterCallbacks in Plugin.Callbacks.cs).

            // Option 1 works using exported dll functions (see PluginMain) which can be declared only in the Stub project.
            // You can add new event types by adding the desired dll export to PluginMain, extending the IPlugin interface and implementing the necessary functions.
            // Option 2, in turn, just registers the specified callbacks directly.

            // Please note that Option 1 goes through remoting in Debug builds (where Impl assembly unloading is enabled),
            // so it may be somewhat slower than Option 2. Release builds don't use remoting, just direct calls, so in that case there should be no significant difference.
            // However, it's recommended to disable dll exports for unused/manually registered callbacks by commenting them out in PluginMain.

            RegisterCallbacks();

            // Commands and function expressions are discovered and registered automatically. See Plugin.Commands.cs and Plugin.ExpressionFunctions.cs.

            return true;
        }

        public override void Setup(in Plugins.PLUG_SETUPSTRUCT setupStruct)
        {
            RegisterMenu(setupStruct);
        }

        public override Task<bool> StopAsync()
        {
            UnregisterMenu();

            UnregisterCallbacks();

            return Task.FromResult(true);
        }
    }
}
