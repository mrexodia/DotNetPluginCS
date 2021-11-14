using System.Threading;
using System.Windows.Forms;
using Managed.x64dbg.Script;
using Managed.x64dbg.SDK;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace DotNetPlugin
{
    public static class RegisteredCommands
    {
        public static bool cbNetTestCommand(int argc, string[] argv)
        {
            PLog.WriteLine("[.net TEST] .Net test command!");
            string empty = string.Empty;
            string Left = Interaction.InputBox("Enter value pls", "NetTest", "", -1, -1);
            if (Left == null | Operators.CompareString(Left, "", false) == 0)
                PLog.WriteLine("[TEST] cancel pressed!");
            else
                PLog.WriteLine("[TEST] line: {0}", Left);
            return true;
        }

        public static bool cbDumpProcessCommand(int argc, string[] argv)
        {
            var addr = argc >= 2 ? Bridge.DbgValFromString(argv[1]) : Bridge.DbgValFromString("cip");
            PLog.WriteLine("[DotNet TEST] addr: {0}", addr.ToPtrString());
            var modinfo = new Module.ModuleInfo();
            if (!Module.InfoFromAddr(addr, ref modinfo))
            {
                PLog.WriteLine("[DotNet TEST] Module.InfoFromAddr failed...");
                return false;
            }
            PLog.WriteLine("[DotNet TEST] InfoFromAddr success, base: {0}", modinfo.@base.ToPtrString());
            var hProcess = Bridge.DbgValFromString("$hProcess");
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Executables (*.dll,*.exe)|*.exe|All Files (*.*)|*.*",
                RestoreDirectory = true,
                FileName = modinfo.name
            };
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
                    PLog.WriteLine("[DotNet TEST] DumpProcess failed...");
                    return false;
                }
                PLog.WriteLine("[DotNet TEST] Dumping done!");
            }
            return true;
        }

        public static bool cbModuleEnum(int argc, string[] argv)
        {
            foreach (var mod in Module.GetList())
            {
                PLog.WriteLine("[DotNet TEST] {0} {1}", mod.@base.ToPtrString(), mod.name);
                foreach (var section in Module.SectionListFromAddr(mod.@base))
                    PLog.WriteLine("[DotNet TEST]    {0} \"{1}\"", section.addr.ToPtrString(), section.name);
            }
            return true;
        }
    }
}
