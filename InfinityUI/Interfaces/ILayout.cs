using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Interfaces
{
    public interface ILayout
    {
        bool NeedsUpdate { get; set; }
        void Invalidate();
    }
}
