using Avalonia.Controls;
using InfinityUI.Components;
using InfinityUI.Components.Layout;
using InfinityUI.Controls;
using InfinityUI.Core;
using Materia.Graph;
using Materia.Rendering.Mathematics;
using MateriaCore.Components.Dialogs;
using MateriaCore.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MateriaCore.Components.GL
{
    //note: there should only ever be one of these active
    public class UIDocuments : UIObject
    {
        public static UIDocuments Current { get; protected set; }
        public event Action<UIDocuments, UIDocumentTab> ActiveDocumentChanged;

        public List<UIDocumentTab> Documents { get; protected set; } = new List<UIDocumentTab>();

        #region components
        protected ToggleGroup toggleGroup;
        #endregion

        public bool IsModified
        {
            get
            {
                return Documents.Find(m => m.Graph.Modified) != null;
            }
        }

        public UIGraph ActiveGraph
        {
            get => (toggleGroup?.ActiveToggle?.Parent as UIDocumentTab)?.Graph;
        }

        public UIDocumentTab ActiveTab
        {
            get => (toggleGroup?.ActiveToggle?.Parent as UIDocumentTab);
        }

        public UIDocuments() : base()
        {
            Current = this;
            InitializeComponents();
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            GlobalEvents.On(GlobalEvent.OpenDocument, OnOpenDocument);
            GlobalEvents.On(GlobalEvent.NewDocument, OnNewDocument);
            GlobalEvents.On(GlobalEvent.SaveAllDocuments, OnSaveAll);
            GlobalEvents.On(GlobalEvent.CloseAllDocuments, OnCloseAll);
        }

        private void InitializeComponents()
        {
            RaycastTarget = true;
            var stack = AddComponent<UIStackPanel>();
            stack.Direction = Orientation.Horizontal;
            toggleGroup = AddComponent<ToggleGroup>();
            toggleGroup.ToggleChanged += ToggleGroup_ToggleChanged;
        }

        private void ToggleGroup_ToggleChanged(ToggleGroup arg1, UIToggleable arg2, int arg3)
        {
            if (arg3 < 0 || arg3 >= Documents.Count) ActiveDocumentChanged?.Invoke(this, null);
            ActiveDocumentChanged?.Invoke(this, Children[arg3] as UIDocumentTab);
        }

        private void CloseAll()
        {
            Task.Run(async () =>
            {
                var docs = Documents;
                for (int i = 0; i < docs.Count; ++i)
                {
                    var doc = docs[i];
                    var graph = doc?.Graph;
                    if (graph == null) continue;
                    if (!graph.Modified) continue;

                    //todo: show dialog asking if we want to save
                }
            }).ContinueWith(t =>
            {
                var docs = Documents;
                for (int i = 0; i < docs.Count; ++i)
                {
                    var doc = docs[i];
                    if (doc == null) continue;
                    
                    //remove it from UI
                    RemoveChild(doc);

                    var graph = doc.Graph;
                    graph?.Dispose();
                    doc.Dispose();
                }
                docs.Clear();
                ActiveDocumentChanged?.Invoke(this, null);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void SaveAll()
        {
            Task.Run(async () =>
            {
                var docs = Documents;
                for (int i = 0; i < docs.Count; ++i)
                {
                    var doc = docs[i];
                    var graph = doc?.Graph;
                    if (graph == null) continue;
                    if (string.IsNullOrEmpty(graph.Filename))
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Filters.Add(new FileDialogFilter
                        {
                            Extensions = new List<string>(new string[] { "mtg", "mtga" }),
                            Name = "Materia Graph" //todo: use localization here
                        });
                        string filename = await sfd.ShowAsync(MainWindow.Instance);
                        if (string.IsNullOrEmpty(filename)) continue;
                        graph.Save(filename);
                    }
                    else
                    {
                        graph.Save();
                    }
                }
            });
        }

        private void OnCloseAll(object sender, object arg)
        {
            CloseAll();
        }

        private void OnSaveAll(object sender, object arg)
        {
            SaveAll();
        }

        private void AddDocument(UIGraph g)
        {
            if (g == null) return;
            UIDocumentTab tab = new UIDocumentTab(g);
            tab.Close += Tab_Close;
            Documents.Add(tab);
            AddChild(tab);
            toggleGroup.SetActive(Documents.Count - 1);
        }

        private void RemoveTab(UIDocumentTab tab)
        {
            if (tab == null) return;
            int idx = Documents.IndexOf(tab);

            Documents.Remove(tab);
            RemoveChild(tab);
            tab.Dispose();

            if (Documents.Count == 0)
            {
                ActiveDocumentChanged?.Invoke(this, null);
            }
            else
            {
                if (idx <= 0) toggleGroup?.SetActive(0);
                else toggleGroup?.SetActive(idx - 1);
            }
        }

        private void Tab_Close(UIDocumentTab tab)
        {
            if (tab == null) return;
            if (tab.Graph == null)
            {
                RemoveTab(tab);
                return;
            }

            //todo: add in enum for dialog result
            var graph = tab.Graph;
            Task.Run(async () =>
            {
                if (!graph.Modified) return;
                //todo: show dialog asking if we want to save, close, cancel
            }).ContinueWith(task =>
            {
                //todo: check enum here if saved or close then continue
                //with below
                //otherwise return
                graph.Dispose();
                RemoveTab(tab);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void OnNewDocument(object sender, object arg)
        {
            if (!(arg is Graph)) return;
            UIGraph graph = new UIGraph();
            graph.Load(arg as Graph);
            AddDocument(graph);
        }

        private void OnOpenDocument(object sender, object arg)
        {
            if (!(arg is string)) return;
            string path = arg as string;
            if (string.IsNullOrEmpty(path)) return;

            UIGraph graph = new UIGraph();
            graph.Load(path);
            AddDocument(graph);
        }

        public async static Task TryAndCreate()
        {
            NewGraph ngd = new NewGraph();
            var graph = await ngd.ShowDialog<Graph>(MainWindow.Instance);
            if (graph == null) return;
            GlobalEvents.Emit(GlobalEvent.NewDocument, Current, graph);
        }

        public async static Task TryAndOpen()
        {
            FileDialogFilter f = new FileDialogFilter
            {
                Extensions = new List<string>(new string[] { "mtg", "mtga" }),
                Name = "Materia Graph" //todo: use localization here
            };

            OpenFileDialog ofd = new OpenFileDialog
            {
                Filters = new List<FileDialogFilter>(new FileDialogFilter[] { f })
            };

            ofd.AllowMultiple = false;

            string[] paths = await ofd.ShowAsync(MainWindow.Instance);
            if (paths == null || paths.Length == 0) return;

            string filepath = paths[0];
            if (string.IsNullOrEmpty(filepath)) return;

            if (!System.IO.File.Exists(filepath))
            {
                //todo: show message box with file not existing error
                return;
            }

            GlobalEvents.Emit(GlobalEvent.OpenDocument, Current, filepath);
        }
    }
}
