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
    partial class Plugin
    {
        [Command("DotNetpluginTestCommand")]
        public static void cbNetTestCommand(string[] args)
        {
            Console.WriteLine(".Net test command!");
            string empty = string.Empty;
            string Left = Interaction.InputBox("Enter value pls", "NetTest", "", -1, -1);
            if (Left == null | Operators.CompareString(Left, "", false) == 0)
                Console.WriteLine("cancel pressed!");
            else
                Console.WriteLine($"line: {Left}");
        }

        [Command("DotNetDumpProcess", DebugOnly = true)]
        public static bool cbDumpProcessCommand(string[] args)
        {
            var addr = args.Length >= 2 ? Bridge.DbgValFromString(args[1]) : Bridge.DbgValFromString("cip");
            Console.WriteLine($"addr: {addr.ToPtrString()}");
            var modinfo = new Module.ModuleInfo();
            if (!Module.InfoFromAddr(addr, ref modinfo))
            {
                Console.Error.WriteLine($"Module.InfoFromAddr failed...");
                return false;
            }
            Console.WriteLine($"InfoFromAddr success, base: {modinfo.@base.ToPtrString()}");
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
                        Console.Error.WriteLine($"DumpProcess failed...");
                        return false;
                    }
                    Console.WriteLine($"Dumping done!");
                }
            }
            return true;
        }

        [Command("DotNetModuleEnum", DebugOnly = true)]
        public static void cbModuleEnum(string[] args)
        {
            foreach (var mod in Module.GetList())
            {
                Console.WriteLine($"{mod.@base.ToPtrString()} {mod.name}");
                foreach (var section in Module.SectionListFromAddr(mod.@base))
                    Console.WriteLine($"    {section.addr.ToPtrString()} \"{section.name}\"");
            }
        }
    }
}
