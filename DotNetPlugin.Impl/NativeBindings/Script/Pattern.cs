using System.Runtime.InteropServices;

namespace DotNetPlugin.NativeBindings.Script
{
    public static class Pattern
    {
#if AMD64
        private const string dll = "x64dbg.dll";

        private const string Script_Pattern_FindMemEP = "?FindMem@Pattern@Script@@YA_K_K0PEBD@Z";
#else
        private const string dll = "x32dbg.dll";

        private const string Script_Pattern_FindMemEP = "?FindMem@Pattern@Script@@YAKKKPBD@Z";
#endif
        private const CallingConvention cdecl = CallingConvention.Cdecl;

        [DllImport(dll, CallingConvention = cdecl, EntryPoint = Script_Pattern_FindMemEP, ExactSpelling = true)]
        private static extern nuint Script_Pattern_FindMem(nuint start, nuint size, [MarshalAs(UnmanagedType.LPUTF8Str)] string pattern);

        public static nuint FindMem(nuint start, nuint size, string pattern) => Script_Pattern_FindMem(start, size, pattern);
    }
}
