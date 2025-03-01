using DotNetPlugin.NativeBindings.SDK;
using System;
using System.Runtime.InteropServices;

namespace DotNetPlugin.NativeBindings.Script
{
    public static class Argument
    {
        [Serializable]
        public unsafe struct ArgumentInfo
        {
            private fixed byte _mod[BridgeBase.MAX_MODULE_SIZE];
            public string mod
            {
                get
                {
                    fixed (byte* ptr = _mod)
                        return new IntPtr(ptr).MarshalToStringUTF8(Bridge.MAX_MODULE_SIZE);
                }
                set
                {
                    fixed (byte* ptr = _mod)
                        value.MarshalToPtrUTF8(new IntPtr(ptr), Bridge.MAX_MODULE_SIZE * 4);
                }
            }

            nuint rvaStart;
            nuint rvaEnd;
            bool manual;
            nuint instructioncount;
        };

#if AMD64
        private const string dll = "x64dbg.dll";

        private const string Script_Argument_DeleteRangeEP = "?DeleteRange@Argument@Script@@YAX_K0_N@Z";
#else
        private const string dll = "x32dbg.dll";

        private const string Script_Argument_DeleteRangeEP = "?DeleteRange@Argument@Script@@YAXKK_N@Z";
#endif
        private const CallingConvention cdecl = CallingConvention.Cdecl;

        [DllImport(dll, CallingConvention = cdecl, EntryPoint = Script_Argument_DeleteRangeEP, ExactSpelling = true)]
        private static extern void Script_Argument_DeleteRange(nuint start, nuint end, bool deleteManual = false);

        public static void DeleteRange(nuint start, nuint end, bool deleteManual = false) => 
            Script_Argument_DeleteRange(start, end, deleteManual);
    }
}
