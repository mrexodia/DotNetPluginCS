# DotNetPluginCS - A .NET Framework plugin template for x86dbg
This project provides a foundation for developing plugins for [x64dbg](https://x64dbg.com/) in C#, on the .NET platform. (.NET means the "classic", Windows-only .NET Framework in this case.)

On top of defining essential native bindings which are necessary to interact with the debugger host, it also provides a straightforward project structure and an ergonomic wrapper API over the aforementioned low-level bindings.

The template is architected so that it can speed up test-change-test development cycles greatly. At development time, whenever you need to make a change during testing, you'll only have to rebuild your plugin and it will be reloaded automatically into the host.

## Prerequisites
To build the project, you'll minimally need
* [Visual Studio Build Tools](https://docs.microsoft.com/hu-hu/visualstudio/releases/2019/history) (2019 v16.7 or later)
* [.NET Core 3.1/.NET 6+ SDK](https://dotnet.microsoft.com/en-us/download/dotnet) (required only for building)
* [.NET Framework 4.7.2  SDK](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472)

However, probably you'd just have an easier time with the full [Visual Studio IDE](https://visualstudio.microsoft.com/) (2019 v16.7 or later, Community edition is sufficient) since that includes all of the required components. Other IDEs which support C# 9 might also do but it's not tested.

## Getting Started

1. Fork the project and create a local copy (`git clone <fork-url>`).
2. Open *Directory.Build.props* and change the `PluginName` and `PluginAssemblyName` properties to your liking.
3. The actual implementation of the plugin logic resides in the _DotNetPlugin.Impl_ project. (Usually you won’t need to touch the other ones.) So, open *DotNetPlugin.Impl/Plugin.cs*, which is the entry point to your code. You find some further information there.
5. You can quickly start to implement your ideas by examining the samples to be found in the following files:
   * *Plugin.Commands.cs* - Here you can define custom [commands](https://help.x64dbg.com/en/latest/introduction/Input.html#commands) using the `Command` attribute. Methods marked with this attribute will be automatically discovered (using reflection) and registered. (Both static and instance methods are allowed as handlers, they just need to have the right signature. Return type can be `void`, in which case the command will always report success.) 
   * *Plugin.EventCallbacks.cs* - Here you can register [callbacks](https://help.x64dbg.com/en/latest/developers/plugins/basics.html#exports) to get notified of debugger events. Use the `EventCallback` attribute to make them automatically registered, just like in the case of commands. Further remarks: pay close attention to the method signature. The parameter type must match the event type specified in the attribute, otherwise it won't work. You can look up this mapping e.g. in [the plugin SDK definition](https://github.com/x64dbg/x64dbg/blob/29bb559aa6ac5155ff518b43f3c84f4a72abd8bf/src/dbg/_plugins.h#L260). Also keep in mind [this warning from the docs](https://help.x64dbg.com/en/latest/developers/plugins/Callbacks/index.html):
     > In general AVOID time-consuming operations inside callbacks, do these in separate threads.
   *  *Plugin.ExpressionFunction.cs* - Here you can define [expression functions](https://help.x64dbg.com/en/latest/introduction/Expression-functions.html) using the `ExpressionFunction` attribute. Works in the same way as commands. The return type, however, must always be `nuint` (`UIntPtr`).
   *  *Plugin.Menus.cs* - In the `SetupMenu` method you can register menu items and sub-menus via a fluent-like API to extend the various menus of the host.

## Development
After implementing something useful, you can test your plugin like this:
1. Build the `DotNetPlugin.Impl` project (or the whole solution - the end result should be the same) **in *Debug* configuration**. 
2. In the root directory of the project a folder named *bin* will be created. Locate the *.dp32*/*.dp64* file (depending on the target CPU architecture of the build). Copy that file together with *DotNetPlugin.RemotingHelper.dll* to the corresponding *plugins* directory of x64dbg.
3. Run the debugger.
4. Test your plugin.
5. Modify the plugin if needed and rebuild it. At this point it will be automatically reloaded, so goto 4 until you're satisfied.

## Release
When you decide that the plugin is ready for use, you can create a performance-optimized version of it. So, change the **build configuration to *Release*** and build it. Now you'll only need the single *.dp32*/*.dp64* file.

## Samples
* [tracecalls](https://github.com/adams85/DotNetPluginCS/tree/tracecalls) by adams85 - An attempt on implementing WinDbg's `wt` command (or at least something similar to that) for x64dbg.

## Project Status
The wiring-up to the host is pretty much complete now and it's wrapped in a more ergonomic API (though the low-level APIs are also available if needed).

However, many functions exposed by the host (especially, C++ APIs) are still missing. Currently there's no plan to add bindings for these to make them available out of the box. Plugin writers can add them manually as needed. Pushing such changes back upstream would be greatly appreciated.

As an alternative, you can use the more complete bindings of the [DotX64Dbg](https://github.com/x64dbg/DotX64Dbg), another .NET plugin project. This one runs on .NET Core and uses a different approach but the bindings defined there (using C++/CLI) can be used with this solution too. A proof of concept for this can be found [here](https://github.com/adams85/DotNetPluginCS/tree/dotx64dbg-backport).

### Happy coding & debugging! 
