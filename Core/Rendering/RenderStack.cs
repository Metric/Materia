using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Textures;

namespace Materia.Rendering
{
    public class RenderStack
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

        public void Process()
        {
            GLTextuer2D[] lastOuputs = null;
            for(int i = 0; i < renderers.Count; ++i)
            {
                renderers[i].Render(lastOuputs, out lastOuputs);
            }
        }

        public void Release()
        {
            for(int i = 0; i < renderers.Count; ++i)
            {
                renderers[i].Release();
            }

            renderers.Clear();
        }
    }
}
