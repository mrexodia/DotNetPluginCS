using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DotNetPlugin.Bindings.SDK
{
    // https://github.com/x64dbg/x64dbg/blob/development/src/bridge/bridgemain.h
    public static partial class Bridge
    {
        public const int GUI_MAX_LINE_SIZE = 65536;

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        private static extern bool GuiGetLineWindow([MarshalAs(UnmanagedType.LPUTF8Str)] string title, IntPtr text);

        public static unsafe bool GuiGetLineWindow([MarshalAs(UnmanagedType.LPUTF8Str)] string title, out string text)
        {
            // alternatively we could implement a custom marshaler (ICustomMarshaler) but that wont't work for ref/out parameters for some reason...
            var textBuffer = Marshal.AllocHGlobal(GUI_MAX_LINE_SIZE);
            try
            {
                var success = GuiGetLineWindow(title, textBuffer);
                text = success ? textBuffer.MarshalToStringUTF8(GUI_MAX_LINE_SIZE) : default;
                return success;
            }
            finally { Marshal.FreeHGlobal(textBuffer); }
        }
    }
}
