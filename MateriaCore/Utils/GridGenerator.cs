using Materia.Rendering.Imaging;
using Materia.Rendering.Textures;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Utils
{
    public static class GridGenerator
    {
        public static GLTexture2D BasicGrid { get; private set; }
        public static GLTexture2D TransparentGrid { get; private set; }

        private static GLPixel DarkGray = GLPixel.FromRGBA(0.25f, 0.25f, 0.25f, 1f);
        private static GLPixel DarkerGray = GLPixel.FromRGBA(0.1f, 0.1f, 0.1f, 1f);

        private static GLPixel Gray = GLPixel.FromRGBA(0.5f, 0.5f, 0.5f, 1f);
        private static GLPixel LightGray = GLPixel.FromRGBA(0.75f, 0.75f, 0.75f, 1f);

        public static GLTexture2D CreateBasic(int w, int h)
        {
            if (BasicGrid != null) return BasicGrid;

            int whalf = w / 2 - 1;
            int hhalf = h / 2 - 1;

            RawBitmap bmp = new RawBitmap(w, h);
            bmp.Clear(DarkerGray);
            bmp.FillRect(0, 0, whalf, hhalf, DarkGray);
            bmp.FillRect(whalf + 1, 0, whalf, hhalf, DarkGray);
            bmp.FillRect(0, hhalf + 1, whalf, hhalf, DarkGray);
            bmp.FillRect(whalf + 1, hhalf + 1, whalf, hhalf, DarkGray);

            BasicGrid = new GLTexture2D(Materia.Rendering.Interfaces.PixelInternalFormat.Rgba8);
            BasicGrid.Bind();
            BasicGrid.SetData(bmp.Image, Materia.Rendering.Interfaces.PixelFormat.Bgra, w, h);
            BasicGrid.Linear();
            BasicGrid.Repeat();
            BasicGrid.GenerateMipMaps();
            GLTexture2D.Unbind();
            return BasicGrid;
        }

        public static GLTexture2D CreateTransparent(int w, int h)
        {
            if (TransparentGrid != null) return TransparentGrid;
            int whalf = w / 2;
            int hhalf = h / 2;

            RawBitmap bmp = new RawBitmap(w, h);
            bmp.FillRect(0, 0, whalf, hhalf, Gray);
            bmp.FillRect(whalf, 0, whalf, hhalf, LightGray);
            bmp.FillRect(0, hhalf, whalf, hhalf, LightGray);
            bmp.FillRect(whalf, hhalf, whalf, hhalf, Gray);

            TransparentGrid = new GLTexture2D(Materia.Rendering.Interfaces.PixelInternalFormat.Rgba8);
            TransparentGrid.Bind();
            TransparentGrid.SetData(bmp.Image, Materia.Rendering.Interfaces.PixelFormat.Bgra, w, h);
            TransparentGrid.Linear();
            TransparentGrid.Repeat();
            TransparentGrid.GenerateMipMaps();
            GLTexture2D.Unbind();
            return TransparentGrid;
        }

        public static void Dispose()
        {
            BasicGrid?.Dispose();
            BasicGrid = null;

            TransparentGrid?.Dispose();
            TransparentGrid = null;
        }
    }
}
