using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using DotNetPlugin.NativeBindings.SDK;

namespace DotNetPlugin
{
    /// <summary>
    /// Attribute for automatically registering expression functions in x64Dbg.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ExpressionFunctionAttribute : Attribute
    {
        public string Name { get; }

        public ExpressionFunctionAttribute() { }

        public ExpressionFunctionAttribute(string name)
        {
            Name = name;
        }
    }

    internal static class ExpressionFunctions
    {
        private static readonly MethodInfo s_marshalReadIntPtrMethod = new Func<IntPtr, int, IntPtr>(Marshal.ReadIntPtr).Method;
        private static readonly MethodInfo s_intPtrToUIntPtrMethod = new Func<IntPtr, UIntPtr>(IntPtrToUIntPtr).Method;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UIntPtr IntPtrToUIntPtr(IntPtr value) => (nuint)(nint)value;

        private static Plugins.CBPLUGINEXPRFUNCTION_RAWARGS BuildCallback(PluginBase plugin, MethodInfo method, int methodParamCount)
        {
            var argcParam = Expression.Parameter(typeof(int));
            var argvParam = Expression.Parameter(typeof(IntPtr));
            var userdataParam = Expression.Parameter(typeof(object));

            var callArgs = Enumerable.Range(0, methodParamCount)
                .Select((param, i) => Expression.Call(
                    s_intPtrToUIntPtrMethod,
                    Expression.Call(s_marshalReadIntPtrMethod, argvParam, Expression.Constant(i * IntPtr.Size))))
                .ToArray();

            var call = method.IsStatic ? Expression.Call(method, callArgs) : Expression.Call(Expression.Constant(plugin), method, callArgs);

            var lambda = Expression.Lambda<Plugins.CBPLUGINEXPRFUNCTION_RAWARGS>(call, argcParam, argvParam, userdataParam);

            return lambda.Compile();
        }

        public static IDisposable Initialize(PluginBase plugin, MethodInfo[] pluginMethods)
        {
            // expression function names are case-sensitive
            var registeredNames = new HashSet<string>();

            var methods = pluginMethods
                .SelectMany(method => method.GetCustomAttributes<ExpressionFunctionAttribute>().Select(attribute => (method, attribute)));

            foreach (var (method, attribute) in methods)
            {
                var name = attribute.Name ?? method.Name;

                if (method.ReturnType != typeof(UIntPtr))
                {
                    PluginBase.LogError($"Registration of expression function '{name}' is skipped. Method '{method.Name}' has an invalid return type.");
                    continue;
                }

                var methodParams = method.GetParameters();
                if (methodParams.Any(param => param.ParameterType != typeof(UIntPtr)))
                {
                    PluginBase.LogError($"Registration of expression function '{name}' is skipped. Method '{method.Name}' has an invalid signature.");
                    continue;
                }

                if (registeredNames.Contains(name) ||
                    !Plugins._plugin_registerexprfunction(plugin.PluginHandle, name, methodParams.Length, BuildCallback(plugin, method, methodParams.Length), null))
                {
                    PluginBase.LogError($"Registration of expression function '{name}' failed.");
                    continue;
                }

                registeredNames.Add(name);
            }

            return new Registrations(plugin, registeredNames);
        }

        private sealed class Registrations : IDisposable
        {
            private PluginBase _plugin;
            private HashSet<string> _registeredNames;

            public Registrations(PluginBase plugin, HashSet<string> registeredNames)
            {
                _plugin = plugin;
                _registeredNames = registeredNames;
            }

            public void Dispose()
            {
                var plugin = Interlocked.Exchange(ref _plugin, null);

                if (plugin != null)
                {
                    foreach (var name in _registeredNames)
                    {
                        if (!Plugins._plugin_unregisterexprfunction(plugin.PluginHandle, name))
                            PluginBase.LogError($"Unregistration of expression function '{name}' failed.");
                    }

                    _registeredNames = null;
                }
            }
        }
    }
}