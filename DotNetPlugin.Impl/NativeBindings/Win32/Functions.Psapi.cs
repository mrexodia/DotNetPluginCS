using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNetPlugin.NativeBindings.Win32
{
    public static class Psapi
    {
        [DllImport("psapi.dll", CharSet = CharSet.Auto)]
        public static extern uint GetModuleBaseName(IntPtr hProcess, IntPtr hModule, StringBuilder lpBaseName, uint nSize);
    }
}
