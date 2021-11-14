using System;
using System.Runtime.InteropServices;
using Managed.x64dbg.SDK;

namespace Managed.x64dbg.Script
{
    // https://github.com/x64dbg/x64dbg/blob/development/src/dbg/_scriptapi_module.h
    public static class Module
    {
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

            private fixed byte pathBytes[WAPI.MAX_PATH];
            public string path
            {
                get
                {
                    fixed (byte* ptr = pathBytes)
                        return new IntPtr(ptr).MarshalToStringUTF8(WAPI.MAX_PATH);
                }
            }
        }

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
#else
        private const string dll = "x32dbg.dll";

        private const string Script_Module_GetListEP = "?GetList@Module@Script@@YA_NPAUListInfo@@@Z";
        private const string Script_Module_SectionListFromAddrEP = "?SectionListFromAddr@Module@Script@@YA_NKPAUListInfo@@@Z";
        private const string Script_Module_InfoFromAddrEP = "?InfoFromAddr@Module@Script@@YA_NKPAUModuleInfo@12@@Z";
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
    }
}
