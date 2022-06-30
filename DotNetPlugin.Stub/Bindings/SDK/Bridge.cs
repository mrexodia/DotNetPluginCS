using System;
using System.IO;
using System.Runtime.InteropServices;
using DotNetPlugin.Bindings.SDK;

namespace DotNetPlugin.Bindings.SDK
{
    // https://github.com/x64dbg/x64dbg/blob/development/src/bridge/bridgemain.h
    public static partial class Bridge
    {
#if AMD64
        private const string dll = "x64bridge.dll";
#else
        private const string dll = "x32bridge.dll";
#endif
        private const CallingConvention cdecl = CallingConvention.Cdecl;

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern IntPtr BridgeAlloc(nuint size);

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern void BridgeFree(IntPtr ptr);

        [Serializable]
        public struct ICONDATA
        {
            public IntPtr data;
            public nuint size;
        }

        // https://github.com/x64dbg/x64dbg/blob/development/src/bridge/bridgelist.h
        [Serializable]
        public struct ListInfo
        {
            public int count;
            public nuint size;
            public IntPtr data;

            public T[] ToArray<T>(bool success) where T : new()
            {
                if (!success || count == 0 || size == 0)
                    return Array.Empty<T>();
                var list = new T[count];
                var szt = Marshal.SizeOf(typeof(T));
                var sz = checked((int)(size / (nuint)count));
                if (szt != sz)
                    throw new InvalidDataException(string.Format("{0} type size mismatch, expected {1} got {2}!",
                        typeof(T).Name, szt, sz));
                var ptr = data;
                for (var i = 0; i < count; i++)
                {
                    list[i] = (T)Marshal.PtrToStructure(ptr, typeof(T));
                    ptr += sz;
                }
                BridgeFree(data);
                return list;
            }
        }
    }
}
