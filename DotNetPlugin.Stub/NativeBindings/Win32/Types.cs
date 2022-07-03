using System;
using System.Runtime.InteropServices;

namespace DotNetPlugin.NativeBindings.Win32
{
#pragma warning disable 0649
    
    [Serializable]
    public struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public UIntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
        public uint lPrivate;
    }

    [Serializable]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [Serializable]
    public struct IMAGEHLP_MODULE64
    {
        public uint SizeOfStruct;
        public ulong BaseOfImage;
        public uint ImageSize;
        public uint TimeDateStamp;
        public uint CheckSum;
        public uint NumSyms;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public SYM_TYPE[] SymType;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string ModuleName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string ImageName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string LoadedImageName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string LoadedPdbName;
        public uint CVSig;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 780)]
        public string CVData;
        public uint PdbSig;
        public GUID PdbSig70;
        public uint PdbAge;
        public int PdbUnmatched;
        public int DbgUnmatched;
        public int LineNumbers;
        public int GlobalSymbols;
        public int TypeInfo;
        public int SourceIndexed;
        public int Publics;
    }

    public enum SYM_TYPE
    {
        SymNone,
        SymCoff,
        SymCv,
        SymPdb,
        SymExport,
        SymDeferred,
        SymSym,
        SymDia,
        SymVirtual,
        NumSymTypes,
    }

    [Serializable]
    public struct GUID
    {
        public uint Data1;
        public ushort Data2;
        public ushort Data3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Data4;
    }

    [Serializable]
    public struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public uint dwProcessId;
        public uint dwThreadId;
    }
}
