using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Utils
{
    public enum GlobalEvent
    {
        Preview2D, //used by UI2DWindow
        Preview2DUV, //used by UI2DWindow, UI3DWindow
        Preview3DColor,
        Preview3DNormal,
        Preview3DHeight,
        Preview3DThickness,
        Preview3DMetallic,
        Preview3DRoughness,
        Preview3DOcclusion,
        Preview3DEmission, //used by UINode, Renderer
        MoveSelected, //used by UINode, UIGraph
        MoveComplete, //used by UINode, UIGraph
        ViewParameters, //used by Parameters pane
        UpdateParameters, //used by Settings Classes, Parameters pane
        ClearViewParameters, //used by Parameters pane
        HdriUpdate, //used by GLMainWindow, Renderer
        SkyboxUpdate, //used by GLMainWindow, Renderer
        ScheduleExport, //used by UIGraph, GLMainWindow
        ArrangeNodesVertical, //used by UIGraph
        ArrangeNodesHorizontal, //used by UIGraph
        FitNodesIntoView, //used by UIGraph
        ActualNodeViewSize, //used by UIGraph
        UpdateTrackedNode, //used by UIGraph, Parameters Pane
        HideParameters, //used by UIGraphEvents, Parameters Pane
        ShowParameters, //used by UIGraphEvents, Parameters Pane
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
