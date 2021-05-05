using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Interfaces
{
    public interface IFocusable
    {
        void OnFocus(FocusEvent ev);
        void OnLostFocus(FocusEvent ev);
    }

    public class FocusEvent : UIEventArgs
    {
        public bool IsHandled { get; set; } = false;
    }
}
