using System;
using System.Collections.Concurrent;
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
        NewDocument, //used by MainMenu, UIDocuments
        OpenDocument, //used by MainMenu, UIDocuments
        SaveAllDocuments, //used by MainMenu, UIDocuments
        CloseAllDocuments, //used by MainMenu, UIDocuments
        CloseAllWindows, //used by MainMenu, GLMainWindow
    }

    public static class GlobalEvents
    {
        private struct GlobalEventMesssage
        {
            public GlobalEvent evt;
            public object sender;
            public object arg;
        }

        private const int MAX_TO_POLL = 100;

        private static readonly Dictionary<GlobalEvent, List<Action<object,object>>> events = new Dictionary<GlobalEvent, List<Action<object,object>>>();
        private static readonly Queue<GlobalEventMesssage> messages = new Queue<GlobalEventMesssage>();

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
            messages.Enqueue(new GlobalEventMesssage
            {
                evt = name,
                sender = sender,
                arg = arg
            });
        }

        //doing this in order to clear stack
        //each frame before we call the next event
        public static void Poll()
        {
            int pollCount = 0;
            while (messages.Count > 0 && pollCount++ < MAX_TO_POLL)
            {
                var msg = messages.Dequeue();
                GlobalEvent name = msg.evt;
                object sender = msg.sender;
                object arg = msg.arg;

                if (events.TryGetValue(name, out List<Action<object, object>> handlers))
                {
                    for (int i = 0; i < handlers.Count; ++i)
                    {
                        handlers[i]?.Invoke(sender, arg);
                    }
                }
            }
        }

        public static void Clear()
        {
            messages.Clear();
        }
    }
}
