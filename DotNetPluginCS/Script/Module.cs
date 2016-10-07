using System;
using System.Runtime.InteropServices;
using DotNetPlugin.SDK;

namespace DotNetPlugin.Script
{
    public static class Module
    {
        public struct ModuleInfo
        {
            public IntPtr @base;
            public IntPtr size;
            public IntPtr entry;
            public int sectionCount;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Bridge.MAX_MODULE_SIZE)]
            public string name;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = WAPI.MAX_PATH)]
            public string path;
        }

        public struct ModuleSectionInfo
        {
            public IntPtr addr;
            public IntPtr size;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Bridge.MAX_SECTION_SIZE * 5)]
            public string name;
        }

        [DllImport("x64dbg.dll", CallingConvention = CallingConvention.Cdecl,
             EntryPoint = "?GetList@Module@Script@@YA_NPEAUListInfo@@@Z")]
        private static extern bool ScriptModuleGetList(ref Bridge.ListInfo listInfo);

        public static ModuleInfo[] GetList()
        {
            var listInfo = new Bridge.ListInfo();
            return listInfo.ToArray<ModuleInfo>(ScriptModuleGetList(ref listInfo));
        }

        [DllImport("x64dbg.dll", CallingConvention = CallingConvention.Cdecl,
             EntryPoint = "?SectionListFromAddr@Module@Script@@YA_N_KPEAUListInfo@@@Z")]
        private static extern bool ScriptModuleSectionListFromAddr(IntPtr addr, ref Bridge.ListInfo listInfo);

        public static ModuleSectionInfo[] SectionListFromAddr(IntPtr addr)
        {
            var listInfo = new Bridge.ListInfo();
            return listInfo.ToArray<ModuleSectionInfo>(ScriptModuleSectionListFromAddr(addr, ref listInfo));
        }
    }
}
