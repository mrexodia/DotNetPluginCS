using System;
using System.Threading;
using System.Windows.Forms;
using DotNetPlugin.NativeBindings;
using DotNetPlugin.NativeBindings.Script;
using DotNetPlugin.NativeBindings.SDK;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace DotNetPlugin
{
    public static class RegisteredCommands
    {
        public static bool cbNetTestCommand(int argc, string[] argv)
        {
            Console.WriteLine($"[{Plugin.PluginLogName}] .Net test command!");
            string empty = string.Empty;
            string Left = Interaction.InputBox("Enter value pls", "NetTest", "", -1, -1);
            if (Left == null | Operators.CompareString(Left, "", false) == 0)
                Console.WriteLine($"[{Plugin.PluginLogName}] cancel pressed!");
            else
                Console.WriteLine($"[{Plugin.PluginLogName}] line: {Left}");
            return true;
        }

        public static bool cbDumpProcessCommand(int argc, string[] argv)
        {
            var addr = argc >= 2 ? Bridge.DbgValFromString(argv[1]) : Bridge.DbgValFromString("cip");
            Console.WriteLine($"[{Plugin.PluginLogName}] addr: {addr.ToPtrString()}");
            var modinfo = new Module.ModuleInfo();
            if (!Module.InfoFromAddr(addr, ref modinfo))
            {
                Console.WriteLine($"[{Plugin.PluginLogName}] Module.InfoFromAddr failed...");
                return false;
            }
            Console.WriteLine($"[{Plugin.PluginLogName}] InfoFromAddr success, base: {modinfo.@base.ToPtrString()}");
            var hProcess = Bridge.DbgValFromString("$hProcess");
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Executables (*.dll,*.exe)|*.exe|All Files (*.*)|*.*",
                RestoreDirectory = true,
                FileName = modinfo.name
            };
            using (saveFileDialog)
            {
                var result = DialogResult.Cancel;
                var t = new Thread(() => result = saveFileDialog.ShowDialog());
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();
                if (result == DialogResult.OK)
                {
                    string fileName = saveFileDialog.FileName;
                    if (!TitanEngine.DumpProcess((nint)hProcess, (nint)modinfo.@base, fileName, addr))
                    {
                        Console.WriteLine($"[{Plugin.PluginLogName}] DumpProcess failed...");
                        return false;
                    }
                    Console.WriteLine($"[{Plugin.PluginLogName}] Dumping done!");
                }
            }
            return true;
        }

        public static bool cbModuleEnum(int argc, string[] argv)
        {
            foreach (var mod in Module.GetList())
            {
                Console.WriteLine($"[{Plugin.PluginLogName}] {mod.@base.ToPtrString()} {mod.name}");
                foreach (var section in Module.SectionListFromAddr(mod.@base))
                    Console.WriteLine($"[{Plugin.PluginLogName}]    {section.addr.ToPtrString()} \"{section.name}\"");
            }
            return true;
        }
    }
}
