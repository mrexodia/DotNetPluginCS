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

#if AMD64
        [DllImport("x64dbg.dll", CallingConvention = CallingConvention.Cdecl,
             EntryPoint = "?GetList@Module@Script@@YA_NPEAUListInfo@@@Z")]
#else
        [DllImport("x32dbg.dll", CallingConvention = CallingConvention.Cdecl,
             EntryPoint = "?GetList@Module@Script@@YA_NPAUListInfo@@@Z")]
#endif

        private static extern bool ScriptModuleGetList(ref Bridge.ListInfo listInfo);

        public static ModuleInfo[] GetList()
        {
            var listInfo = new Bridge.ListInfo();
            return listInfo.ToArray<ModuleInfo>(ScriptModuleGetList(ref listInfo));
        }

#if AMD64
        [DllImport("x64dbg.dll", CallingConvention = CallingConvention.Cdecl,
             EntryPoint = "?SectionListFromAddr@Module@Script@@YA_N_KPEAUListInfo@@@Z")]
#else
        [DllImport("x32dbg.dll", CallingConvention = CallingConvention.Cdecl,
             EntryPoint = "?SectionListFromAddr@Module@Script@@YA_NKPAUListInfo@@@Z")]
#endif

        private static extern bool ScriptModuleSectionListFromAddr(IntPtr addr, ref Bridge.ListInfo listInfo);

        public static ModuleSectionInfo[] SectionListFromAddr(IntPtr addr)
        {
            var listInfo = new Bridge.ListInfo();
            return listInfo.ToArray<ModuleSectionInfo>(ScriptModuleSectionListFromAddr(addr, ref listInfo));
        }

#if AMD64
        [DllImport("x64dbg.dll", CallingConvention = CallingConvention.Cdecl,
             EntryPoint = "?InfoFromAddr@Module@Script@@YA_N_KPEAUModuleInfo@12@@Z")]
#else
        [DllImport("x32dbg.dll", CallingConvention = CallingConvention.Cdecl,
             EntryPoint = "?InfoFromAddr@Module@Script@@YA_NKPAUModuleInfo@12@@Z")]
#endif

        private static extern bool ScriptModuleInfoFromAddr(IntPtr addr, ref ModuleInfo info);

        public static bool InfoFromAddr(IntPtr addr, ref ModuleInfo info)
        {
            return ScriptModuleInfoFromAddr(addr, ref info);
        }
    }
}
