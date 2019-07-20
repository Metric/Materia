using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Materia.Material;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Graphics;
using System.IO;
using Materia.Imaging.GLProcessing;
using Materia.UI.Components;
using Materia.UI.Helpers;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using Materia.Undo;
using Materia.Nodes;
using Materia.UI;
using Materia.Settings;
using NLog;
using System.Windows.Threading;

namespace Materia
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    public partial class MainWindow : Window
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        public static MainWindow Instance { get; protected set; }

        protected List<UIGraph> graphs;

        protected List<LayoutDocument> documents;

        protected Dictionary<UIGraph, string> paths;

        protected UIPopupShelf popupShelf;

        protected RecentSettings recent;

        protected TKGL tk;

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            recent = new RecentSettings();
            recent.Load();
            graphs = new List<UIGraph>();
            documents = new List<LayoutDocument>();
            LoadLayout();
            mnuGraphSettings.IsEnabled = false;
            GraphDocuments.PropertyChanged += GraphDocuments_PropertyChanged;

            UndoRedoManager.OnUndo += UndoRedoManager_OnUndo;
            UndoRedoManager.OnRedo += UndoRedoManager_OnRedo;
            UndoRedoManager.OnRedoAdded += UndoRedoManager_OnRedoAdded;
            UndoRedoManager.OnUndoAdded += UndoRedoManager_OnUndoAdded;

            mnuRedo.IsEnabled = false;
            mnuUndo.IsEnabled = false;

            //set node context
            Materia.Nodes.Node.AppContext = new DispatcherSynchronizationContext();
            //initialize OpenTK GL Abstraction Layer
            tk = new TKGL();
        }

        private void BuildRecentSubMenu()
        {
            RecentMenu.Items.Clear();

            foreach(RecentSettings.RecentPath p in recent.Paths)
            {
                MenuItem recent = new MenuItem();
                recent.Header = p.path;
                recent.Foreground = new SolidColorBrush(Colors.Black);
                recent.Click += Recent_Click;
                RecentMenu.Items.Add(recent);
            }
        }

        private void Recent_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mnu = sender as MenuItem;
            string path = mnu.Header.ToString();
            HandleOpen(path);
        }

        private void UndoRedoManager_OnUndoAdded(string id, int count)
        {
            if(graphs.Count > 0)
            {
                var g = graphs[GraphDocuments.SelectedContentIndex];

                if(g.Id.Equals(id))
                {
                    mnuUndo.IsEnabled = true;
                }
            }
        }

        private void UndoRedoManager_OnRedoAdded(string id, int count)
        {
            if (graphs.Count > 0)
            {
                var g = graphs[GraphDocuments.SelectedContentIndex];

                if (g.Id.Equals(id))
                {
                    mnuRedo.IsEnabled = true;
                }
            }
        }

        private void UndoRedoManager_OnRedo(string id, int count)
        {
            if (graphs.Count > 0)
            {
                var g = graphs[GraphDocuments.SelectedContentIndex];

                if (g.Id.Equals(id))
                {
                    if (count > 0)
                    {
                        mnuRedo.IsEnabled = true;
                    }
                    else
                    {
                        mnuRedo.IsEnabled = false;
                    }
                }
            }
        }

        private void UndoRedoManager_OnUndo(string id, int count)
        {
            if (graphs.Count > 0)
            {
                var g = graphs[GraphDocuments.SelectedContentIndex];

                if (g.Id.Equals(id))
                {
                    if (count > 0)
                    {
                        mnuUndo.IsEnabled = true;
                    }
                    else
                    {
                        mnuUndo.IsEnabled = false;
                    }
                }
            }
        }

        private void GraphDocuments_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName.Equals("SelectedContentIndex"))
            {
                //clear this if we are changing tabs
                //but still have a node selected
                UINodePoint.SelectOrigin = null;

                if(graphs.Count > 0 && GraphDocuments.SelectedContentIndex > -1 && GraphDocuments.SelectedContentIndex < graphs.Count)
                {
                    var g = graphs[GraphDocuments.SelectedContentIndex];

                    if(UndoRedoManager.UndoCount(g.Id) > 0)
                    {
                        mnuUndo.IsEnabled = true;
                    }
                    else
                    {
                        mnuUndo.IsEnabled = false;
                    }

                    if(UndoRedoManager.RedoCount(g.Id) > 0)
                    {
                        mnuRedo.IsEnabled = true;
                    }
                    else
                    {
                        mnuRedo.IsEnabled = false;
                    }

                    if(UINodeParameters.Instance != null)
                    {
                        UINodeParameters.Instance.ClearView();
                    }
                }
            }
        }

        public void MarkModified()
        {
            if(GraphDocuments.SelectedContentIndex > -1 && GraphDocuments.SelectedContentIndex < graphs.Count)
            {
                UIGraph ugraph = graphs[GraphDocuments.SelectedContentIndex];
                ugraph.MarkModified();
            }
        }

        public void Push(Node n, Graph g, string param = null, GraphStackType type = GraphStackType.Parameter)
        {
            if(GraphDocuments.SelectedContentIndex > -1 && GraphDocuments.SelectedContentIndex < graphs.Count)
            {
                UIGraph ugraph = graphs[GraphDocuments.SelectedContentIndex];
                ugraph.Push(n, g, type, param);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            ///edit
            if(item.Header.ToString().ToLower().Contains("graph setting"))
            {
                if(UINodeParameters.Instance != null && graphs.Count > 0)
                {
                    if (GraphDocuments.SelectedContentIndex > -1)
                    {
                        var graph = graphs[GraphDocuments.SelectedContentIndex];
                        if (graph.Graph != null)
                        {
                            UINodeParameters.Instance.SetActive(graph.Graph);
                        }
                    }
                }
            }
            else if(item.Header.ToString().ToLower().Contains("redo"))
            {
                if (GraphDocuments.SelectedContentIndex > -1)
                {
                    var graph = graphs[GraphDocuments.SelectedContentIndex];
                    graph.TryAndRedo();
                }
            }
            else if(item.Header.ToString().ToLower().Contains("undo"))
            {
                if (GraphDocuments.SelectedContentIndex > -1)
                {
                    var graph = graphs[GraphDocuments.SelectedContentIndex];
                    graph.TryAndUndo();
                }
            }
            ///windows
            else if(item.Header.ToString().ToLower().Contains("3d"))
            {
                if (Preview3DPane.IsVisible)
                {
                    Preview3DPane.Hide();
                }
                else
                {
                    Preview3DPane.Show();
                }
            }
            else if(item.Header.ToString().ToLower().Contains("2d"))
            {
                if(Preview2DPane.IsVisible)
                {
                    Preview2DPane.Hide();
                }
                else
                {
                    Preview2DPane.Show();
                }
            }
            else if(item.Header.ToString().ToLower().Contains("parameters"))
            {
                if(ParametersPane.IsVisible)
                {
                    ParametersPane.Hide();
                }
                else
                {
                    ParametersPane.Show();
                }
            }
            else if(item.Header.ToString().ToLower().Contains("shelf"))
            {
                if(ShelfPane.IsVisible)
                {
                    ShelfPane.Hide();
                }
                else
                {
                    ShelfPane.Show();
                }
            }
            else if(item.Header.ToString().ToLower().Contains("log"))
            {
                if(LogPane.IsVisible)
                {
                    LogPane.Hide();
                }
                else
                {
                    LogPane.Show();
                }
            }
            else if (item.Header.ToString().ToLower().Contains("close all graph"))
            {
                for (int i = 0; i < documents.Count; i++)
                {
                    var doc = documents[i];
                    doc.Close();
                }
            }
            //file menu
            else if (item.Header.ToString().ToLower().Contains("save as"))
            {
                if (graphs.Count > 0)
                {
                    System.Windows.Forms.SaveFileDialog svf = new System.Windows.Forms.SaveFileDialog();
                    svf.CheckPathExists = true;
                    svf.DefaultExt = ".mtg";
                    svf.Filter = "Materia Graph (*.mtg)|*.mtg";

                    if (svf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        UIGraph g = graphs[GraphDocuments.SelectedContentIndex];
                        if (g != null)
                        {
                            g.SaveAs(svf.FileName);

                            var doc = documents[GraphDocuments.SelectedContentIndex];
                            doc.Title = g.GraphName;
                        }
                    }
                }
            }
            else if (item.Header.ToString().ToLower().Contains("save"))
            {
                if (graphs.Count > 0)
                {
                    UIGraph g = graphs[GraphDocuments.SelectedContentIndex];
                    HandleSave(g);
                    var doc = documents[GraphDocuments.SelectedContentIndex];
                    doc.Title = g.GraphName;
                }
            }
            else if (item.Header.ToString().ToLower().Contains("open"))
            {
                System.Windows.Forms.OpenFileDialog ovf = new System.Windows.Forms.OpenFileDialog();
                ovf.CheckFileExists = true;
                ovf.CheckPathExists = true;
                ovf.DefaultExt = ".mtg";
                ovf.Filter = "Materia Graph (*.mtg)|*.mtg";

                if (ovf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    HandleOpen(ovf.FileName);
                }
            }
            else if(item.Header.ToString().ToLower().Contains("new"))
            {
                UINewGraph ngraphDialog = new UINewGraph();

                ngraphDialog.Owner = this;

                if(ngraphDialog.ShowDialog() == false)
                {
                    return;
                }

                NewGraph(ngraphDialog.Result);
                Log.Info("New Graph Created");
            }
            else if(item.Header.ToString().ToLower().Contains("export output"))
            {
                if(graphs.Count > 0 && GraphDocuments.SelectedContentIndex > -1 
                    && GraphDocuments.SelectedContentIndex < graphs.Count)
                {
                    UIExportOutputs exportdialog = new UIExportOutputs(graphs[GraphDocuments.SelectedContentIndex]);
                    exportdialog.ShowDialog();
                }
            }
        }

        void HandleOpen(string path)
        {
            if (File.Exists(path))
            {
                UIGraph g = NewGraph();

                if (g != null)
                {
                    g.LoadGraph(path);

                    var doc = documents[GraphDocuments.SelectedContentIndex];
                    doc.Title = g.GraphName;

                    Log.Info("Opened Graph {0}", g.GraphName);

                    recent.Add(path);

                    BuildRecentSubMenu();
                }
            }
            else
            {
                Log.Warn("File does not exist: " + path);
            }
        }

        void HandleSave(UIGraph g)
        {
            if (string.IsNullOrEmpty(g.FilePath))
            {
                System.Windows.Forms.SaveFileDialog svf = new System.Windows.Forms.SaveFileDialog();
                svf.CheckPathExists = true;
                svf.DefaultExt = ".mtg";
                svf.Filter = "Materia Graph (*.mtg)|*.mtg";

                if (svf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (g != null)
                    {
                        g.Save(svf.FileName);
                        recent.Add(svf.FileName);
                        BuildRecentSubMenu();
                    }
                }
            }
            else
            {
                if (g != null)
                {
                    g.Save();
                }
            }

            Log.Info("Saved Graph {0}", g.GraphName);
        }

        UIGraph NewGraph(Graph template = null)
        {
            UIGraph g = new UIGraph(template);
            LayoutDocument doc = new LayoutDocument();
            doc.Content = g;
            doc.Title = g.GraphName;
            doc.Closing += Doc_Closing;
            doc.ContentId = g.GraphName + graphs.Count;
            doc.CanFloat = false;
            doc.CanMove = false;

            documents.Add(doc);
            graphs.Add(g);

            GraphDocuments.Children.Add(doc);
            GraphDocuments.SelectedContentIndex = documents.Count - 1;

            mnuGraphSettings.IsEnabled = true;

            return g;
        }

        private void Doc_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LayoutDocument doc = sender as LayoutDocument;

            int idx = documents.IndexOf(doc);

            if(idx > -1)
            {
                if (idx >= 0 && idx < graphs.Count)
                {
                    var g = graphs[idx];
                    if (g.Modified && !g.ReadOnly)
                    {
                        if (MessageBox.Show(this, g.GraphName + " has been modified. Do you want to save the changes?", "Save Changes", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            HandleSave(g);
                        }

                        graphs.RemoveAt(idx);
                        documents.Remove(doc);
                        g.Release();
                    }
                    else
                    {
                        graphs.RemoveAt(idx);
                        documents.Remove(doc);
                        g.Release();
                    }
                }
            }

            if(graphs.Count == 0)
            {
                mnuGraphSettings.IsEnabled = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            for(int i = 0; i < graphs.Count; i++)
            {
                UIGraph g = graphs[i];
                if (g.Modified && !g.ReadOnly)
                {
                    var result = MessageBox.Show(this, g.GraphName + " has been modified. Do you want to save the changes?", "Save Changes", MessageBoxButton.YesNoCancel);
                    if (result == MessageBoxResult.Yes)
                    {
                        HandleSave(g);
                    }
                    else if (result == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }

            recent.Save();

            //release all opengl content
            foreach(UIGraph g in graphs)
            {
                g.Release();
            }

            graphs.Clear();

            //clear material and shader caches
            PBRMaterial.ReleaseBRDF();
            ImageProcessor.ReleaseAll();
            Material.Material.ReleaseAll();

            //release gl view
            if (UI3DPreview.Instance != null)
            {
                UI3DPreview.Instance.Release();
            }

            if(UIPreviewPane.Instance != null)
            {
                UIPreviewPane.Instance.Release();
            }

            ViewContext.Dispose();

            //save layout
            SaveLayout();

            popupShelf.Close();
            popupShelf = null;
        }

        private void SaveLayout()
        {
            XmlLayoutSerializer layout = new XmlLayoutSerializer(Docker);
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "layout.xml");
            layout.Serialize(path);
        }

        private void LoadLayout()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "layout.xml");
            if (System.IO.File.Exists(path))
            {
                XmlLayoutSerializer layout = new XmlLayoutSerializer(Docker);
                layout.Deserialize(path);
                GraphDocuments = Docker.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();

                if (GraphDocuments != null)
                {
                    GraphDocuments.Children.Clear();
                }

                var anchorables = Docker.Layout.Descendents().OfType<LayoutAnchorable>();

                Preview3DPane = anchorables.FirstOrDefault(m => m.ContentId.Equals("3dpreview"));
                Preview2DPane = anchorables.FirstOrDefault(m => m.ContentId.Equals("2dpreview"));
                ShelfPane = anchorables.FirstOrDefault(m => m.ContentId.Equals("shelf"));
                ParametersPane = anchorables.FirstOrDefault(m => m.ContentId.Equals("parameters"));

                //have to do this for saved layouts in case the layout does not have the original
                var tmpLog = anchorables.FirstOrDefault(m => m.ContentId.Equals("log"));

                if(tmpLog == null)
                {
                    var firstLayout = Docker.Layout.Descendents().OfType<LayoutPanel>().FirstOrDefault();
                    
                    if(firstLayout != null)
                    {
                        firstLayout.Children.Add(LogAnchor);
                    }
                }
                else
                {
                    LogPane = tmpLog;
                }

                Log.Info("Previouse Layout Loaded");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BuildRecentSubMenu();
            MateriaInputManager.Init();
            RegisterInputActions();

            popupShelf = new UIPopupShelf();
            popupShelf.Owner = this;
            popupShelf.Hide();

            if (App.Current.Properties.Contains("OpenFile"))
            {
                try
                {
                    string path = (string)App.Current.Properties["OpenFile"];
                    if (string.IsNullOrEmpty(path)) return;
                    HandleOpen(path);
                }
                catch
                {

                }
            }

            Log.Info("Main Window Loaded");
        }

        private void RegisterInputActions()
        {
            MateriaInputManager.Add(InputManagerCommand.Clear, (e) =>
            {
                UINodePoint.SelectOrigin = null;
                if(popupShelf != null)
                {
                    popupShelf.Hide();
                }
            });
            MateriaInputManager.Add(InputManagerCommand.Copy, (e) =>
            {
                if (GraphDocuments.SelectedContentIndex > -1 && GraphDocuments.SelectedContentIndex < graphs.Count)
                {
                    var g = graphs[GraphDocuments.SelectedContentIndex];
                    g.TryAndCopy();
                }
            });
            MateriaInputManager.Add(InputManagerCommand.Paste, (e) =>
            {
                if (GraphDocuments.SelectedContentIndex > -1 && GraphDocuments.SelectedContentIndex < graphs.Count)
                {
                    var g = graphs[GraphDocuments.SelectedContentIndex];
                    g.TryAndPaste();
                }
            });
            MateriaInputManager.Add(InputManagerCommand.Save, (e) =>
            {
                if (GraphDocuments.SelectedContentIndex > -1 && GraphDocuments.SelectedContentIndex < graphs.Count)
                {
                    var g = graphs[GraphDocuments.SelectedContentIndex];
                    HandleSave(g);
                }
            });
            MateriaInputManager.Add(InputManagerCommand.Delete, (e) =>
            {
                if (!(Keyboard.FocusedElement is IUIGraphNode) && !(Keyboard.FocusedElement is UIGraph)
                   && Keyboard.FocusedElement != this && !(Keyboard.FocusedElement is LayoutDocument))
                {
                    return;
                }

                if (GraphDocuments.SelectedContentIndex > -1 && GraphDocuments.SelectedContentIndex < graphs.Count)
                {
                    var g = graphs[GraphDocuments.SelectedContentIndex];
                    g.TryAndDelete();
                }
            });
            MateriaInputManager.Add(InputManagerCommand.Undo, (e) =>
            {
                if (GraphDocuments.SelectedContentIndex > -1 && GraphDocuments.SelectedContentIndex < graphs.Count)
                {
                    var g = graphs[GraphDocuments.SelectedContentIndex];
                    g.TryAndUndo();
                }
            });
            MateriaInputManager.Add(InputManagerCommand.Redo, (e) =>
            {
                if (GraphDocuments.SelectedContentIndex > -1 && GraphDocuments.SelectedContentIndex < graphs.Count)
                {
                    var g = graphs[GraphDocuments.SelectedContentIndex];
                    g.TryAndRedo();
                }
            });
            MateriaInputManager.Add(InputManagerCommand.PopupShelf, (e) =>
            {
                if (popupShelf == null || popupShelf.Visibility == Visibility.Visible) return;

                if(!(Keyboard.FocusedElement is IUIGraphNode) && !(Keyboard.FocusedElement is UIGraph) 
                    && Keyboard.FocusedElement != this && !(Keyboard.FocusedElement is LayoutDocument))
                {
                    return;
                }

                if(GraphDocuments.SelectedContentIndex > -1 && GraphDocuments.SelectedContentIndex < graphs.Count)
                {
                    var g = graphs[GraphDocuments.SelectedContentIndex];
                    g.PrepareInsert();
                    var pop = popupShelf;
                    pop.Graph = g;
                    Point m = Mouse.GetPosition(this);
                    Point m2 = Mouse.GetPosition(g.ViewPort);

                    if(m2.X < 0 || m2.Y < 0 || m2.X > g.ViewPort.ActualWidth || m2.Y > g.ViewPort.ActualHeight)
                    {
                        return;
                    }

                    pop.Left = m.X;
                    pop.Top = m.Y;
                    pop.Show();
                    pop.Focus();
                }
            });
            MateriaInputManager.Add(InputManagerCommand.Comment, (e) =>
            {

                if (!(Keyboard.FocusedElement is IUIGraphNode) && !(Keyboard.FocusedElement is UIGraph)
                    && Keyboard.FocusedElement != this && !(Keyboard.FocusedElement is LayoutDocument))
                {
                    return;
                }

                if (GraphDocuments.SelectedContentIndex > -1 && GraphDocuments.SelectedContentIndex < graphs.Count)
                {
                    var g = graphs[GraphDocuments.SelectedContentIndex];

                    Point m2 = Mouse.GetPosition(g.ViewPort);

                    if (m2.X < 0 || m2.Y < 0 || m2.X > g.ViewPort.ActualWidth || m2.Y > g.ViewPort.ActualHeight)
                    {
                        return;
                    }

                    g.TryAndComment();
                }
            });
            MateriaInputManager.Add(InputManagerCommand.Pin, (e) =>
            {
                if (!(Keyboard.FocusedElement is IUIGraphNode) && !(Keyboard.FocusedElement is UIGraph)
                   && Keyboard.FocusedElement != this && !(Keyboard.FocusedElement is LayoutDocument))
                {
                    return;
                }

                if (GraphDocuments.SelectedContentIndex > -1 && GraphDocuments.SelectedContentIndex < graphs.Count)
                {
                    var g = graphs[GraphDocuments.SelectedContentIndex];

                    Point m2 = Mouse.GetPosition(g.ViewPort);

                    if (m2.X < 0 || m2.Y < 0 || m2.X > g.ViewPort.ActualWidth || m2.Y > g.ViewPort.ActualHeight)
                    {
                        return;
                    }

                    g.TryAndPin();
                }
            });
            MateriaInputManager.Add(InputManagerCommand.NextPin, (e) =>
            {
                if (!(Keyboard.FocusedElement is IUIGraphNode) && !(Keyboard.FocusedElement is UIGraph)
                   && Keyboard.FocusedElement != this && !(Keyboard.FocusedElement is LayoutDocument))
                {
                    return;
                }

                if (GraphDocuments.SelectedContentIndex > -1 && GraphDocuments.SelectedContentIndex < graphs.Count)
                {
                    var g = graphs[GraphDocuments.SelectedContentIndex];
                    e.Handled = true;
                    g.NextPin();
                }
            });
        }
    }
}
