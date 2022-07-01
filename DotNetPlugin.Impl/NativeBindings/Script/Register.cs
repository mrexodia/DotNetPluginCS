using System;
using System.Runtime.InteropServices;

namespace DotNetPlugin.NativeBindings.Script
{
    public static class Register
    {
#if AMD64
        private const string dll = "x64dbg.dll";

        private const string ScriptRegisterGetCIP = "?GetCIP@Register@Script@@YA_KXZ";
        private const string ScriptRegisterGetCSP = "?GetCSP@Register@Script@@YA_KXZ";
#else
        private const string dll = "x32dbg.dll";

        private const string ScriptRegisterGetCIP = "?GetCIP@Register@Script@@YAKXZ";
        private const string ScriptRegisterGetCSP = "?GetCSP@Register@Script@@YAKXZ";
#endif
        private const CallingConvention cdecl = CallingConvention.Cdecl;

        [DllImport(dll, CallingConvention = cdecl, EntryPoint = ScriptRegisterGetCIP, ExactSpelling = true)]
        public static extern UIntPtr GetCIP();

        [DllImport(dll, CallingConvention = cdecl, EntryPoint = ScriptRegisterGetCSP, ExactSpelling = true)]
        public static extern UIntPtr GetCSP();
    }
}
