using InfinityUI.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Interfaces
{
    public interface IComponent : IDisposable
    {
        UIObject Parent { get; set; }
        void Awake();

        void Update();
    }
}
