using System;
using System.Runtime.InteropServices;

namespace DotNetPlugin.NativeBindings.SDK
{
    // https://github.com/x64dbg/x64dbg/blob/development/src/bridge/bridgemain.h
    partial class Bridge
    {
        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern bool DbgCmdExec([MarshalAs(UnmanagedType.LPUTF8Str)] string cmd);

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern bool DbgCmdExecDirect([MarshalAs(UnmanagedType.LPUTF8Str)] string cmd);

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern void DbgDisasmFastAt(nuint addr, ref BASIC_INSTRUCTION_INFO basicinfo);

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern nuint DbgGetBranchDestination(nuint addr);

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        private static extern bool DbgGetCommentAt(nuint addr, IntPtr text);

        public static unsafe bool DbgGetCommentAt(nuint addr, out string text)
        {
            var textBufferPtr = stackalloc byte[MAX_COMMENT_SIZE];
            var success = DbgGetCommentAt(addr, new IntPtr(textBufferPtr));
            text = success ? new IntPtr(textBufferPtr).MarshalToStringUTF8(MAX_COMMENT_SIZE) : default;
            return success;
        }

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        private static extern bool DbgGetLabelAt(nuint addr, SEGMENTREG segment, IntPtr text);

        public static unsafe bool DbgGetLabelAt(nuint addr, SEGMENTREG segment, out string text)
        {
            var textBufferPtr = stackalloc byte[MAX_LABEL_SIZE];
            var success = DbgGetLabelAt(addr, segment, new IntPtr(textBufferPtr));
            text = success ? new IntPtr(textBufferPtr).MarshalToStringUTF8(MAX_LABEL_SIZE) : default;
            return success;
        }

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        private static extern bool DbgGetModuleAt(nuint addr, IntPtr text);

        public static unsafe bool DbgGetModuleAt(nuint addr, out string text)
        {
            var textBufferPtr = stackalloc byte[MAX_MODULE_SIZE];
            var success = DbgGetModuleAt(addr, new IntPtr(textBufferPtr));
            text = success ? new IntPtr(textBufferPtr).MarshalToStringUTF8(MAX_MODULE_SIZE) : default;
            return success;
        }

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern bool DbgIsDebugging();

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern nuint DbgModBaseFromName([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern nuint DbgValFromString([MarshalAs(UnmanagedType.LPUTF8Str)] string @string);

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern bool DbgValToString([MarshalAs(UnmanagedType.LPUTF8Str)] string @string, nuint value);

        public enum SEGMENTREG
        {
            SEG_DEFAULT,
            SEG_ES,
            SEG_DS,
            SEG_FS,
            SEG_GS,
            SEG_CS,
            SEG_SS
        }

        #region Definitions for BASIC_INSTRUCTION_INFO.type
        public const uint TYPE_VALUE = 1;
        public const uint TYPE_MEMORY = 2;
        public const uint TYPE_ADDR = 4;
        #endregion

        public enum MEMORY_SIZE
        {
            size_byte = 1,
            size_word = 2,
            size_dword = 4,
            size_qword = 8,
            size_xmmword = 16,
            size_ymmword = 32
        }

        [Serializable]
        public struct VALUE_INFO
        {
            public nuint value;
            public MEMORY_SIZE size;
        }

        [Serializable]
        public unsafe struct MEMORY_INFO
        {
            public nuint value; //displacement / addrvalue (rip-relative)
            public MEMORY_SIZE size; //byte/word/dword/qword

            private fixed byte mnemonicBytes[MAX_MNEMONIC_SIZE];
            public string mnemonic
            {
                get
                {
                    fixed (byte* ptr = mnemonicBytes)
                        return new IntPtr(ptr).MarshalToStringUTF8(MAX_MNEMONIC_SIZE);
                }
            }
        }

        [Serializable]
        public unsafe struct BASIC_INSTRUCTION_INFO
        {
            public uint type; //value|memory|addr
            public VALUE_INFO value; //immediat
            public MEMORY_INFO memory;
            public nuint addr; //addrvalue (jumps + calls)

            private byte branchByte; //jumps/calls
            public bool branch
            {
                get => Convert.ToBoolean(branchByte);
                set => branchByte = Convert.ToByte(value);
            }

            private byte callByte; //instruction is a call
            public bool call
            {
                get => Convert.ToBoolean(callByte);
                set => callByte = Convert.ToByte(value);
            }

            public int size;

            private fixed byte instructionBytes[MAX_MNEMONIC_SIZE * 4];
            public string instruction
            {
                get
                {
                    fixed (byte* ptr = instructionBytes)
                        return new IntPtr(ptr).MarshalToStringUTF8(MAX_MNEMONIC_SIZE * 4);
                }
                set
                {
                    fixed (byte* ptr = instructionBytes)
                        value.MarshalToPtrUTF8(new IntPtr(ptr), MAX_MNEMONIC_SIZE * 4);
                }
            }
        }
    }
}
