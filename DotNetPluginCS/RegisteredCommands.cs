using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using DotNetPlugin.Script;
using DotNetPlugin.SDK;
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
            var num1 = argc >= 2 ? Bridge.DbgValFromString(argv[1]) : TitanEngine.GetContextData(35U);
            IntPtr num2 = Marshal.AllocHGlobal(Bridge.MAX_MODULE_SIZE);
            string empty = string.Empty;
            if (!Bridge.DbgGetModuleAt(num1, num2))
            {
                PLog.WriteLine("[DotNet TEST] no module at {0}...\n", num1);
                return false;
            }
            string stringAnsi = Marshal.PtrToStringAnsi(num2);
            var ImageBase = Bridge.DbgModBaseFromName(stringAnsi);
            if (ImageBase == IntPtr.Zero)
            {
                PLog.WriteLine("[DotNet TEST] could not get module base...");
                return false;
            }
            var processInformation = (WAPI.PROCESS_INFORMATION)Marshal.PtrToStructure(TitanEngine.TitanGetProcessInformation(), typeof(WAPI.PROCESS_INFORMATION));
            var hProcess = processInformation.hProcess;// (structure != null ? (Plugins.PROCESS_INFORMATION) structure : processInformation).hProcess;
            StringBuilder lpBaseName = new StringBuilder();
            lpBaseName.Append(stringAnsi);
            if ((int)WAPI.GetModuleBaseNameA(hProcess, ImageBase, lpBaseName, 256U) == 0)
            {
                PLog.WriteLine("[DotNet TEST] could not get module base name...");
                return false;
            }
            string str1 = lpBaseName.ToString();
            string str2 = str1.Substring(0, str1.IndexOf(".")) + "_dump" + str1.Substring(str1.IndexOf("."), checked(str1.Length - str1.IndexOf(".")));
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Executables (*.dll,*.exe)|*.exe|All Files (*.*)|*.*";
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FileName = str2;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;
                if (!TitanEngine.DumpProcess(hProcess, ImageBase, fileName, num1))
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
