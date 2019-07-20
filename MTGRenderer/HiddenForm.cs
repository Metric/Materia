using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Materia.Nodes;
using Materia.Exporters;
using Materia.Hdri;

namespace MTGRenderer
{
    public partial class HiddenForm : Form
    {
        protected SynchronizationContext context;

        public OpenTK.GLControl View { get; protected set; }

        Graph graph;

        public HiddenForm()
        {
            InitializeComponent();
            View = glView;
            context = new WindowsFormsSynchronizationContext();
            Materia.Nodes.Node.AppContext = context;
            HdriManager.Context = context;
            HdriManager.OnHdriLoaded += HdriManager_OnHdriLoaded;
        }

        private void HdriManager_OnHdriLoaded(Materia.Textures.GLTextuer2D irradiance, Materia.Textures.GLTextuer2D prefiltered)
        {
            Materia.Nodes.Atomic.MeshNode.Irradiance = irradiance;
            Materia.Nodes.Atomic.MeshNode.Prefilter = prefiltered;
        }

        public void Render(string graphPath, string exportPath, int type)
        {
            Graph g = graph = LoadGraph(graphPath);
            Exporter exporter = null;

            if (g == null) return;

            switch(type)
            {
                case 0:
                    exporter = new SeparateExporter(g);
                    break;
                case 1:
                    exporter = new Unity5Exporter(g);
                    break;
                case 2:
                    exporter = new Unreal4Exporter(g);
                    break;
                default:
                    exporter = new SeparateExporter(g);
                    break;
            }

            exporter.ExportSync(exportPath);
        }

        public static Graph LoadGraph(string path)
        {
            //set to sync only for node updates
            //that way we ensure everything is ready for
            //export properly without worrying about async tasks
            //however it will cause slightly longer load times
            //for the graph but meh
            Materia.Nodes.Node.Async = false;
            Graph g = new Graph("temp");

            if(System.IO.File.Exists(path) && path.EndsWith(".mtg"))
            {
                g.FromJson(System.IO.File.ReadAllText(path));
                HdriManager.Selected = g.HdriIndex;
                return g;
            }

            return null;
        }

        private void glView_Load(object sender, EventArgs e)
        {

        }

        private void HiddenForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(graph != null)
            {
                graph.Dispose();
            }

            HdriManager.Release();
            Materia.Material.PBRMaterial.ReleaseBRDF();
            Materia.Material.Material.ReleaseAll();
            Materia.Imaging.GLProcessing.ImageProcessor.ReleaseAll();
        }
    }
}
