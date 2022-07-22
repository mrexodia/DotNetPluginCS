using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using DotNetPlugin.NativeBindings;
using DotNetPlugin.NativeBindings.SDK;

namespace DotNetPlugin
{
    /// <summary>
    /// Attribute for automatically registering event callbacks in x64Dbg.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class EventCallbackAttribute : Attribute
    {
        public Plugins.CBTYPE EventType { get; }

        public EventCallbackAttribute(Plugins.CBTYPE eventType)
        {
            EventType = eventType;
        }
    }

    internal static class EventCallbacks
    {
        private delegate void Callback<T>(ref T info) where T : unmanaged;

        private delegate void InvokeCallbackDelegate<T>(Callback<T> callback, IntPtr callbackInfo) where T : unmanaged;

        private static readonly MethodInfo s_invokeCallbackMethodDefinition =
            new InvokeCallbackDelegate<Plugins.PLUG_CB_INITDEBUG>(InvokeCallback).Method.GetGenericMethodDefinition();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InvokeCallback<T>(Callback<T> callback, IntPtr callbackInfo) where T : unmanaged =>
            callback(ref callbackInfo.ToStructUnsafe<T>());

        private static Plugins.CBPLUGIN BuildCallback(PluginBase plugin, MethodInfo method, Type eventInfoType)
        {
            object firstArg = method.IsStatic ? null : plugin;

            if (eventInfoType.IsByRef)
            {
                // ref return is not possible with expression trees (https://github.com/dotnet/csharplang/discussions/158),
                // so method can't be called directly, only via an indirection (InvokeCallback<T>)

                eventInfoType = eventInfoType.GetElementType();

                var eventTypeParam = Expression.Parameter(typeof(Plugins.CBTYPE));
                var eventInfoParam = Expression.Parameter(typeof(IntPtr));

                var callbackType = typeof(Callback<>).MakeGenericType(eventInfoType);
                var callback = Delegate.CreateDelegate(callbackType, firstArg, method, throwOnBindFailure: true);

                var callArgs = new Expression[]
                {
                    Expression.Constant(callback, callbackType),
                    eventInfoParam
                };

                method = s_invokeCallbackMethodDefinition.MakeGenericMethod(eventInfoType);
                var call = method.IsStatic ? Expression.Call(method, callArgs) : Expression.Call(Expression.Constant(plugin), method, callArgs);

                var lambda = Expression.Lambda<Plugins.CBPLUGIN>(call, eventTypeParam, eventInfoParam);

                return lambda.Compile();
            }
            else
            {
                var callback = (Action<IntPtr>)Delegate.CreateDelegate(typeof(Action<IntPtr>), firstArg, method, throwOnBindFailure: true);
                return (_, info) => callback(info);
            }
        }

        private static bool IsValidCallbackInfoType(Plugins.CBTYPE eventType, Type eventInfoType) => eventType switch
        {
            Plugins.CBTYPE.CB_INITDEBUG => eventInfoType == typeof(Plugins.PLUG_CB_INITDEBUG),
            Plugins.CBTYPE.CB_STOPDEBUG => eventInfoType == typeof(Plugins.PLUG_CB_STOPDEBUG),
            Plugins.CBTYPE.CB_CREATEPROCESS => eventInfoType == typeof(Plugins.PLUG_CB_CREATEPROCESS),
            Plugins.CBTYPE.CB_EXITPROCESS => eventInfoType == typeof(Plugins.PLUG_CB_EXITPROCESS),
            Plugins.CBTYPE.CB_CREATETHREAD => eventInfoType == typeof(Plugins.PLUG_CB_CREATETHREAD),
            Plugins.CBTYPE.CB_EXITTHREAD => eventInfoType == typeof(Plugins.PLUG_CB_EXITTHREAD),
            Plugins.CBTYPE.CB_SYSTEMBREAKPOINT => eventInfoType == typeof(Plugins.PLUG_CB_SYSTEMBREAKPOINT),
            Plugins.CBTYPE.CB_LOADDLL => eventInfoType == typeof(Plugins.PLUG_CB_LOADDLL),
            Plugins.CBTYPE.CB_UNLOADDLL => eventInfoType == typeof(Plugins.PLUG_CB_UNLOADDLL),
            Plugins.CBTYPE.CB_OUTPUTDEBUGSTRING => eventInfoType == typeof(Plugins.PLUG_CB_OUTPUTDEBUGSTRING),
            Plugins.CBTYPE.CB_EXCEPTION => eventInfoType == typeof(Plugins.PLUG_CB_EXCEPTION),
            Plugins.CBTYPE.CB_BREAKPOINT => eventInfoType == typeof(Plugins.PLUG_CB_BREAKPOINT),
            Plugins.CBTYPE.CB_PAUSEDEBUG => eventInfoType == typeof(Plugins.PLUG_CB_PAUSEDEBUG),
            Plugins.CBTYPE.CB_RESUMEDEBUG => eventInfoType == typeof(Plugins.PLUG_CB_RESUMEDEBUG),
            Plugins.CBTYPE.CB_STEPPED => eventInfoType == typeof(Plugins.PLUG_CB_STEPPED),
            Plugins.CBTYPE.CB_ATTACH => eventInfoType == typeof(Plugins.PLUG_CB_ATTACH),
            Plugins.CBTYPE.CB_DETACH => eventInfoType == typeof(Plugins.PLUG_CB_DETACH),
            Plugins.CBTYPE.CB_DEBUGEVENT => eventInfoType == typeof(Plugins.PLUG_CB_DEBUGEVENT),
            Plugins.CBTYPE.CB_MENUENTRY => eventInfoType == typeof(Plugins.PLUG_CB_MENUENTRY),
            Plugins.CBTYPE.CB_WINEVENT => eventInfoType == typeof(Plugins.PLUG_CB_WINEVENT),
            Plugins.CBTYPE.CB_WINEVENTGLOBAL => eventInfoType == typeof(Plugins.PLUG_CB_WINEVENTGLOBAL),
            Plugins.CBTYPE.CB_LOADDB => eventInfoType == typeof(Plugins.PLUG_CB_LOADSAVEDB),
            Plugins.CBTYPE.CB_SAVEDB => eventInfoType == typeof(Plugins.PLUG_CB_LOADSAVEDB),
            Plugins.CBTYPE.CB_FILTERSYMBOL => eventInfoType == typeof(Plugins.PLUG_CB_FILTERSYMBOL),
            Plugins.CBTYPE.CB_TRACEEXECUTE => eventInfoType == typeof(Plugins.PLUG_CB_TRACEEXECUTE),
            Plugins.CBTYPE.CB_SELCHANGED => eventInfoType == typeof(Plugins.PLUG_CB_SELCHANGED),
            Plugins.CBTYPE.CB_ANALYZE => eventInfoType == typeof(Plugins.PLUG_CB_ANALYZE),
            Plugins.CBTYPE.CB_ADDRINFO => eventInfoType == typeof(Plugins.PLUG_CB_ADDRINFO),
            Plugins.CBTYPE.CB_VALFROMSTRING => eventInfoType == typeof(Plugins.PLUG_CB_VALFROMSTRING),
            Plugins.CBTYPE.CB_VALTOSTRING => eventInfoType == typeof(Plugins.PLUG_CB_VALTOSTRING),
            Plugins.CBTYPE.CB_MENUPREPARE => eventInfoType == typeof(Plugins.PLUG_CB_MENUPREPARE),
            Plugins.CBTYPE.CB_STOPPINGDEBUG => eventInfoType == typeof(Plugins.PLUG_CB_STOPDEBUG),
            _ => false
        };

        public static IDisposable Initialize(PluginBase plugin, MethodInfo[] pluginMethods)
        {
            var registeredEventTypes = new HashSet<Plugins.CBTYPE>();

            var methods = pluginMethods
                .SelectMany(method => method.GetCustomAttributes<EventCallbackAttribute>().Select(attribute => (method, attribute)));

            foreach (var (method, attribute) in methods)
            {
                var eventType = attribute.EventType;

                if (method.ReturnType != typeof(void))
                {
                    PluginBase.LogError($"Registration of event callback {eventType} is skipped. Method '{method.Name}' has an invalid return type.");
                    continue;
                }

                var methodParams = method.GetParameters();
                ParameterInfo eventInfoParam;
                Type eventInfoType;
                if (methodParams.Length != 1 ||
                    (eventInfoType = (eventInfoParam = methodParams[0]).ParameterType) != typeof(IntPtr) &&
                     !(eventInfoType.IsByRef && !eventInfoParam.IsIn && !eventInfoParam.IsOut && IsValidCallbackInfoType(eventType, eventInfoType.GetElementType())))
                {
                    PluginBase.LogError($"Registration of event callback {eventType} is skipped. Method '{method.Name}' has an invalid signature.");
                    continue;
                }

                if (registeredEventTypes.Contains(eventType))
                {
                    PluginBase.LogError($"Registration of event callback {eventType} failed.");
                    continue;
                }

                Plugins._plugin_registercallback(plugin.PluginHandle, eventType, BuildCallback(plugin, method, eventInfoType));

                registeredEventTypes.Add(eventType);

                PluginBase.LogInfo($"Event callback {eventType} registered!");
            }

            return new Registrations(plugin, registeredEventTypes);
        }

        private sealed class Registrations : IDisposable
        {
            private PluginBase _plugin;
            private HashSet<Plugins.CBTYPE> _registeredEventTypes;

            public Registrations(PluginBase plugin, HashSet<Plugins.CBTYPE> registeredEventTypes)
            {
                _plugin = plugin;
                _registeredEventTypes = registeredEventTypes;
            }

            public void Dispose()
            {
                var plugin = Interlocked.Exchange(ref _plugin, null);

                if (plugin != null)
                {
                    foreach (var eventType in _registeredEventTypes)
                    {
                        if (Plugins._plugin_unregistercallback(plugin.PluginHandle, eventType))
                            PluginBase.LogInfo($"Event callback {eventType} unregistered!");
                        else
                            PluginBase.LogError($"Unregistration of event callback {eventType} failed.");
                    }

                    _registeredEventTypes = null;
                }
            }
        }
    }
}