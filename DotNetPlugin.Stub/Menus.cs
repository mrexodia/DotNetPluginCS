using System;
using System.Collections.Generic;
using System.Drawing;
using DotNetPlugin.NativeBindings.SDK;

namespace DotNetPlugin
{
    /// <remarks>
    /// Not thread-safe. If you want to modify the menu structure dynamically, you are responsible for synchronization.
    /// See also <seealso cref="PluginBase.MenusSyncObj"/>.
    /// </remarks>
    public sealed class Menus : IDisposable
    {
        private int _lastId;
        internal Dictionary<int, MenuItem> _menuItemsById;

        internal Menus(int pluginHandle, in Plugins.PLUG_SETUPSTRUCT setupStruct)
        {
            PluginHandle = pluginHandle;

            All = new[]
            {
                Main = new Menu(this, setupStruct.hMenu),
                Disasm = new Menu(this, setupStruct.hMenuDisasm),
                Dump = new Menu(this, setupStruct.hMenuDump),
                Stack = new Menu(this, setupStruct.hMenuStack),
                Graph = new Menu(this, setupStruct.hMenuGraph),
                Memmap = new Menu(this, setupStruct.hMenuMemmap),
                Symmod = new Menu(this, setupStruct.hMenuSymmod),
            };

            _menuItemsById = new Dictionary<int, MenuItem>();
        }

        public void Dispose()
        {
            if (_menuItemsById != null)
            {
                Clear();
                _menuItemsById = null;
            }
        }

        internal void EnsureNotDisposed()
        {
            if (_menuItemsById == null)
                throw new ObjectDisposedException(nameof(Menus));
        }

        internal int PluginHandle { get; }

        public Menu Main; // main menu
        public Menu Disasm; // disasm menu
        public Menu Dump; // dump menu
        public Menu Stack; // stack menu
        public Menu Graph; // graph menu
        public Menu Memmap; // memory map menu
        public Menu Symmod; // symbol module menu

        public IReadOnlyList<Menu> All { get; }

        internal int NextItemId() => ++_lastId;

        internal MenuItem GetMenuItemById(int id) => _menuItemsById.TryGetValue(id, out var menuItem) ? menuItem : null;

        public void Clear()
        {
            foreach (var menu in All)
                menu.Clear();
        }
    }

    public sealed class MenuException : ApplicationException
    {
        public MenuException(string message) : base(message) { }
    }

    public abstract class MenuItemBase
    {
        internal MenuItemBase(Menu parent)
        {
            Parent = parent;
        }

        public Menu Parent { get; }

        public abstract bool Remove();
    }

    public sealed class Menu : MenuItemBase
    {
        internal readonly Menus _menus;

        private Menu(Menus menus, Menu parent, int handle) : base(parent)
        {
            _menus = menus;
            Handle = handle;

            _items = new List<MenuItemBase>();
        }

        internal Menu(Menus menus, int handle) : this(menus, null, handle) { }

        private Menu(Menu parent, int handle) : this(parent._menus, parent, handle) { }

        public int Handle { get; }
        public bool IsRoot => Parent == null;

        internal readonly List<MenuItemBase> _items;
        public IReadOnlyList<MenuItemBase> Items => _items;

        public Menu AddAndConfigureSubMenu(string title)
        {
            if (title == null)
                throw new ArgumentNullException(nameof(title));

            _menus.EnsureNotDisposed();

            var subMenuHandle = Plugins._plugin_menuadd(Handle, title);

            if (subMenuHandle < 0)
                throw new MenuException($"Failed to add sub-menu '{title}'.");

            var subMenu = new Menu(this, subMenuHandle);
            _items.Add(subMenu);
            return subMenu;
        }

        public Menu AddSubMenu(string title)
        {
            AddAndConfigureSubMenu(title);
            return this;
        }

        public MenuItem AddAndConfigureItem(string title, Action<MenuItem> handler)
        {
            if (title == null)
                throw new ArgumentNullException(nameof(title));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _menus.EnsureNotDisposed();

            var itemId = _menus.NextItemId();

            if (!Plugins._plugin_menuaddentry(Handle, itemId, title))
                throw new MenuException($"Failed to add menu item '{title}'.");

            var item = new MenuItem(this, itemId, handler);
            _items.Add(item);
            _menus._menuItemsById.Add(itemId, item);
            return item;
        }

        public Menu AddItem(string title, Action<MenuItem> handler)
        {
            AddAndConfigureItem(title, handler);
            return this;
        }

        public Menu AddSeparator()
        {
            _menus.EnsureNotDisposed();

            if (!Plugins._plugin_menuaddseparator(Handle))
                throw new MenuException($"Failed to add separator.");
            
            return this;
        }

        private unsafe Menu SetIcon(byte[] iconArray)
        {
            fixed (byte* iconBytes = iconArray)
            {
                var iconStruct = new BridgeBase.ICONDATA
                {
                    data = (IntPtr)iconBytes,
                    size = (nuint)iconArray.Length
                };

                Plugins._plugin_menuseticon(Handle, in iconStruct);
            }

            return this;
        }

        public Menu SetIcon(Icon icon) => SetIcon(BridgeBase.ICONDATA.GetIconData(icon));
        public Menu SetIcon(Image image) => SetIcon(BridgeBase.ICONDATA.GetIconData(image));

        public Menu SetVisible(bool value)
        {
            Plugins._plugin_menusetvisible(Parent._menus.PluginHandle, Handle, value);

            return this;
        }

        public Menu SetName(string value)
        {
            Plugins._plugin_menusetname(Parent._menus.PluginHandle, Handle, value);

            return this;
        }

        public override bool Remove()
        {
            _menus.EnsureNotDisposed();

            if (IsRoot || !Plugins._plugin_menuremove(Handle))
                return false;

            Parent._items.Remove(this);
            return true;
        }

        public void Clear()
        {
            for (int i = _items.Count - 1; i >= 0; i--)
                _items[i].Remove();
        }
    }

    public sealed class MenuItem : MenuItemBase
    {
        internal MenuItem(Menu parent, int id, Action<MenuItem> handler) : base(parent)
        {
            Id = id;
            Handler = handler;
        }

        public int Id { get; }

        internal Action<MenuItem> Handler { get; }

        private unsafe MenuItem SetIcon(byte[] iconArray)
        {
            fixed (byte* iconBytes = iconArray)
            {
                var iconStruct = new BridgeBase.ICONDATA
                {
                    data = (IntPtr)iconBytes,
                    size = (nuint)iconArray.Length
                };

                Plugins._plugin_menuentryseticon(Parent._menus.PluginHandle, Id, in iconStruct);
            }

            return this;
        }

        public MenuItem SetIcon(Icon icon) => SetIcon(BridgeBase.ICONDATA.GetIconData(icon));
        public MenuItem SetIcon(Image image) => SetIcon(BridgeBase.ICONDATA.GetIconData(image));

        public MenuItem SetChecked(bool value)
        {
            Plugins._plugin_menuentrysetchecked(Parent._menus.PluginHandle, Id, value);

            return this;
        }

        public MenuItem SetVisible(bool value)
        {
            Plugins._plugin_menuentrysetvisible(Parent._menus.PluginHandle, Id, value);

            return this;
        }

        public MenuItem SetName(string value)
        {
            Plugins._plugin_menuentrysetname(Parent._menus.PluginHandle, Id, value);

            return this;
        }

        public MenuItem SetHotKey(string value)
        {
            Plugins._plugin_menuentrysethotkey(Parent._menus.PluginHandle, Id, value);

            return this;
        }

        public override bool Remove()
        {
            Parent._menus.EnsureNotDisposed();

            if (!Plugins._plugin_menuentryremove(Parent._menus.PluginHandle, Id))
                return false;

            Parent._menus._menuItemsById.Remove(Id);
            Parent._items.Remove(this);
            return true;
        }
    }
}
