using System;
using System.Windows.Forms;

namespace DotNetPlugin.NativeBindings.Win32
{
    public sealed class Win32Window : IWin32Window
    {
        public Win32Window(IntPtr handle)
        {
            Handle = handle;
        }

        public IntPtr Handle { get; }
    }
}
