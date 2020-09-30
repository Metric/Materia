using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

/*
  copyright s-hull.org 2011
  released under the contributors beerware license

  contributors: Phil Atkin, Dr Sinclair.
*/
namespace DelaunayTriangulator
{
    /// <summary>
    /// Vertices belonging to the convex hull need to maintain a point and triad index
    /// </summary>
    internal class HullVertex : Vertex
    {
        public int pointsIndex;
        public int triadIndex;

        public HullVertex(List<Vector2> points, int pointIndex)
        {
            x = points[pointIndex].x;
            y = points[pointIndex].y;
            pointsIndex = pointIndex;
            triadIndex = 0;
        }
    }

    /// <summary>
    /// Hull represents a list of vertices in the convex hull, and keeps track of
    /// their indices (into the associated points list) and triads
    /// </summary>
    class Hull : List<HullVertex>
    {
        private int NextIndex(int index)
        {
            if (index == Count - 1)
                return 0;
            else
                return index + 1;
        }

        /// <summary>
        /// Return vector from the hull point at index to next point
        /// </summary>
        public void VectorToNext(int index, out float dx, out float dy)
        {
            Vertex et = this[index], en = this[NextIndex(index)];

            dx = en.x - et.x;
            dy = en.y - et.y;
        }

        /// <summary>
        /// Return whether the hull vertex at index is visible from the supplied coordinates
        /// </summary>
        public bool EdgeVisibleFrom(int index, float dx, float dy)
        {
            float idx, idy;
            VectorToNext(index, out idx, out idy);

            float crossProduct = -dy * idx + dx * idy;
            return crossProduct < 0;
        }

        /// <summary>
        /// Return whether the hull vertex at index is visible from the point
        /// </summary>
        public bool EdgeVisibleFrom(int index, HullVertex point)
        {
            float idx, idy;
            VectorToNext(index, out idx, out idy);

            float dx = point.x - this[index].x;
            float dy = point.y - this[index].y;

            float crossProduct = -dy * idx + dx * idy;
            return crossProduct < 0;
        }
    }
}
