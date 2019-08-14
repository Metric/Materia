using Materia.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Materia.Undo
{
    public abstract class UndoObject
    {
        public string StackId { get; set; }
        public abstract Task Undo(Action<UndoObject> cb);
    }

    public class DeleteNode : UndoObject
    {
        public string json;
        public Point point;
        public List<NodeConnection> parents;
        public UIGraph graph;
        public string nid;

        protected string[] stack;

        public DeleteNode(string stackId, string js, string id, Point p, List<NodeConnection> pars, UIGraph g, string[] gstack = null)
        {
            json = js;
            nid = id;
            graph = g;
            point = p;
            parents = pars;
            StackId = stackId;

            if (gstack == null)
            {
                stack = graph.GetGraphStack();
            }
            else
            {
                stack = gstack;
            }
        }

        public async override Task Undo(Action<UndoObject> cb)
        {
            if(graph != null && !string.IsNullOrEmpty(json))
            {
                graph.TryAndLoadGraphStack(stack);

                await Task.Delay(25);

                App.Current.Dispatcher.Invoke(() =>
                {
                    UI.IUIGraphNode n = graph.AddNodeFromJson(json, point);

                    if (n != null && parents.Count > 0)
                    {
                        Task.Delay(100).ContinueWith(t =>
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                foreach (var p in parents)
                                {
                                    UI.IUIGraphNode unode = graph.GetNode(p.parent);

                                    if (unode != null)
                                    {
                                        unode.Node.SetConnection(n.Node, p);
                                        unode.LoadConnection(n.Id);
                                    }
                                }

                            //update the graph after reconnections
                            graph.Graph.TryAndProcess();
                            });
                        });
                    }

                    if (cb != null)
                    {
                        cb.Invoke(new CreateNode(StackId, nid, graph, stack));
                    }
                });

                return;
            }

            if (cb != null)
            {
                cb.Invoke(null);
            }
        }
    }

    public class CreateNode : UndoObject
    {
        public string id;
        public UIGraph graph;
        protected string[] stack;

        public CreateNode(string stackId, string i, UIGraph g, string[] gstack = null)
        {
            id = i;
            graph = g;
            StackId = stackId;
            if (gstack == null)
            {
                stack = graph.GetGraphStack();
            }
            else
            {
                stack = gstack;
            }
        }

        public async override Task Undo(Action<UndoObject> cb)
        {
            if (graph != null)
            {
                graph.TryAndLoadGraphStack(stack);

                await Task.Delay(25);

                App.Current.Dispatcher.Invoke(() =>
                {
                    Tuple<string, Point, List<NodeConnection>> result = graph.RemoveNode(id);

                    if (result != null)
                    {
                        if (cb != null)
                        {
                            cb.Invoke(new DeleteNode(StackId, result.Item1, id, result.Item2, result.Item3, graph, stack));
                        }
                        return;
                    }

                    if(cb != null)
                    {
                        cb.Invoke(null);
                    }
                });

                return;
            }

            if(cb != null)
            {
                cb.Invoke(null);
            }
        }
    }
}
