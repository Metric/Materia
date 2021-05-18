using InfinityUI.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Interfaces
{
    public interface IFocusable
    {
        bool IsFocusable { get; set; }
        UIObject Parent { get; }
        void OnFocus(FocusEvent ev);
        void OnLostFocus(FocusEvent ev);
    }

    public class FocusEvent : UIEventArgs
    {
        public bool Navigated { get; set; } = false;
        public bool IsHandled { get; set; } = false;

        public FocusEvent(bool navigated = false)
        {
            Navigated = navigated;
        }
    }
}
