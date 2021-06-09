﻿using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Geometry;
using Materia.Rendering.Buffers;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class MeshDepthProcessor : ImageProcessor
    {
        public MeshRenderer Mesh { get; set; }

        public MeshDepthProcessor() : base()
        {
            
        }

        public override void Process(GLTexture2D output)
        {
            GLTexture2D temp = output.Copy();

            if (Mesh != null)
            {
                PrepareView(temp);

                IGL.Primary.Enable((int)EnableCap.CullFace);
                IGL.Primary.CullFace((int)CullFaceMode.Back);
                IGL.Primary.Enable((int)EnableCap.DepthTest);

                IGL.Primary.PolygonMode((int)MaterialFace.FrontAndBack, (int)PolygonMode.Fill);

                MeshRenderer.SharedVao?.Bind();

                Mesh.DrawAsDepth();

                MeshRenderer.SharedVao?.Unbind();

                //restore to default depth info
                IGL.Primary.Disable((int)EnableCap.DepthTest);
                IGL.Primary.Disable((int)EnableCap.CullFace);

                FullScreenQuad.SharedVao?.Bind();
            }

            PrepareView(output);

            Identity();
            Resize(temp);
            Bind();
            SetTextures(temp);
            renderQuad?.Draw();
            temp.Dispose();
            Unbind();
        }
    }
}
