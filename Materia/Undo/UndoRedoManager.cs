using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Undo
{
    public class UndoRedoManager
    {
        public delegate void UndoChanged(string id, int count);
        public delegate void RedoChanged(string id, int count);

        public static event UndoChanged OnUndo;
        public static event RedoChanged OnRedo;

        public static event UndoChanged OnUndoAdded;
        public static event RedoChanged OnRedoAdded;

        protected static Dictionary<string, Stack<UndoObject>> undos = new Dictionary<string, Stack<UndoObject>>();
        protected static Dictionary<string, Stack<RedoObject>> redos = new Dictionary<string, Stack<RedoObject>>();

        public static int MaxUndos = 200; 

        public static int UndoCount(string stackId)
        {
            Stack<UndoObject> stack = null;

            if(undos.TryGetValue(stackId, out stack))
            {
                return stack.Count;
            }

            return 0;
        }

        public static int RedoCount(string stackId)
        {
            Stack<RedoObject> stack = null;

            if(redos.TryGetValue(stackId, out stack))
            {
                return stack.Count;
            }

            return 0;
        }

        public static void AddUndo(UndoObject o)
        {
            if(o != null)
            {
                string id = o.StackId;

                Stack<UndoObject> stack = null;

                if(!undos.TryGetValue(id, out stack))
                {
                    stack = new Stack<UndoObject>();
                    undos[id] = stack;
                }

                if (stack.Count + 1 > MaxUndos)
                {
                    //we skip the first twenty in order
                    //to not have to recalc this every time
                    //we add to the stack when at max stack
                    //this is an optimization
                    var items = stack.ToArray().Skip(20);
                    stack.Clear();

                    foreach (UndoObject ob in items)
                    {
                        stack.Push(ob);
                    }
                }

                stack.Push(o);

                if(OnUndoAdded != null)
                {
                    OnUndoAdded.Invoke(id, stack.Count);
                }
            }
        }

        public static void AddRedo(RedoObject o)
        {
            if(o != null)
            {
                string id = o.StackId;

                Stack<RedoObject> stack = null;

                if (!redos.TryGetValue(id, out stack))
                {
                    stack = new Stack<RedoObject>();
                    redos[id] = stack;
                }

                if (stack.Count + 1 > MaxUndos)
                {
                    //we skip the first twenty in order
                    //to not have to recalc this every time
                    //we add to the stack when at max stack
                    //this is an optimization
                    var items = stack.ToArray().Skip(20);
                    stack.Clear();

                    foreach(RedoObject ob in items)
                    {
                        stack.Push(ob);
                    }
                }

                stack.Push(o);

                if(OnRedoAdded != null)
                {
                    OnRedoAdded.Invoke(id, stack.Count);
                }
            }
        }

        public static void Undo(string stackId)
        {
            Stack<UndoObject> stack = null;

            if (undos.TryGetValue(stackId, out stack))
            {
                if (stack.Count > 0)
                {
                    var p = stack.Pop();
                    var r = p.Undo();

                    AddRedo(r);

                    if (OnUndo != null)
                    {
                        OnUndo.Invoke(stackId, stack.Count);
                    }
                }
            }
        }

        public static void Redo(string stackId)
        {
            Stack<RedoObject> stack = null;

            if (redos.TryGetValue(stackId, out stack))
            {
                if (redos.Count > 0)
                {
                    var p = stack.Pop();
                    var r = p.Redo();

                    AddUndo(r);

                    if (OnRedo != null)
                    {
                        OnRedo.Invoke(stackId, stack.Count);
                    }
                }
            }
        }
    }
}
