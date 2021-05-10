using System;
using System.Collections.Generic;
using Materia.Rendering.Textures;

namespace Materia.Rendering.Passes
{
    public class RenderStack : IDisposable
    {
        public List<RenderStackItem> renderers;

        public RenderStack()
        {
            renderers = new List<RenderStackItem>();
        }

        public void Add(RenderStackItem r)
        {
            renderers.Add(r);
        }

        public void Remove(RenderStackItem r)
        {
            renderers.Remove(r);
        }

        public void Process(Action renderScene = null)
        {
            GLTexture2D[] lastOuputs = null;
            for(int i = 0; i < renderers.Count; ++i)
            {
                renderers[i].Render(lastOuputs, out lastOuputs, renderScene);
            }
        }

        public void Dispose()
        {
            for(int i = 0; i < renderers.Count; ++i)
            {
                renderers[i].Dispose();
            }

            renderers.Clear();
        }
    }
}
