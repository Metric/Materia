using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Utils
{
    public enum GlobalEvent
    {
        Preview2D,
        Preview2DUV,
        Preview3D,
        Preview3DColor,
        Preview3DNormal,
        Preview3DHeight,
        Preview3DThickness,
        Preview3DMetallic,
        Preview3DRoughness,
        Preview3DOcclusion,
        Preview3DEmission,
        MoveSelected,
        MoveComplete,
        ViewParameters,
        ClearViewParameters
    }

    public static class GlobalEvents
    {
        private static readonly Dictionary<GlobalEvent, List<Action<object,object>>> events = new Dictionary<GlobalEvent, List<Action<object,object>>>();

        public static void Off(GlobalEvent name, Action<object,object> handler = null)
        {
            events.TryGetValue(name, out List<Action<object, object>> handlers);
            
            if (handler != null)
            {
                handlers?.Remove(handler);
            }
            else
            {
                handlers?.Clear();
            }
        }

        public static void On(GlobalEvent name, Action<object,object> handler)
        {
            events.TryGetValue(name, out List<Action<object,object>> handlers);
            handlers ??= new List<Action<object,object>>();
            if (!handlers.Contains(handler))
            {
                handlers.Add(handler);
            }
            events[name] = handlers;
        }

        public static void Emit(GlobalEvent name, object sender, object arg)
        {
            if (events.TryGetValue(name, out List<Action<object,object>> handlers))
            {
                for (int i = 0; i < handlers.Count; ++i)
                {
                    handlers[i]?.Invoke(sender,arg);
                }
            }
        }
    }
}
