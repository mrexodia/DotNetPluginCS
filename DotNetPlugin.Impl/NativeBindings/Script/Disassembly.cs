using System.Runtime.InteropServices;

namespace DotNetPlugin.NativeBindings.Script
{
    public static partial class Gui
    {
        public static class Disassembly
        {
#if AMD64
            private const string dll = "x64dbg.dll";

            private const string Script_Gui_Disassembly_SelectionGetStartEP = "?SelectionGetStart@Disassembly@Gui@Script@@YA_KXZ";
            private const string Script_Gui_Disassembly_SelectionGetEndEP = "?SelectionGetEnd@Disassembly@Gui@Script@@YA_KXZ";
#else
            private const string dll = "x32dbg.dll";

            private const string Script_Gui_Disassembly_SelectionGetStartEP = "?SelectionGetStart@Disassembly@Gui@Script@@YAKXZ";
            private const string Script_Gui_Disassembly_SelectionGetEndEP = "?SelectionGetEnd@Disassembly@Gui@Script@@YAKXZ";
#endif
            private const CallingConvention cdecl = CallingConvention.Cdecl;

            [DllImport(dll, CallingConvention = cdecl, EntryPoint = Script_Gui_Disassembly_SelectionGetStartEP, ExactSpelling = true)]
            private static extern nuint Script_Gui_Disassembly_SelectionGetStart();

            public static nuint SelectionGetStart() => Script_Gui_Disassembly_SelectionGetStart();

            [DllImport(dll, CallingConvention = cdecl, EntryPoint = Script_Gui_Disassembly_SelectionGetEndEP, ExactSpelling = true)]
            private static extern nuint Script_Gui_Disassembly_SelectionGetEnd();

            public static nuint SelectionGetEnd() => Script_Gui_Disassembly_SelectionGetEnd();
        }
    }
}
