using System.Runtime.InteropServices;

namespace DotNetPlugin.NativeBindings.Script
{
    public static partial class Gui
    {
        public enum Window
        {
            DisassemblyWindow,
            DumpWindow,
            StackWindow,
            GraphWindow,
            MemMapWindow,
            SymModWindow
        };

#if AMD64
        private const string dll = "x64dbg.dll";

        private const string Script_Gui_SelectionGetStartEP = "?SelectionGetStart@Gui@Script@@YA_KW4Window@12@@Z";
        private const string Script_Gui_SelectionGetEndEP = "?SelectionGetEnd@Gui@Script@@YA_KW4Window@12@@Z";
#else
        private const string dll = "x32dbg.dll";

        private const string Script_Gui_SelectionGetStartEP = "?SelectionGetStart@Gui@Script@@YAKW4Window@12@@Z";
        private const string Script_Gui_SelectionGetEndEP = "?SelectionGetEnd@Gui@Script@@YAKW4Window@12@@Z";
#endif
        private const CallingConvention cdecl = CallingConvention.Cdecl;

        [DllImport(dll, CallingConvention = cdecl, EntryPoint = Script_Gui_SelectionGetStartEP, ExactSpelling = true)]
        private static extern nuint Script_Gui_SelectionGetStart(Window window);

        public static nuint SelectionGetStart(Window window) => Script_Gui_SelectionGetStart(window);

        [DllImport(dll, CallingConvention = cdecl, EntryPoint = Script_Gui_SelectionGetEndEP, ExactSpelling = true)]
        private static extern nuint Script_Gui_SelectionGetEnd(Window window);

        public static nuint SelectionGetEnd(Window window) => Script_Gui_SelectionGetEnd(window);
    }
}
