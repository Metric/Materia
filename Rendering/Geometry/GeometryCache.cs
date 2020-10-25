using Materia.Rendering.Geometry;
using Materia.Rendering.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Materia.Rendering.Geometry
{
    public static class GeometryCache
    {
        public static Dictionary<Type, IDisposeShared> DisposeCache { get; private set; } = new Dictionary<Type, IDisposeShared>();
        public static void RegisterForDispose(IDisposeShared o)
        {
            DisposeCache[o.GetType()] = o;
        }

        public static void Dispose()
        {
            List<IDisposeShared> items = DisposeCache.Values.ToList();
            for (int i = 0; i < items.Count; ++i)
            {
                items[i].DisposeShared();
            }

            DisposeCache.Clear();
        }
    }
}
