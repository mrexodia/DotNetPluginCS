using System;
using System.Runtime.InteropServices;

namespace DotNetPlugin.NativeBindings.Win32
{
    public static class Kernel32
    {
        [DllImport("kernel32.dll", EntryPoint = "RtlZeroMemory", ExactSpelling = true)]
        public static extern void ZeroMemory(IntPtr dst, nuint length);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ContinueDebugEvent(int dwProcessId, int dwThreadId, ContinueStatus dwContinueStatus);
    }
}
