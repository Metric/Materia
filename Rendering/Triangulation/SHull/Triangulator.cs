using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Materia.Rendering.Mathematics;
using System.Runtime.ConstrainedExecution;
using Materia.Rendering.Spatial;

/*
  copyright s-hull.org 2011
  released under the contributors beerware license

  contributors: Phil Atkin, Dr Sinclair.
*/
namespace DelaunayTriangulator
{
    class Triangulator
    {
        private List<Vector2> points;

        public Triangulator()
        {
        }

        private void Analyse(List<Vector2> suppliedPoints, Hull hull, List<Triad> triads, bool rejectDuplicatePoints, bool hullOnly)
        {
            if (suppliedPoints.Count < 3)
                throw new ArgumentException("Number of points supplied must be >= 3");

            this.points = suppliedPoints;
            int nump = points.Count;

            float[] distance2ToCentre = new float[nump];
            int[] sortedIndices = new int[nump];

            // Choose first point as the seed
            for (int k = 0; k < nump; k++)
            {
                Vector2 p0 = points[0];
                Vector2 pk = points[k];
                distance2ToCentre[k] = Vector2.DistanceSquared(ref p0, ref pk);
                sortedIndices[k] = k;
            }

            // Sort by distance to seed point
            Array.Sort(distance2ToCentre, sortedIndices);

            // Duplicates are more efficiently rejected now we have sorted the vertices
            if (rejectDuplicatePoints)
            {
                // Search backwards so each removal is independent of any other
                for (int k = nump - 2; k >= 0; k--)
                {
                    // If the points are identical then their distances will be the same,
                    // so they will be adjacent in the sorted list
                    if ((points[sortedIndices[k]].x == points[sortedIndices[k + 1]].x) &&
                        (points[sortedIndices[k]].y == points[sortedIndices[k + 1]].y))
                    {
                        // Duplicates are expected to be rare, so this is not particularly efficient
                        Array.Copy(sortedIndices, k + 2, sortedIndices, k + 1, nump - k - 2);
                        Array.Copy(distance2ToCentre, k + 2, distance2ToCentre, k + 1, nump - k - 2);
                        nump--;
                    }
                }
            }

            //Debug.WriteLine((points.Count - nump).ToString() + " duplicate points rejected");

            if (nump < 3)
            {
                //oops not enough exit now
                return;
            }

            int mid = -1;
            float romin2 = float.MaxValue, circumCentreX = 0, circumCentreY = 0;

            // Find the point which, with the first two points, creates the triangle with the smallest circumcircle
            Triad tri = new Triad(sortedIndices[0], sortedIndices[1], 2);
            for (int kc = 2; kc < nump; kc++)
            {
                tri.c = sortedIndices[kc];
                if (tri.FindCircumcirclePrecisely(points) && tri.circumcircleR2 < romin2)
                {
                    mid = kc;
                    // Centre of the circumcentre of the seed triangle
                    romin2 = tri.circumcircleR2;
                    circumCentreX = tri.circumcircleX;
                    circumCentreY = tri.circumcircleY;
                }
                else if (romin2 * 4 < distance2ToCentre[kc])
                    break;
            }

            if (mid < 0)
            {
                return;
            }

            // Change the indices, if necessary, to make the 2th point produce the smallest circumcircle with the 0th and 1th
            if (mid != 2)
            {
                int indexMid = sortedIndices[mid];
                float distance2Mid = distance2ToCentre[mid];

                Array.Copy(sortedIndices, 2, sortedIndices, 3, mid - 2);
                Array.Copy(distance2ToCentre, 2, distance2ToCentre, 3, mid - 2);
                sortedIndices[2] = indexMid;
                distance2ToCentre[2] = distance2Mid;
            }

            // These three points are our seed triangle
            tri.c = sortedIndices[2];
            tri.MakeClockwise(points);
            tri.FindCircumcirclePrecisely(points);

            // Add tri as the first triad, and the three points to the convex hull
            triads.Add(tri);
            hull.Add(new HullVertex(points, tri.a));
            hull.Add(new HullVertex(points, tri.b));
            hull.Add(new HullVertex(points, tri.c));

            // Sort the remainder according to their distance from its centroid
            // Re-measure the points' distances from the centre of the circumcircle
            Vector2 centre = new Vector2(circumCentreX, circumCentreY);
            for (int k = 3; k < nump; k++)
            {
                Vector2 p0 = points[sortedIndices[k]];
                distance2ToCentre[k] = Vector2.DistanceSquared(ref p0, ref centre);
            }
            // Sort the _other_ points in order of distance to circumcentre
            Array.Sort(distance2ToCentre, sortedIndices, 3, nump - 3);

            // Add new points into hull (removing obscured ones from the chain)
            // and creating triangles....
            int numt = 0;
            for (int k = 3; k < nump; k++)
            {
                int pointsIndex = sortedIndices[k];
                HullVertex ptx = new HullVertex(points, pointsIndex);

                float dx = ptx.x - hull[0].x, dy = ptx.y - hull[0].y;  // outwards pointing from hull[0] to pt.

                int numh = hull.Count, numh_old = numh;
                List<int> pidx = new List<int>(), tridx = new List<int>();
                int hidx;  // new hull point location within hull.....

                if (hull.EdgeVisibleFrom(0, dx, dy))
                {
                    // starting with a visible hull facet !!!
                    int e2 = numh;
                    hidx = 0;

                    // check to see if segment numh is also visible
                    if (hull.EdgeVisibleFrom(numh - 1, dx, dy))
                    {
                        // visible.
                        pidx.Add(hull[numh - 1].pointsIndex);
                        tridx.Add(hull[numh - 1].triadIndex);

                        for (int h = 0; h < numh - 1; h++)
                        {
                            // if segment h is visible delete h
                            pidx.Add(hull[h].pointsIndex);
                            tridx.Add(hull[h].triadIndex);
                            if (hull.EdgeVisibleFrom(h, ptx))
                            {
                                hull.RemoveAt(h);
                                h--;
                                numh--;
                            }
                            else
                            {
                                // quit on invisibility
                                hull.Insert(0, ptx);
                                numh++;
                                break;
                            }
                        }
                        // look backwards through the hull structure
                        for (int h = numh - 2; h > 0; h--)
                        {
                            // if segment h is visible delete h + 1
                            if (hull.EdgeVisibleFrom(h, ptx))
                            {
                                pidx.Insert(0, hull[h].pointsIndex);
                                tridx.Insert(0, hull[h].triadIndex);
                                hull.RemoveAt(h + 1);  // erase end of chain
                            }
                            else
                                break; // quit on invisibility
                        }
                    }
                    else
                    {
                        hidx = 1;  // keep pt hull[0]
                        tridx.Add(hull[0].triadIndex);
                        pidx.Add(hull[0].pointsIndex);

                        for (int h = 1; h < numh; h++)
                        {
                            // if segment h is visible delete h  
                            pidx.Add(hull[h].pointsIndex);
                            tridx.Add(hull[h].triadIndex);
                            if (hull.EdgeVisibleFrom(h, ptx))
                            {                     // visible
                                hull.RemoveAt(h);
                                h--;
                                numh--;
                            }
                            else
                            {
                                // quit on invisibility
                                hull.Insert(h, ptx);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    int e1 = -1, e2 = numh;
                    for (int h = 1; h < numh; h++)
                    {
                        if (hull.EdgeVisibleFrom(h, ptx))
                        {
                            if (e1 < 0)
                                e1 = h;  // first visible
                        }
                        else
                        {
                            if (e1 > 0)
                            {
                                // first invisible segment.
                                e2 = h;
                                break;
                            }
                        }
                    }

                    if (e1 < 0)
                    {
                        continue;
                    }

                    // triangle pidx starts at e1 and ends at e2 (inclusive).	
                    if (e2 < numh)
                    {
                        for (int e = e1; e <= e2; e++)
                        {
                            pidx.Add(hull[e].pointsIndex);
                            tridx.Add(hull[e].triadIndex);
                        }
                    }
                    else
                    {
                        for (int e = e1; e < e2; e++)
                        {
                            pidx.Add(hull[e].pointsIndex);
                            tridx.Add(hull[e].triadIndex);   // there are only n-1 triangles from n hull pts.
                        }
                        pidx.Add(hull[0].pointsIndex);
                    }

                    // erase elements e1+1 : e2-1 inclusive.
                    if (e1 < e2 - 1)
                        hull.RemoveRange(e1 + 1, e2 - e1 - 1);

                    // insert ptx at location e1+1.
                    hull.Insert(e1 + 1, ptx);
                    hidx = e1 + 1;
                }

                // If we're only computing the hull, we're done with this point
                if (hullOnly)
                    continue;

                int a = pointsIndex, T0;

                int npx = pidx.Count - 1;
                numt = triads.Count;
                T0 = numt;

                for (int p = 0; p < npx; p++)
                {
                    Triad trx = new Triad(a, pidx[p], pidx[p + 1]);
                    trx.FindCircumcirclePrecisely(points);

                    trx.bc = tridx[p];
                    if (p > 0)
                        trx.ab = numt - 1;
                    trx.ac = numt + 1;

                    // index back into the triads.
                    Triad txx = triads[tridx[p]];
                    if ((trx.b == txx.a && trx.c == txx.b) | (trx.b == txx.b && trx.c == txx.a))
                        txx.ab = numt;
                    else if ((trx.b == txx.a && trx.c == txx.c) | (trx.b == txx.c && trx.c == txx.a))
                        txx.ac = numt;
                    else if ((trx.b == txx.b && trx.c == txx.c) | (trx.b == txx.c && trx.c == txx.b))
                        txx.bc = numt;

                    triads.Add(trx);
                    numt++;
                }
                // Last edge is on the outside
                triads[numt - 1].ac = -1;

                hull[hidx].triadIndex = numt - 1;
                if (hidx > 0)
                    hull[hidx - 1].triadIndex = T0;
                else
                {
                    numh = hull.Count;
                    hull[numh - 1].triadIndex = T0;
                }
            }
        }

        /// <summary>
        /// Return the convex hull of the supplied points,
        /// Don't check for duplicate points
        /// </summary>
        /// <param name="points">List of 2D vertices</param>
        /// <returns></returns>
        public List<Vector2> ConvexHull(List<Vector2> points)
        {
            return ConvexHull(points, false);
        }
        
        /// <summary>
        /// Return the convex hull of the supplied points,
        /// Optionally check for duplicate points
        /// </summary>
        /// <param name="points">List of 2D vertices</param>
        /// <param name="rejectDuplicatePoints">Whether to omit duplicated points</param>
        /// <returns></returns>
        public List<Vector2> ConvexHull(List<Vector2> points, bool rejectDuplicatePoints)
        {
            Hull hull = new Hull();
            List<Triad> triads = new List<Triad>();

            Analyse(points, hull, triads, rejectDuplicatePoints, true);

            List<Vector2> hullVertices = new List<Vector2>();
            foreach (HullVertex hv in hull)
                hullVertices.Add(new Vector2(hv.x, hv.y));

            return hullVertices;
        }

        /// <summary>
        /// Return the Delaunay triangulation of the supplied points
        /// Don't check for duplicate points
        /// </summary>
        /// <param name="points">List of 2D vertices</param>
        /// <returns>Triads specifying the triangulation</returns>
        public List<Triad> Triangulation(List<Vector2> points)
        {
            return Triangulation(points, false);
        }

        /// <summary>
        /// Return the Delaunay triangulation of the supplied points
        /// Optionally check for duplicate points
        /// </summary>
        /// <param name="points">List of 2D vertices</param>
        /// <param name="rejectDuplicatePoints">Whether to omit duplicated points</param>
        /// <returns></returns>
        public List<Triad> Triangulation(List<Vector2> points, bool rejectDuplicatePoints)
        {
            List<Triad> triads = new List<Triad>();
            Hull hull = new Hull();

            Analyse(points, hull, triads, rejectDuplicatePoints, false);

            // Now, need to flip any pairs of adjacent triangles not satisfying
            // the Delaunay criterion
            int numt = triads.Count;
            bool[] idsA = new bool[numt];
            bool[] idsB = new bool[numt];

            // We maintain a "list" of the triangles we've flipped in order to propogate any
            // consequent changes
            // When the number of changes is large, this is best maintained as a vector of bools
            // When the number becomes small, it's best maintained as a set
            // We switch between these regimes as the number flipped decreases
            //
            // the iteration cycle limit is included to prevent degenerate cases 'oscillating'
            // and the algorithm failing to stop.
            int flipped = FlipTriangles(triads, idsA);

            int iterations = 1;
            while (flipped > (int)(fraction * (float)numt) && iterations<1000)
            {
                if ((iterations & 1) == 1)
                    flipped = FlipTriangles(triads, idsA, idsB);
                else
                    flipped = FlipTriangles(triads, idsB, idsA);

                iterations++;
            }

            Set<int> idSetA = new Set<int>(), idSetB = new Set<int>();
            flipped = FlipTriangles(triads,
                ((iterations & 1) == 1) ? idsA : idsB, idSetA);

            iterations = 1;
            while (flipped > 0 && iterations< 2000)
            {
                if ((iterations & 1) == 1)
                    flipped = FlipTriangles(triads, idSetA, idSetB);
                else
                    flipped = FlipTriangles(triads, idSetB, idSetA);

                iterations++;
            }

            return triads;
        }

        public static bool PointInPolygon(List<Vector2> pts, ref Vector2 p)
        {
            //return false as we are just a segment
            if (pts.Count <= 2)
            {
                return false;
            }

            float y = p.Y;
            float x = p.X;

            int i = 0, j = pts.Count - 1;
            bool odd = false;
            for (i = 0; i < pts.Count; ++i)
            {
                Vector2 p2 = pts[i];
                Vector2 p3 = pts[j];

                if (((p2.Y < y && p3.Y >= y) || (p3.Y < y && p2.Y >= y))
                    && (p2.X <= x || p3.X <= x))
                {
                    odd ^= (p2.X + (y - p2.Y) / (p3.Y - p2.Y) * (p3.X - p2.X) < x);
                }
                j = i;
            }

            return odd;
        }

        /// <summary>
        /// Triangulates the specified points and ensure all triangles are facing
        /// the appropriate direction order / winding
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns></returns>
        public List<int> Triangulate(List<Vector2> points, bool isShape = false)
        {
            List<int> triangles = new List<int>();
            List<Triad> triads = new List<Triad>();
           
            triads = Triangulation(points, true);
            //due to how triangle flipping is done
            //in the triangulation
            //it does not guarantee all faces
            //are in the same direction
            //thus we calculate the 2D crosses
            //to determine if the order needs
            //to be flipped or not

            foreach(Triad t in triads)
            {
                Vector2 a = points[t.a];
                Vector2 b = points[t.b];
                Vector2 c = points[t.c];

                if (!isShape)
                {
                    float ab = Vector2.Cross(ref a, ref b);
                    float bc = Vector2.Cross(ref b, ref c);
                    float ac = Vector2.Cross(ref c, ref a);

                    float sum = (ab + bc + ac) * 0.5f;

                    if (sum <= 0)
                    {
                        triangles.Add(t.a);
                        triangles.Add(t.b);
                        triangles.Add(t.c);
                    }
                    else
                    {
                        triangles.Add(t.c);
                        triangles.Add(t.b);
                        triangles.Add(t.a);
                    }
                }
                else
                {
                    float cx = (a.X * 0.333f + b.x * 0.333f + c.x * 0.333f);
                    float cy = (a.y * 0.333f + b.y * 0.333f + c.y * 0.333f);
                    Vector2 cp = new Vector2(cx, cy);

                    if(PointInPolygon(points, ref cp))
                    {
                        triangles.Add(t.a);
                        triangles.Add(t.b);
                        triangles.Add(t.c);
                    }
                }
            }

            return triangles;
        }

        public float fraction = 0.3f;

        /// <summary>
        /// Test the triad against its 3 neighbours and flip it with any neighbour whose opposite point
        /// is inside the circumcircle of the triad
        /// </summary>
        /// <param name="triads">The triads</param>
        /// <param name="triadIndexToTest">The index of the triad to test</param>
        /// <param name="triadIndexFlipped">Index of adjacent triangle it was flipped with (if any)</param>
        /// <returns>true iff the triad was flipped with any of its neighbours</returns>
        bool FlipTriangle(List<Triad> triads, int triadIndexToTest, out int triadIndexFlipped)
        {
            int oppositeVertex = 0, edge1, edge2, edge3 = 0, edge4 = 0;
            triadIndexFlipped = 0;

            Triad tri = triads[triadIndexToTest];
            // test all 3 neighbours of tri 

            if (tri.bc >= 0)
            {
                triadIndexFlipped = tri.bc;
                Triad t2 = triads[triadIndexFlipped];
                // find relative orientation (shared limb).
                t2.FindAdjacency(tri.b, triadIndexToTest, out oppositeVertex, out edge3, out edge4);
                if (tri.InsideCircumcircle(points[oppositeVertex]))
                {  // not valid in the Delaunay sense.
                    edge1 = tri.ab;
                    edge2 = tri.ac;
                    if (edge1 != edge3 && edge2 != edge4)
                    {
                        int tria = tri.a, trib = tri.b, tric = tri.c;
                        tri.Initialize(tria, trib, oppositeVertex, edge1, edge3, triadIndexFlipped, points);
                        t2.Initialize(tria, tric, oppositeVertex, edge2, edge4, triadIndexToTest, points);

                        // change knock on triangle labels.
                        if (edge3 >= 0)
                            triads[edge3].ChangeAdjacentIndex(triadIndexFlipped, triadIndexToTest);
                        if (edge2 >= 0)
                            triads[edge2].ChangeAdjacentIndex(triadIndexToTest, triadIndexFlipped);
                        return true;
                    }
                }
            }


            if (tri.ab >= 0)
            {
                triadIndexFlipped = tri.ab;
                Triad t2 = triads[triadIndexFlipped];
                // find relative orientation (shared limb).
                t2.FindAdjacency(tri.a, triadIndexToTest, out oppositeVertex, out edge3, out edge4);
                if (tri.InsideCircumcircle(points[oppositeVertex]))
                {  // not valid in the Delaunay sense.
                    edge1 = tri.ac;
                    edge2 = tri.bc;
                    if (edge1 != edge3 && edge2 != edge4)
                    {
                        int tria = tri.a, trib = tri.b, tric = tri.c;
                        tri.Initialize(tric, tria, oppositeVertex, edge1, edge3, triadIndexFlipped, points);
                        t2.Initialize(tric, trib, oppositeVertex, edge2, edge4, triadIndexToTest, points);

                        // change knock on triangle labels.
                        if (edge3 >= 0)
                            triads[edge3].ChangeAdjacentIndex(triadIndexFlipped, triadIndexToTest);
                        if (edge2 >= 0)
                            triads[edge2].ChangeAdjacentIndex(triadIndexToTest, triadIndexFlipped);
                        return true;
                    }
                }
            }

            if (tri.ac >= 0)
            {
                triadIndexFlipped = tri.ac;
                Triad t2 = triads[triadIndexFlipped];
                // find relative orientation (shared limb).
                t2.FindAdjacency(tri.a, triadIndexToTest, out oppositeVertex, out edge3, out edge4);
                if (tri.InsideCircumcircle(points[oppositeVertex]))
                {  // not valid in the Delaunay sense.
                    edge1 = tri.ab;   // .ac shared limb
                    edge2 = tri.bc;
                    if (edge1 != edge3 && edge2 != edge4)
                    {
                        int tria = tri.a, trib = tri.b, tric = tri.c;
                        tri.Initialize(trib, tria, oppositeVertex, edge1, edge3, triadIndexFlipped, points);
                        t2.Initialize(trib, tric, oppositeVertex, edge2, edge4, triadIndexToTest, points);

                        // change knock on triangle labels.
                        if (edge3 >= 0)
                            triads[edge3].ChangeAdjacentIndex(triadIndexFlipped, triadIndexToTest);
                        if (edge2 >= 0)
                            triads[edge2].ChangeAdjacentIndex(triadIndexToTest, triadIndexFlipped);
                        return true;
                    }
                }
            }

            return false;
        }
         
        /// <summary>
        /// Flip triangles that do not satisfy the Delaunay condition
        /// </summary>
        private int FlipTriangles(List<Triad> triads, bool[] idsFlipped)
        {
            int numt = (int)triads.Count;
            Array.Clear(idsFlipped, 0, numt);

            int flipped = 0;
            for (int t = 0; t < numt; t++)
            {
                int t2;
                if (FlipTriangle(triads, t, out t2))
                {
                    flipped += 2;
                    idsFlipped[t] = true;
                    idsFlipped[t2] = true;

                }
            }

            return flipped;
        }
         
        private int FlipTriangles(List<Triad> triads, bool[] idsToTest, bool[] idsFlipped)
        {
            int numt = (int)triads.Count;
            Array.Clear(idsFlipped, 0, numt);

            int flipped = 0;
            for (int t = 0; t < numt; t++)
            {
                if (idsToTest[t])
                {
                    int t2;
                    if (FlipTriangle(triads, t, out t2))
                    {
                        flipped += 2;
                        idsFlipped[t] = true;
                        idsFlipped[t2] = true;
                    }
                }
            }

            return flipped;
        }

        private int FlipTriangles(List<Triad> triads, bool[] idsToTest, Set<int> idsFlipped)
        {
            int numt = (int)triads.Count;
            idsFlipped.Clear();

            int flipped = 0;
            for (int t = 0; t < numt; t++)
            {
                if (idsToTest[t])
                {
                    int t2;
                    if (FlipTriangle(triads, t, out t2))
                    {
                        flipped += 2;
                        idsFlipped.Add(t);
                        idsFlipped.Add(t2);
                    }
                }
            }

            return flipped;
        }

        private int FlipTriangles(List<Triad> triads, Set<int> idsToTest, Set<int> idsFlipped)
        {
            int flipped = 0;
            idsFlipped.Clear();

            foreach (int t in idsToTest)
            {
                int t2;
                if (FlipTriangle(triads, t, out t2))
                {
                    flipped += 2;
                    idsFlipped.Add(t);
                    idsFlipped.Add(t2);
                }
            }

            return flipped;
        }

        #region Debug verification routines: verify that triad adjacency and indeces are set correctly
#if DEBUG
        private void VerifyHullContains(Hull hull, int idA, int idB)
        {
            if (
                ((hull[0].pointsIndex == idA) && (hull[hull.Count - 1].pointsIndex == idB)) ||
                ((hull[0].pointsIndex == idB) && (hull[hull.Count - 1].pointsIndex == idA)))
                return;

            for (int h = 0; h < hull.Count - 1; h++)
            {
                if (hull[h].pointsIndex == idA)
                {
                    Debug.Assert(hull[h + 1].pointsIndex == idB);
                    return;
                }
                else if (hull[h].pointsIndex == idB)
                {
                    Debug.Assert(hull[h + 1].pointsIndex == idA);
                    return;
                }
            }

        }

        private void VerifyTriadContains(Triad tri, int nbourTriad, int idA, int idB)
        {
            if (tri.ab == nbourTriad)
            {
                Debug.Assert(
                    ((tri.a == idA) && (tri.b == idB)) ||
                    ((tri.b == idA) && (tri.a == idB)));
            }
            else if (tri.ac == nbourTriad)
            {
                Debug.Assert(
                    ((tri.a == idA) && (tri.c == idB)) ||
                    ((tri.c == idA) && (tri.a == idB)));
            }
            else if (tri.bc == nbourTriad)
            {
                Debug.Assert(
                    ((tri.c == idA) && (tri.b == idB)) ||
                    ((tri.b == idA) && (tri.c == idB)));
            }
            else
                Debug.Assert(false);
        }

        private void VerifyTriads(List<Triad> triads, Hull hull)
        {
            for (int t = 0; t < triads.Count; t++)
            {
                if (t == 17840)
                    t = t + 0;

                Triad tri = triads[t];
                if (tri.ac == -1)
                    VerifyHullContains(hull, tri.a, tri.c);
                else
                    VerifyTriadContains(triads[tri.ac], t, tri.a, tri.c);

                if (tri.ab == -1)
                    VerifyHullContains(hull, tri.a, tri.b);
                else
                    VerifyTriadContains(triads[tri.ab], t, tri.a, tri.b);

                if (tri.bc == -1)
                    VerifyHullContains(hull, tri.b, tri.c);
                else
                    VerifyTriadContains(triads[tri.bc], t, tri.b, tri.c);

            }
        }

        private void WriteTriangles(List<Triad> triangles, string name)
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(name + ".dtt"))
            {
                writer.WriteLine(triangles.Count.ToString());
                for (int i = 0; i < triangles.Count; i++)
                {
                    Triad t = triangles[i];
                    writer.WriteLine(string.Format("{0}: {1} {2} {3} - {4} {5} {6}",
                        i + 1,
                        t.a, t.b, t.c,
                        t.ab + 1, t.bc + 1, t.ac + 1));
                }
            }
        }

#endif

        #endregion
    }

}
