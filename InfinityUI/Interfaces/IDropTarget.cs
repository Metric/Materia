using InfinityUI.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Interfaces
{
    public interface IDropTarget
    {
        void OnDrop(UIDropEvent e);
        void OnFileDrop(UIFileDropEvent e);
    }

    public class UIFileDropEvent : UIEventArgs
    {
        public bool IsHandled { get; set; }
        public string[] files;
    }

    public class UIDropEvent : UIEventArgs
    { 
        public bool IsHandled { get; set; }
        public UIDragDrop dragDrop;
    }
}
