using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Materia.UI.Helpers
{
    public enum InputManagerCommand
    {
        None,
        Undo,
        Redo,
        Save,
        Copy,
        Paste,
        Clear,
        Delete,
        PopupShelf,
        New,
        Comment,
        Pin,
        NextPin
    }

    public class MateriaInputManager
    {
        protected static Dictionary<string, InputManagerCommand> keyMap = new Dictionary<string, InputManagerCommand>();
        protected static Dictionary<InputManagerCommand, List<Action<KeyEventArgs>>> actionMap = new Dictionary<InputManagerCommand, List<Action<KeyEventArgs>>>();

        public static bool IsShiftDown
        {
            get
            {
                return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            }
        }

        public static bool IsAltDown
        {
            get
            {
                return Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
            }
        }

        public static bool IsCtrlDown
        {
            get
            {
                return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            }
        }

        static MateriaInputManager()
        {
            //set defaults
            Defaults();
        }

        public static void Init()
        {
            EventManager.RegisterClassHandler(typeof(Window), Window.KeyDownEvent, new KeyEventHandler(HandleKeys), true);
        }

        protected static void Defaults()
        {
            Set(InputManagerCommand.Copy, Key.C, false, true, false);
            Set(InputManagerCommand.Paste, Key.V, false, true, false);
            Set(InputManagerCommand.Undo, Key.Z, false, true, false);
            Set(InputManagerCommand.Redo, Key.Y, false, true, false);
            Set(InputManagerCommand.Save, Key.S, false, true, false);
            Set(InputManagerCommand.Clear, Key.Escape, false, false, false);
            Set(InputManagerCommand.Delete, Key.Delete, false, false, false);
            Set(InputManagerCommand.PopupShelf, Key.Space, false, false, false);
            Set(InputManagerCommand.New, Key.N, false, true, false);
            Set(InputManagerCommand.Comment, Key.C, false, false, false);
            Set(InputManagerCommand.Pin, Key.P, false, false, false);
            Set(InputManagerCommand.NextPin, Key.Tab, false, false, false);
        }

        protected static void HandleKeys(object sender, KeyEventArgs e)
        {
            string k = e.Key.ToString() + "(:)" + IsShiftDown + "(:)" + IsCtrlDown + "(:)" + IsAltDown;

            InputManagerCommand cmd = InputManagerCommand.None;

            keyMap.TryGetValue(k, out cmd);

            if(cmd != InputManagerCommand.None)
            {
                List<Action<KeyEventArgs>> actions = null;
                if(actionMap.TryGetValue(cmd, out actions))
                {
                    foreach(Action<KeyEventArgs> a in actions)
                    {
                        a.Invoke(e);
                    }
                }
            }
        }

        public static void Set(InputManagerCommand command, Key k, bool shift = false, bool ctrl = false, bool alt = false)
        {
            string key = k.ToString() + "(:)" + shift + "(:)" + ctrl + "(:)" + alt;
            keyMap[key] = command;
        }

        public static void Add(InputManagerCommand command, Action<KeyEventArgs> a)
        {
            List<Action<KeyEventArgs>> actions = null;
            actionMap.TryGetValue(command, out actions);

            if(actions == null)
            {
                actions = new List<Action<KeyEventArgs>>();
            }

            actions.Add(a);
            actionMap[command] = actions;
        }
    }
}
