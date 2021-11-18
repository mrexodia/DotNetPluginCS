#if ALLOW_UNLOADING

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using DotNetPlugin.Bindings.SDK;

namespace DotNetPlugin
{
    /// <summary>
    /// A proxy for <see cref="PluginSession"/>. (Supports Impl assembly unloading.)
    /// Creates a session in a separate app domain and forward calls to it, thus, enables the Impl assembly
    /// to be unloaded, replaced and reloaded without restarting the host application.
    /// </summary>
    /// <remarks>
    /// We need this because x64dbg's default plugin unloading won't work in the case of .NET libraries.
    /// </remarks>
    internal sealed class PluginSessionProxy : IPluginSession
    {
        private readonly AppDomain _appDomain;
       
        private volatile PluginSession _session;

        public PluginSessionProxy()
        {
            var appDomainSetup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(typeof(PluginMain).Assembly.Location),
                AppDomainInitializer = AppDomainInitializer.Initialize
            };

            _appDomain = AppDomain.CreateDomain("PluginImplDomain", null, appDomainSetup);

            _session = (PluginSession)_appDomain.CreateInstanceAndUnwrap(typeof(PluginSession).Assembly.GetName().Name, typeof(PluginSession).FullName);
        }

        public void Dispose()
        {
            var session = Interlocked.Exchange(ref _session, PluginSession.Null);

            if (session != PluginSession.Null)
            {
                session.Dispose();

                AppDomain.Unload(_appDomain);
            }
        }

        public int PluginVersion => _session.PluginVersion;
        public string PluginName => _session.PluginName;
        public int PluginHandle
        {
            get => _session.PluginHandle;
            set => _session.PluginHandle = value;
        }

        public bool Init() => _session.Init();
        public void Setup(in Plugins.PLUG_SETUPSTRUCT setupStruct) => _session.Setup(setupStruct);
        public bool Stop() => _session.Stop();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnInitDebug(in Plugins.PLUG_CB_INITDEBUG info) => _session.OnInitDebug(in info);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnStopDebug(in Plugins.PLUG_CB_STOPDEBUG info) => _session.OnStopDebug(in info);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCreateProcess(in Plugins.PLUG_CB_CREATEPROCESS info) => _session.OnCreateProcess(in info);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnLoadDll(in Plugins.PLUG_CB_LOADDLL info) => _session.OnLoadDll(in info);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnMenuEntry(in Plugins.PLUG_CB_MENUENTRY info) => _session.OnMenuEntry(in info);
    }
}

#endif