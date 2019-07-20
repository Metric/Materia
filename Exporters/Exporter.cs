using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Exporters
{
    public abstract class Exporter
    {
        public delegate void Progress(int current, int total, float progress);
        public event Progress OnProgress;

        protected void ProgressChanged(int current, int total, float progress)
        {
            if(OnProgress != null)
            {
                OnProgress.Invoke(current, total, progress);
            }
        }

        public abstract void ExportSync(string path);

        public abstract Task Export(string path);
    }
}
