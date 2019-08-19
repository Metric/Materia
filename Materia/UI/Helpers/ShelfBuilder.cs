using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.UI.Helpers
{
    public class ShelfBuilder
    {
        public delegate void BuildComplete(ShelfBuilder builder);
        public event BuildComplete OnBuildComplete;

        public ShelfBuilderItem Root { get; protected set; }

        public ShelfBuilder()
        {
            Root = new ShelfBuilderItem("Categories");
        }

        public void Build()
        {
            Task.Run(() =>
            {
                string dir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shelf");
                Stack<string> stack = new Stack<string>();
                stack.Push(dir);

                while (stack.Count > 0)
                {
                    List<string> sorter = new List<string>();
                    Dictionary<string, string> lookup = new Dictionary<string, string>();
                    string d = stack.Pop();

                    ShelfBuilderItem child = null;
                    var tmp = d.Replace(dir, "");
                    if (string.IsNullOrEmpty(tmp))
                    {
                        child = Root;
                    }
                    else
                    {
                        child = Root.FindChild(d.Replace(dir, ""));
                    }

                    if (System.IO.Directory.Exists(d))
                    {
                        string[] subs = System.IO.Directory.GetDirectories(d);

                        foreach (string s in subs)
                        {
                            string ds = System.IO.Path.Combine(d, s);
                            var split = ds.Split(System.IO.Path.DirectorySeparatorChar);
                            var achild = new ShelfBuilderItem(split[split.Length - 1]);
                            child.Add(achild);
                            stack.Push(ds);
                        }

                        string[] files = System.IO.Directory.GetFiles(d);

                        foreach (string p in files)
                        {
                            if (System.IO.Path.GetExtension(p).Equals(".mtg")
                                || System.IO.Path.GetExtension(p).Equals(".mti")
                                || System.IO.Path.GetExtension(p).Equals(".mtga"))
                            {
                                string fname = System.IO.Path.GetFileNameWithoutExtension(p);
                                sorter.Add(fname);
                                lookup[fname] = System.IO.Path.Combine(d, p);
                            }
                        }

                        sorter.Sort();

                        foreach (string fname in sorter)
                        {
                            string p = null;
                            if (lookup.TryGetValue(fname, out p))
                            {
                                ShelfResourceItem r = new ShelfResourceItem();
                                r.Title = fname;
                                if (System.IO.Path.GetExtension(p).Equals(".mtg") 
                                || System.IO.Path.GetExtension(p).Equals(".mtga"))
                                {
                                    r.Type = p;
                                    child.Add(r);
                                }
                                else if (System.IO.Path.GetExtension(p).Equals(".mti"))
                                {
                                    r.Type = System.IO.File.ReadAllText(p);
                                    child.Add(r);
                                }
                            }
                        }
                    }
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    if(OnBuildComplete != null)
                    {
                        OnBuildComplete.Invoke(this);
                    }
                });
            });
        }

        public class ShelfResourceItem
        {
            public string Title { get; set; }
            public string Path { get; set; }
            public string Type { get; set; }
        }

        public class ShelfBuilderItem
        {
            public string Path { get; protected set; }
            public string BaseName { get; protected set; }

            protected List<ShelfBuilderItem> children;
            public List<ShelfBuilderItem> Children
            {
                get
                {
                    return children.ToList();
                }
            }
            public ShelfBuilderItem Parent { get; protected set; }

            public List<ShelfResourceItem> Nodes = new List<ShelfResourceItem>();

            public ShelfBuilderItem()
            {
                children = new List<ShelfBuilderItem>();
            }

            public ShelfBuilderItem(string name)
            {
                BaseName = name;
                Path = BaseName;
                children = new List<ShelfBuilderItem>();
            }

            public void Add(ShelfBuilderItem item)
            {
                item.Path = System.IO.Path.Combine(Path, item.Path);
                item.Parent = this;
                children.Add(item);
            }

            public void Add(ShelfResourceItem nr)
            {
                nr.Path = Path;
                Nodes.Add(nr);
            }

            public void Remove(ShelfResourceItem nr)
            {
                if (Nodes.Remove(nr))
                {
                    nr.Path = "";
                }
            }

            public void Remove(ShelfBuilderItem item)
            {
                bool contained = children.Remove(item);
                if (contained)
                {
                    item.Parent = null;
                    item.Path = item.BaseName;
                }
            }

            public ShelfBuilderItem FindChild(string path)
            {
                if (string.IsNullOrEmpty(path)) return this;

                ShelfBuilderItem parent = this;
                string[] split = path.Split(new string[] { System.IO.Path.DirectorySeparatorChar + "" }, StringSplitOptions.RemoveEmptyEntries);

                int i = 0;
                for (i = 0; i < split.Length; i++)
                {
                    var s = split[i];
                    if (i == 0 && s.Equals(BaseName))
                    {
                        continue;
                    }

                    var nf = parent.children.Find(m => m.BaseName.Equals(s));

                    if (nf != null)
                    {
                        parent = nf;
                    }
                    else
                    {
                        return parent;
                    }
                }

                return parent;
            }
        }
    }
}
