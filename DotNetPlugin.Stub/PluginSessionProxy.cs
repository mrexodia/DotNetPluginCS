#if ALLOW_UNLOADING

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DotNetPlugin.NativeBindings.SDK;

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

        private readonly WaitCallback _implChangedCallback;
        private readonly CancellationTokenSource _implChangeWatcherCts;
        private volatile Task _watchForImplChangeTask;

        private volatile PluginSession _session;

        public PluginSessionProxy(WaitCallback implChangedCallback)
        {
            var appDomainSetup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(typeof(PluginMain).Assembly.Location),
                AppDomainInitializer = AppDomainInitializer.Initialize
            };

            _appDomain = AppDomain.CreateDomain("PluginImplDomain", null, appDomainSetup);

            _session = (PluginSession)_appDomain.CreateInstanceAndUnwrap(typeof(PluginSession).Assembly.GetName().Name, typeof(PluginSession).FullName);

            _implChangedCallback = implChangedCallback;
            _implChangeWatcherCts = new CancellationTokenSource();
            _watchForImplChangeTask = Task.CompletedTask;
        }

        public void Dispose() => Stop();

        public int PluginVersion => _session.PluginVersion;
        public string PluginName => _session.PluginName;
        public int PluginHandle
        {
            get => _session.PluginHandle;
            set => _session.PluginHandle = value;
        }

        private async Task WatchForImplChangeAsync()
        {
            if (PluginMain.ImplAssemblyLocation == null)
                return;

            RestartWatch:

            FileSystemWatcher fsw;

            try { fsw = new FileSystemWatcher(Path.GetDirectoryName(PluginMain.ImplAssemblyLocation), Path.GetFileName(PluginMain.ImplAssemblyLocation)); }
            catch
            {
                await Task.Delay(1000, _implChangeWatcherCts.Token).ConfigureAwait(false);
                goto RestartWatch;
            }

            using (fsw)
            {
                var changedTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

                fsw.Created += delegate { changedTcs.TrySetResult(null); };
                fsw.Changed += delegate { changedTcs.TrySetResult(null); };
                fsw.Renamed += delegate { changedTcs.TrySetResult(null); };
                fsw.Deleted += delegate { changedTcs.TrySetResult(null); };

                fsw.Error += (_, e) => changedTcs.TrySetException(e.GetException());

                _implChangeWatcherCts.Token.Register(() => changedTcs.TrySetCanceled(_implChangeWatcherCts.Token));

                fsw.EnableRaisingEvents = true;

                try { await changedTcs.Task.ConfigureAwait(false); }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    await Task.Delay(1000, _implChangeWatcherCts.Token).ConfigureAwait(false);
                    goto RestartWatch;
                }
            }

            await Task.Delay(500, _implChangeWatcherCts.Token).ConfigureAwait(false);

            RetryFileAccess:

            try 
            { 
                if (File.Exists(PluginMain.ImplAssemblyLocation))
                    File.OpenRead(PluginMain.ImplAssemblyLocation).Dispose(); 
            }
            catch
            {
                await Task.Delay(1000, _implChangeWatcherCts.Token).ConfigureAwait(false);
                goto RetryFileAccess;
            }

            ThreadPool.QueueUserWorkItem(_implChangedCallback, this);
        }

        public bool Init()
        {
            var result = _session.Init();

            if (result)
            {
                _watchForImplChangeTask = WatchForImplChangeAsync();
            }

            return result;
        }

        public void Setup(in Plugins.PLUG_SETUPSTRUCT setupStruct) => _session.Setup(setupStruct);

        private bool StopCore(PluginSession session)
        {
            try
            {
                _implChangeWatcherCts.Cancel();

                var watchForImplChangeTask = Interlocked.Exchange(ref _watchForImplChangeTask, Task.CompletedTask);
                var sessionStopTask = Task.Factory.StartNew(session.Stop, TaskCreationOptions.LongRunning);

                var pendingTasks = Task.WhenAll(watchForImplChangeTask, sessionStopTask);

                if (Task.WhenAny(pendingTasks, Task.Delay(5000)).ConfigureAwait(false).GetAwaiter().GetResult() == pendingTasks)
                {
                    if (pendingTasks.IsFaulted)
                        pendingTasks.ConfigureAwait(false).GetAwaiter().GetResult(); // unwraps exception
                    else
                        return sessionStopTask.ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                PluginMain.LogUnhandledException(ex);
            }

            return false;
        }

        public bool Stop()
        {
            var session = Interlocked.Exchange(ref _session, PluginSession.Null);

            if (session == PluginSession.Null)
                return true;

            var result = StopCore(session);

            _implChangeWatcherCts.Dispose();

            AppDomain.Unload(_appDomain);

            return result;
        }

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