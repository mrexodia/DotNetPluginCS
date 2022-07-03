using System;
using System.Runtime.InteropServices;

namespace DotNetPlugin.NativeBindings.Win32
{
#pragma warning disable 0649

    [Serializable]
    public unsafe struct EXCEPTION_RECORD
    {
        public const int EXCEPTION_MAXIMUM_PARAMETERS = 15;

        public uint ExceptionCode;
        public uint ExceptionFlags;

        private IntPtr ExceptionRecordPtr;
        public StructRef<EXCEPTION_RECORD> ExceptionRecord => new StructRef<EXCEPTION_RECORD>(ExceptionRecordPtr);

        public IntPtr ExceptionAddress;
        public uint NumberParameters;

#if AMD64
        private fixed ulong ExceptionInformationFixed[EXCEPTION_MAXIMUM_PARAMETERS];
#else
        private fixed uint ExceptionInformationFixed[EXCEPTION_MAXIMUM_PARAMETERS];
#endif

        public UIntPtr[] GetExceptionInformation(UIntPtr[] array)
        {
            if (array == null)
                array = new UIntPtr[EXCEPTION_MAXIMUM_PARAMETERS];

#if AMD64
        fixed (ulong* ptr = ExceptionInformationFixed)
#else
        fixed (uint* ptr = ExceptionInformationFixed)
#endif
            {
                var p = ptr;
                for (int i = 0, n = Math.Min(array.Length, EXCEPTION_MAXIMUM_PARAMETERS); i < n; i++, p++)
                    array[i] = new UIntPtr(*p);
            }

            return array;
        }
    }

    [Serializable]
    public struct EXCEPTION_DEBUG_INFO
    {
        public EXCEPTION_RECORD ExceptionRecord;
        public uint dwFirstChance;
    }

    [Serializable]
    public struct CREATE_THREAD_DEBUG_INFO
    {
        public IntPtr hThread;
        public IntPtr lpThreadLocalBase;
        public IntPtr lpStartAddress; // PTHREAD_START_ROUTINE
    }

    [Serializable]
    public struct CREATE_PROCESS_DEBUG_INFO
    {
        public IntPtr hFile;
        public IntPtr hProcess;
        public IntPtr hThread;
        public IntPtr lpBaseOfImage;
        public uint dwDebugInfoFileOffset;
        public uint nDebugInfoSize;
        public IntPtr lpThreadLocalBase;
        public IntPtr lpStartAddress; //PTHREAD_START_ROUTINE
        public IntPtr lpImageName;
        public ushort fUnicode;
    }

    [Serializable]
    public struct EXIT_THREAD_DEBUG_INFO
    {
        public uint dwExitCode;
    }

    [Serializable]
    public struct EXIT_PROCESS_DEBUG_INFO
    {
        public uint dwExitCode;
    }

    [Serializable]
    public struct LOAD_DLL_DEBUG_INFO
    {
        public IntPtr hFile;
        public IntPtr lpBaseOfDll;
        public uint dwDebugInfoFileOffset;
        public uint nDebugInfoSize;
        public IntPtr lpImageName;
        public ushort fUnicode;
    }

    [Serializable]
    public struct UNLOAD_DLL_DEBUG_INFO
    {
        public IntPtr lpBaseOfDll;
    }

    [Serializable]
    public struct OUTPUT_DEBUG_STRING_INFO
    {
        public IntPtr lpDebugStringData;
        public ushort fUnicode;
        public ushort nDebugStringLength;
    }

    [Serializable]
    public struct RIP_INFO
    {
        public uint dwError;
        public uint dwType;
    }

    public enum DebugEventType : uint
    {
        EXCEPTION_DEBUG_EVENT = 1,
        CREATE_THREAD_DEBUG_EVENT = 2,
        CREATE_PROCESS_DEBUG_EVENT = 3,
        EXIT_THREAD_DEBUG_EVENT = 4,
        EXIT_PROCESS_DEBUG_EVENT = 5,
        LOAD_DLL_DEBUG_EVENT = 6,
        UNLOAD_DLL_DEBUG_EVENT = 7,
        OUTPUT_DEBUG_STRING_EVENT = 8,
        RIP_EVENT = 9,
    }

    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct DEBUG_EVENT_UNION
    {
        [FieldOffset(0)] public EXCEPTION_DEBUG_INFO Exception;
        [FieldOffset(0)] public CREATE_THREAD_DEBUG_INFO CreateThread;
        [FieldOffset(0)] public CREATE_PROCESS_DEBUG_INFO CreateProcessInfo;
        [FieldOffset(0)] public EXIT_THREAD_DEBUG_INFO ExitThread;
        [FieldOffset(0)] public EXIT_PROCESS_DEBUG_INFO ExitProcess;
        [FieldOffset(0)] public LOAD_DLL_DEBUG_INFO LoadDll;
        [FieldOffset(0)] public UNLOAD_DLL_DEBUG_INFO UnloadDll;
        [FieldOffset(0)] public OUTPUT_DEBUG_STRING_INFO DebugString;
        [FieldOffset(0)] public RIP_INFO RipInfo;
    }

    [Serializable]
    public struct DEBUG_EVENT
    {
        public DebugEventType dwDebugEventCode;
        public int dwProcessId;
        public int dwThreadId;

        public DEBUG_EVENT_UNION u;
    }
}
