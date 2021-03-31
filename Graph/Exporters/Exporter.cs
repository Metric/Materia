﻿using System.Threading.Tasks;

namespace Materia.Graph.Exporters
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