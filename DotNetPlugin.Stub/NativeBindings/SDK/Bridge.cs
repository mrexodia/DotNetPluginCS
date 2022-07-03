using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DotNetPlugin.NativeBindings.SDK
{
    // https://github.com/x64dbg/x64dbg/blob/development/src/bridge/bridgemain.h
    public class BridgeBase
    {
        public const int MAX_LABEL_SIZE = 256;
        public const int MAX_COMMENT_SIZE = 512;
        public const int MAX_MODULE_SIZE = 256;
        public const int MAX_IMPORT_SIZE = 65536;
        public const int MAX_BREAKPOINT_SIZE = 256;
        public const int MAX_CONDITIONAL_EXPR_SIZE = 256;
        public const int MAX_CONDITIONAL_TEXT_SIZE = 256;
        public const int MAX_SCRIPT_LINE_SIZE = 2048;
        public const int MAX_THREAD_NAME_SIZE = 256;
        public const int MAX_WATCH_NAME_SIZE = 256;
        public const int MAX_STRING_SIZE = 512;
        public const int MAX_ERROR_SIZE = 512;
        public const int MAX_SECTION_SIZE = 10;
        public const int MAX_COMMAND_LINE_SIZE = 256;
        public const int MAX_MNEMONIC_SIZE = 64;
        public const int PAGE_SIZE = 4096;

#if AMD64
        protected const string dll = "x64bridge.dll";
#else
        protected const string dll = "x32bridge.dll";
#endif
        protected const CallingConvention cdecl = CallingConvention.Cdecl;

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern IntPtr BridgeAlloc(nuint size);

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern void BridgeFree(IntPtr ptr);

        protected BridgeBase() { }

#pragma warning disable 0649

        public enum GUIMENUTYPE
        {
            GUI_PLUGIN_MENU,
            GUI_DISASM_MENU,
            GUI_DUMP_MENU,
            GUI_STACK_MENU,
            GUI_GRAPH_MENU,
            GUI_MEMMAP_MENU,
            GUI_SYMMOD_MENU,
        }

        [Serializable]
        public struct BridgeCFGraphList
        {
            public nuint entryPoint; //graph entry point
            public IntPtr userdata; //user data
            public ListInfo nodes; //graph nodes (BridgeCFNodeList)
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

        [Serializable]
        public struct FUNCTION
        {
            public nuint start; //OUT
            public nuint end; //OUT
            public nuint instrcount; //OUT
        }

        [Serializable]
        public struct LOOP
        {
            public int depth; //IN
            public nuint start; //OUT
            public nuint end; //OUT
            public nuint instrcount; //OUT
        }

        [Serializable]
        public unsafe struct BRIDGE_ADDRINFO
        {
            public int flags; //ADDRINFOFLAGS (IN)

            private fixed byte moduleBytes[MAX_MODULE_SIZE]; //module the address is in
            public string module
            {
                get
                {
                    fixed (byte* ptr = moduleBytes)
                        return new IntPtr(ptr).MarshalToStringUTF8(MAX_MODULE_SIZE);
                }
            }

            private fixed byte labelBytes[MAX_LABEL_SIZE];
            public string label
            {
                get
                {
                    fixed (byte* ptr = labelBytes)
                        return new IntPtr(ptr).MarshalToStringUTF8(MAX_LABEL_SIZE);
                }
            }

            private fixed byte commentBytes[MAX_COMMENT_SIZE];
            public string comment
            {
                get
                {
                    fixed (byte* ptr = commentBytes)
                        return new IntPtr(ptr).MarshalToStringUTF8(MAX_COMMENT_SIZE);
                }
            }

            private byte isbookmarkByte;
            public bool isbookmark => Convert.ToBoolean(isbookmarkByte);

            public FUNCTION function;
            public LOOP loop;
            public FUNCTION args;
        }
    }
}
