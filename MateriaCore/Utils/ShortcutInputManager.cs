using InfinityUI.Core;
using InfinityUI.Interfaces;
using System;
using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Text;

namespace MateriaCore.Utils
{
    public enum ShortcutCommand
    {
        None,
        Undo,
        Redo,
        Save,
        SaveAs,
        Copy,
        Paste,
        Clear,
        Delete,
        Shelf,
        New,
        Comment,
        Pin,
        NextPin
    }

    public enum ShortcutModifiers
    {
        None = 0,
        Alt = 2,
        Shift = 4,
        Ctrl = 8
    }

    public class ShortcutInputManager
    {
        protected static Dictionary<string, ShortcutCommand> commandMap = new Dictionary<string, ShortcutCommand>();
        protected static Dictionary<ShortcutCommand, string> reverseMap = new Dictionary<ShortcutCommand, string>();
        protected static Dictionary<ShortcutCommand, List<Action<KeyboardEventArgs>>> commandToActionMap = new Dictionary<ShortcutCommand, List<Action<KeyboardEventArgs>>>();

        public static void Initialize()
        {
            UI.KeyDown += UI_KeyDown;
            Defaults();
            Load();
        }

        protected static void Load()
        {
            //todo: load settings here for shortcuts
        }

        public static void Save()
        {
            //todo: save setting here for shortcuts
        }

        protected static void Defaults()
        {
            Set(ShortcutCommand.Copy, Keys.C, ShortcutModifiers.Ctrl);
            Set(ShortcutCommand.Paste, Keys.V, ShortcutModifiers.Ctrl);
            Set(ShortcutCommand.Undo, Keys.Z, ShortcutModifiers.Ctrl);
            Set(ShortcutCommand.Redo, Keys.Y, ShortcutModifiers.Ctrl);
            Set(ShortcutCommand.Save, Keys.S, ShortcutModifiers.Ctrl);
            Set(ShortcutCommand.SaveAs, Keys.S, ShortcutModifiers.Shift | ShortcutModifiers.Ctrl);
            Set(ShortcutCommand.Clear, Keys.Escape);
            Set(ShortcutCommand.Delete, Keys.Delete);
            Set(ShortcutCommand.Shelf, Keys.Space);
            Set(ShortcutCommand.New, Keys.N, ShortcutModifiers.Ctrl);
            Set(ShortcutCommand.Comment, Keys.C);
            Set(ShortcutCommand.Pin, Keys.P);
            Set(ShortcutCommand.NextPin, Keys.Tab);
        }

        private static void UI_KeyDown(KeyboardEventArgs e)
        {
            if (e == null) return;

            string k = e.Key.ToString();

            string mods = "";
            string sep = "";
            if (e.IsCtrl)
            {
                mods += "Ctrl";
                sep = "-";
            }
            if (e.IsAlt)
            {
                mods += sep + "Alt";
                sep = "-";
            }
            if (e.IsShift)
            {
                mods += sep + "Shift";
                sep = "-";
            }

            k += sep + mods;
            if (commandMap.TryGetValue(k, out ShortcutCommand command)) 
            {
                if (commandToActionMap.TryGetValue(command, out List<Action<KeyboardEventArgs>> actions))
                {
                    if (actions == null) return;
                    for (int i = 0; i < actions.Count; ++i)
                    {
                        actions[i]?.Invoke(e);
                        if (e.IsHandled) return;
                    }
                }
            }
        }

        public static void Unset(ShortcutCommand c)
        {
            if (reverseMap.TryGetValue(c, out string k))
            {
                commandMap.Remove(k);
                reverseMap.Remove(c);
            }
        }

        public static void Set(ShortcutCommand c, Keys key, ShortcutModifiers modifiers = ShortcutModifiers.None)
        {
            string k = key.ToString();

            string mods = "";
            string sep = "";
            if (modifiers.HasFlag(ShortcutModifiers.Ctrl))
            {
                mods += "Ctrl";
                sep = "-";
            }
            if (modifiers.HasFlag(ShortcutModifiers.Alt))
            {
                mods += sep + "Alt";
                sep = "-";
            }
            if (modifiers.HasFlag(ShortcutModifiers.Shift))
            {
                mods += sep + "Shift";
                sep = "-";
            }

            k += sep + mods;

            commandMap[k] = c;
            reverseMap[c] = k;
        }

        public static void AddAction(ShortcutCommand c, Action<KeyboardEventArgs> a)
        {
            if (a == null) return;
            commandToActionMap.TryGetValue(c, out List<Action<KeyboardEventArgs>> actions);
            actions ??= new List<Action<KeyboardEventArgs>>();
            actions.Add(a);
            commandToActionMap[c] = actions;
        }
    }
}
