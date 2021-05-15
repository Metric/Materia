﻿using System;
using System.Collections.Generic;
using Materia.Rendering.Textures;

namespace Materia.Rendering.Passes
{

    public enum RenderStackState
    {
        Color = 0,
        Effect = 1,
        Skybox = 2
    }

    public class RenderStack : IDisposable
    {
        public GLTexture2D[] Output { get; protected set; }

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

        public void Process(Action<RenderStackState> renderScene = null)
        {
            GLTexture2D[] lastOuputs = null;
            for(int i = 0; i < renderers.Count; ++i)
            {
                renderers[i].Render(lastOuputs, out lastOuputs, renderScene);
            }
            Output = lastOuputs;
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
