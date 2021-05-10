using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Components.GL.Renderer
{
    public enum PreviewGeometryType
    {
        Cube,
        Sphere,
        CubeSphere,
        Cylinder,
        RoundedCube,
        Plane,
        Custom
    }

    public enum PreviewCameraPosition
    {
        Top,
        Bottom,
        Left,
        Right,
        Front,
        Back,
        Perspective
    }

    public enum PreviewCameraMode
    {
        Perspective,
        Orthographic
    }

    public enum PreviewRenderMode
    {
        WireframeShading,
        FullShading
    }
}
