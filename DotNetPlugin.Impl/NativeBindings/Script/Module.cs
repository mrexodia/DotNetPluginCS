using System;
using System.Runtime.InteropServices;
using DotNetPlugin.NativeBindings.SDK;
using DotNetPlugin.NativeBindings.Win32;

namespace DotNetPlugin.NativeBindings.Script
{
    // https://github.com/x64dbg/x64dbg/blob/development/src/dbg/_scriptapi_module.h
    public static class Module
    {
        [Serializable]
        public unsafe struct ModuleInfo
        {
            public nuint @base;
            public nuint size;
            public nuint entry;
            public int sectionCount;

            private fixed byte nameBytes[Bridge.MAX_MODULE_SIZE];
            public string name
            {
                get
                {
                    fixed (byte* ptr = nameBytes)
                        return new IntPtr(ptr).MarshalToStringUTF8(Bridge.MAX_MODULE_SIZE);
                }
            }

            private fixed byte pathBytes[Win32Constants.MAX_PATH];
            public string path
            {
                get
                {
                    fixed (byte* ptr = pathBytes)
                        return new IntPtr(ptr).MarshalToStringUTF8(Win32Constants.MAX_PATH);
                }
            }
        }

        [Serializable]
        public unsafe struct ModuleSectionInfo
        {
            public nuint addr;
            public nuint size;

            private fixed byte nameBytes[Bridge.MAX_SECTION_SIZE * 5];
            public string name
            {
                get
                {
                    fixed (byte* ptr = nameBytes)
                        return new IntPtr(ptr).MarshalToStringUTF8(Bridge.MAX_SECTION_SIZE * 5);
                }
            }
        }

#if AMD64
        private const string dll = "x64dbg.dll";

        private const string Script_Module_GetListEP = "?GetList@Module@Script@@YA_NPEAUListInfo@@@Z";
        private const string Script_Module_SectionListFromAddrEP = "?SectionListFromAddr@Module@Script@@YA_N_KPEAUListInfo@@@Z";
        private const string Script_Module_InfoFromAddrEP = "?InfoFromAddr@Module@Script@@YA_N_KPEAUModuleInfo@12@@Z";
        private const string Script_Module_NameFromAddrEP = "?NameFromAddr@Module@Script@@YA_N_KPEAD@Z";
        private const string Script_Module_BaseFromAddrEP = "?BaseFromAddr@Module@Script@@YA_K_K@Z";
        private const string Script_Module_EntryFromAddrEP = "?EntryFromAddr@Module@Script@@YA_K_K@Z";
        private const string Script_Module_SectionFromNameEP = "?SectionFromName@Module@Script@@YA_NPEBDHPEAUModuleSectionInfo@12@@Z";
        private const string Script_Module_GetMainModuleInfoEP = "?GetMainModuleInfo@Module@Script@@YA_NPEAUModuleInfo@12@@Z";
        private const string Script_Module_GetMainModuleNameEP = "?GetMainModuleName@Module@Script@@YA_NPEAD@Z";
#else
        private const string dll = "x32dbg.dll";

        private const string Script_Module_GetListEP = "?GetList@Module@Script@@YA_NPAUListInfo@@@Z";
        private const string Script_Module_SectionListFromAddrEP = "?SectionListFromAddr@Module@Script@@YA_NKPAUListInfo@@@Z";
        private const string Script_Module_InfoFromAddrEP = "?InfoFromAddr@Module@Script@@YA_NKPAUModuleInfo@12@@Z";
        private const string Script_Module_NameFromAddrEP = "?NameFromAddr@Module@Script@@YA_NKPAD@Z";
        private const string Script_Module_BaseFromAddrEP = "?BaseFromAddr@Module@Script@@YAKK@Z";
        private const string Script_Module_EntryFromAddrEP = "?EntryFromAddr@Module@Script@@YAKK@Z";
        private const string Script_Module_SectionFromNameEP = "?SectionFromName@Module@Script@@YA_NPBDHPAUModuleSectionInfo@12@@Z";
        private const string Script_Module_GetMainModuleInfoEP = "?GetMainModuleInfo@Module@Script@@YA_NPAUModuleInfo@12@@Z";
        private const string Script_Module_GetMainModuleNameEP = "?GetMainModuleName@Module@Script@@YA_NPAD@Z";
#endif
        private const CallingConvention cdecl = CallingConvention.Cdecl;

        [DllImport(dll, CallingConvention = cdecl, EntryPoint = Script_Module_GetListEP, ExactSpelling = true)]
        private static extern bool Script_Module_GetList(ref Bridge.ListInfo listInfo);

        public static ModuleInfo[] GetList()
        {
            var listInfo = new Bridge.ListInfo();
            return listInfo.ToArray<ModuleInfo>(Script_Module_GetList(ref listInfo));
        }

        [DllImport(dll, CallingConvention = cdecl, EntryPoint = Script_Module_SectionListFromAddrEP, ExactSpelling = true)]
        private static extern bool Script_Module_SectionListFromAddr(nuint addr, ref Bridge.ListInfo listInfo);

        public static ModuleSectionInfo[] SectionListFromAddr(nuint addr)
        {
            var listInfo = new Bridge.ListInfo();
            return listInfo.ToArray<ModuleSectionInfo>(Script_Module_SectionListFromAddr(addr, ref listInfo));
        }

        [DllImport(dll, CallingConvention = cdecl, EntryPoint = Script_Module_InfoFromAddrEP, ExactSpelling = true)]
        private static extern bool Script_Module_InfoFromAddr(nuint addr, ref ModuleInfo info);

        public static bool InfoFromAddr(nuint addr, ref ModuleInfo info)
        {
            return Script_Module_InfoFromAddr(addr, ref info);
        }

        [DllImport(dll, CallingConvention = cdecl, EntryPoint = Script_Module_NameFromAddrEP, ExactSpelling = true)]
        private static extern bool Script_Module_NameFromAddr(nuint addr, IntPtr name);

        public static bool NameFromAddr(nuint addr, out string name)
        {
            var nameBuffer = Marshal.AllocHGlobal(Bridge.MAX_MODULE_SIZE);
            try
            {
                var success = Script_Module_NameFromAddr(addr, nameBuffer);
                name = success ? nameBuffer.MarshalToStringUTF8(Bridge.MAX_MODULE_SIZE) : default;
                return success;
            }
            finally
            {
                Marshal.FreeHGlobal(nameBuffer);
            }
        }

        [DllImport(dll, CallingConvention = cdecl, EntryPoint = Script_Module_BaseFromAddrEP, ExactSpelling = true)]
        private static extern nuint Script_Module_BaseFromAddr(nuint addr);

        public static nuint BaseFromAddr(nuint addr) => Script_Module_BaseFromAddr(addr);

        [DllImport(dll, CallingConvention = cdecl, EntryPoint = Script_Module_EntryFromAddrEP, ExactSpelling = true)]
        private static extern nuint Script_Module_EntryFromAddr(nuint addr);

        public static nuint EntryFromAddr(nuint addr) => Script_Module_EntryFromAddr(addr);

        [DllImport(dll, CallingConvention = cdecl, EntryPoint = Script_Module_SectionFromNameEP, ExactSpelling = true)]
        private static extern bool Script_Module_SectionFromName(
            [MarshalAs(UnmanagedType.LPUTF8Str)] string name, 
            int number, 
            ref ModuleSectionInfo section);

        public static bool SectionFromName(string name, int number, ref ModuleSectionInfo section) => 
            Script_Module_SectionFromName(name, number, ref section);

        [DllImport(dll, CallingConvention = cdecl, EntryPoint = Script_Module_GetMainModuleInfoEP, ExactSpelling = true)]
        private static extern bool Script_Module_GetMainModuleInfo(ref ModuleInfo info);

        public static bool GetMainModuleInfo(ref ModuleInfo info) => Script_Module_GetMainModuleInfo(ref info);

        [DllImport(dll, CallingConvention = cdecl, EntryPoint = Script_Module_GetMainModuleNameEP, ExactSpelling = true)]
        private static extern bool Script_Module_GetMainModuleName(IntPtr name);

        public static bool GetMainModuleName(out string name)
        {
            var nameBuffer = Marshal.AllocHGlobal(Bridge.MAX_MODULE_SIZE);
            try
            {
                var success = Script_Module_GetMainModuleName(nameBuffer);
                name = success ? nameBuffer.MarshalToStringUTF8(Bridge.MAX_MODULE_SIZE) : default;
                return success;
            }
            finally
            {
                Marshal.FreeHGlobal(nameBuffer);
            }
        }
    }
}
