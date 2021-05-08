using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Materia.Rendering.Geometry
{
    public class Line
    {
        public Vector3 Start = Vector3.Zero;
        public Vector3 End = Vector3.Zero;
        public Vector4 StartColor = new Vector4(1,1,1,1);
        public Vector4 EndColor = new Vector4(1,1,1,1);

        public Line() { }
        public Line(Vector3 start, Vector3 end)
        {
            Start = start;
            End = end;
        }

        public float[] Compact()
        {
            List<float> data = new List<float>();

            data.Add(Start.X);
            data.Add(Start.Y);
            data.Add(Start.Z);
            data.Add(StartColor.X);
            data.Add(StartColor.Y);
            data.Add(StartColor.Z);
            data.Add(StartColor.W);
            data.Add(End.X);
            data.Add(End.Y);
            data.Add(End.Z);
            data.Add(EndColor.X);
            data.Add(EndColor.Y);
            data.Add(EndColor.Z);
            data.Add(EndColor.W);

            return data.ToArray();
        }
    }
}
