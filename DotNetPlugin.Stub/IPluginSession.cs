#if ALLOW_UNLOADING

using System;

namespace DotNetPlugin
{
    /// <summary>
    /// Represents the lifecycle of a plugin instance. (Supports Impl assembly unloading.)
    /// </summary>
    internal interface IPluginSession : IPlugin, IDisposable 
    {
        new int PluginHandle { set; }
    }
}

#endif