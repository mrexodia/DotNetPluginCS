using System;
using DotNetPlugin.NativeBindings.SDK;
using Microsoft.VisualBasic;

namespace DotNetPlugin
{
    partial class Plugin
    {
        private const int MENU_ABOUT = 0;
        private const int MENU_DUMP = 1;
        
        private int _hSubMenu;

        private void RegisterMenu(in Plugins.PLUG_SETUPSTRUCT setupStruct)
        {
            Plugins._plugin_menuaddentry(setupStruct.hMenu, MENU_ABOUT, "&About...");
            Plugins._plugin_menuaddentry(setupStruct.hMenu, MENU_DUMP, "&DotNetDumpProcess");
            _hSubMenu = Plugins._plugin_menuadd(setupStruct.hMenu, "sub menu");
            Plugins._plugin_menuaddentry(_hSubMenu, 3, "sub menu entry");
        }

        private void UnregisterMenu()
        {
            Plugins._plugin_menuentryremove(PluginHandle, MENU_ABOUT);
            Plugins._plugin_menuentryremove(PluginHandle, MENU_DUMP);
            Plugins._plugin_menuremove(_hSubMenu);
        }

        public override void OnMenuEntry(in Plugins.PLUG_CB_MENUENTRY info)
        {
            switch (info.hEntry)
            {
                case MENU_ABOUT:
                    Interaction.MsgBox("DotNet Plugin For x64dbg\nCoded By <your_name_here>", MsgBoxStyle.OkOnly, "Info");
                    break;
                case MENU_DUMP:
                    if (!Bridge.DbgIsDebugging())
                    {
                        Console.WriteLine("You need to be debugging to use this Command");
                        break;
                    }
                    Bridge.DbgCmdExec("DotNetDumpProcess");
                    break;
            }
        }
    }
}
