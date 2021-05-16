using Materia.Rendering.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using tinyexrclr;

namespace Materia.Rendering.Hdr
{
    public class ExrFile : IHdrFile
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public float[] Pixels { get; protected set; }

        public GLTexture2D Texture { get; protected set; }

        string filePath;

        public ExrFile(string path)
        {
            filePath = path;
        }

        public GLTexture2D GetTexture()
        {
            if (Texture != null && Texture.Id != 0) return Texture;
            if (Pixels == null || Width == 0 || Height == 0) return null;

            GLTexture2D texture = new GLTexture2D(Interfaces.PixelInternalFormat.Rgba32f);
            texture.Bind();
            texture.SetData(Pixels, Interfaces.PixelFormat.Rgba, Width, Height);
            texture.Linear();
            texture.Repeat();
            GLTexture2D.Unbind();

            //release local memory
            Pixels = null;

            return texture;
        }

        //This is pretty slow
        public bool Load()
        {
            int w = 0;
            int h = 0;
            float[] buffer = new float[0];
            float[] pixels = null;
            if (!System.IO.File.Exists(filePath)) return false;

            unsafe {
                fixed (float* rgba = buffer)
                {
                    sbyte** err = null;
                    int result = -1;
                    byte[] fname = Encoding.UTF8.GetBytes(filePath);

                    fixed (byte* filename = fname)
                    {
                        sbyte* file = (sbyte*)filename;
                        result = TinyExr.LoadExr(&rgba, &w, &h, file, err);
                    }

                    if (result != 0)
                    {
                        if (err == null) return false;
                        string errorMessage = Marshal.PtrToStringAuto((IntPtr)err);
                        Debug.WriteLine(errorMessage);
                        TinyExr.FreeErrorMessage(*err);
                        return false;
                    }

                    pixels = new float[w * h * 4];
                    Marshal.Copy((IntPtr)rgba, pixels, 0, pixels.Length);
                    TinyExr.FreeData(rgba);
                }
            }

            Pixels = pixels;
            Width = w;
            Height = h;

            return true;
        }

        public void Dispose()
        {
            Texture?.Dispose();
            Texture = null;
        }
    }
}
