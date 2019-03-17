using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Materia.Undo
{
    public abstract class RedoObject
    {
        public string StackId { get; set; }
        public abstract UndoObject Redo();
    }

    public abstract class UndoObject
    {
        public string StackId { get; set; }
        public abstract RedoObject Undo();
    }

    public class RedoDeleteNode : RedoObject
    {
        public string id;
        public UIGraph graph;

        public RedoDeleteNode(string stackId, string i, UIGraph g)
        {
            id = i;
            graph = g;
            StackId = stackId;
        }

        public override UndoObject Redo()
        {
            if(graph != null)
            {
                Tuple<string, Point, List<Tuple<string, List<Nodes.NodeOutputConnection>>>> result = graph.RemoveNode(id);

                if(result != null)
                {
                    return new UndoDeleteNode(StackId, result.Item1, result.Item2, result.Item3, graph);
                }
            }

            return null;
        }
    }

    public class RedoCreateNode : RedoObject
    {
        public string json;
        public Point point;
        public List<Tuple<string, List<Nodes.NodeOutputConnection>>> parents;
        public UIGraph graph;

        public RedoCreateNode(string stackId, string js, Point p, List<Tuple<string, List<Nodes.NodeOutputConnection>>> pars, UIGraph g)
        {
            json = js;
            graph = g;
            point = p;
            parents = pars;
            StackId = stackId;
        }

        public override UndoObject Redo()
        {
            if (graph != null && !string.IsNullOrEmpty(json))
            {
                UINode n = graph.AddNodeFromJson(json, point);

                if (n != null)
                {
                    foreach (var p in parents)
                    {
                        var id = p.Item1;
                        var cons = p.Item2;

                        UINode unode = graph.GetNode(id);

                        if (unode != null)
                        {
                            unode.Node.SetConnection(graph.Graph.NodeLookup, cons, n.Id);
                            unode.LoadConnection(n.Id);
                        }
                    }

                    return new UndoCreateNode(StackId, n.Id, graph);
                }
            }

            return null;
        }
    }

    public class UndoDeleteNode : UndoObject
    {
        public string json;
        public Point point;
        public List<Tuple<string, List<Nodes.NodeOutputConnection>>> parents;
        public UIGraph graph;

        public UndoDeleteNode(string stackId, string js, Point p, List<Tuple<string, List<Nodes.NodeOutputConnection>>> pars, UIGraph g)
        {
            json = js;
            graph = g;
            point = p;
            parents = pars;
            StackId = stackId;
        }

        public override RedoObject Undo()
        {
            if(graph != null && !string.IsNullOrEmpty(json))
            {
                UINode n = graph.AddNodeFromJson(json, point);

                if(n != null)
                {
                    foreach (var p in parents)
                    {
                        var id = p.Item1;
                        var cons = p.Item2;

                        UINode unode = graph.GetNode(id);

                        if(unode != null)
                        {
                            unode.Node.SetConnection(graph.Graph.NodeLookup, cons, n.Id);
                            unode.LoadConnection(n.Id);
                        }
                    }

                    return new RedoDeleteNode(StackId, n.Id, graph);
                }
            }

            return null;
        }
    }

    public class UndoCreateNode : UndoObject
    {
        public string id;
        public UIGraph graph;

        public UndoCreateNode(string stackId, string i, UIGraph g)
        {
            id = i;
            graph = g;
            StackId = stackId;
        }

        public override RedoObject Undo()
        {
            if (graph != null)
            {
                Tuple<string, Point, List<Tuple<string, List<Nodes.NodeOutputConnection>>>> result = graph.RemoveNode(id);

                if (result != null)
                {
                    return new RedoCreateNode(StackId, result.Item1, result.Item2, result.Item3, graph);
                }
            }

            return null;
        }
    }
}
