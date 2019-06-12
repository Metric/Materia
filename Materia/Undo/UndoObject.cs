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
        public abstract UndoObject Undo();
    }

    public class DeleteNode : UndoObject
    {
        public string json;
        public Point point;
        public List<Tuple<string, List<Nodes.NodeOutputConnection>>> parents;
        public UIGraph graph;

        protected string[] stack;

        public DeleteNode(string stackId, string js, Point p, List<Tuple<string, List<Nodes.NodeOutputConnection>>> pars, UIGraph g, string[] gstack = null)
        {
            json = js;
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

        public override UndoObject Undo()
        {
            if(graph != null && !string.IsNullOrEmpty(json))
            {
                graph.TryAndLoadGraphStack(stack);

                UI.IUIGraphNode n = graph.AddNodeFromJson(json, point);

                if(n != null && parents.Count > 0)
                {
                    Task.Delay(250).ContinueWith(t =>
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (var p in parents)
                            {
                                var id = p.Item1;
                                var cons = p.Item2;

                                UI.IUIGraphNode unode = graph.GetNode(id);

                                if (unode != null)
                                {
                                    unode.Node.SetConnection(graph.Graph.NodeLookup, cons, n.Id);
                                    unode.LoadConnection(n.Id);
                                }
                            }
                        });
                    });

                    return new CreateNode(StackId, n.Id, graph, stack);
                }
            }

            return null;
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

        public override UndoObject Undo()
        {
            if (graph != null)
            {
                graph.TryAndLoadGraphStack(stack);

                Tuple<string, Point, List<Tuple<string, List<Nodes.NodeOutputConnection>>>> result = graph.RemoveNode(id);

                if (result != null)
                {
                    return new DeleteNode(StackId, result.Item1, result.Item2, result.Item3, graph, stack);
                }
            }

            return null;
        }
    }
}
