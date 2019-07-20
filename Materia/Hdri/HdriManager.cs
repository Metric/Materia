using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Materia.Textures;
using DDSReader;
using Materia.GLInterfaces;
using NLog;

namespace Materia.Hdri
{
    public class HdriManager
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();
        
        public delegate void HdriLoaded(GLTextuer2D irradiance, GLTextuer2D prefiltered);

        public static event HdriLoaded OnHdriLoaded;

        public static GLTextuer2D Irradiance { get; protected set; }
        public static GLTextuer2D Prefiltered { get; protected set; }

        protected static string selected;
        public static string Selected
        {
            get
            {
                return selected;
            }
            set
            {
                int idx = available.IndexOf(value);

                if (idx > -1)
                {
                    selected = value;
                    SelectedIndex = idx;
                }
            }
        }

        protected static int selectedIndex = -1;
        public static int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                selectedIndex = value;
                Task.Run(async () =>
                {
                    await Load();
                });
            }
        }

        protected static List<string> available = new List<string>();
        public static List<string> Available
        {
            get
            {
                return available.ToList();
            }
        }

        public static void Scan()
        {
            available.Clear();

            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hdri");

            string[] list = Directory.GetDirectories(dir);

            for(int i = 0; i < list.Length; i++)
            {
                string f = list[i];
                if (Directory.Exists(f))
                {
                    string[] files = Directory.GetFiles(f);

                    int fileCount = 0;

                    foreach (string h in files)
                    {
                        if (h.Contains("prefiltered.dds") || h.Contains("irradiance.dds"))
                        {
                            fileCount++;
                        }
                    }

                    if (fileCount >= 2)
                    {
                        available.Add(Path.GetFileNameWithoutExtension(f));
                    }
                }
            }

            if(selectedIndex == -1 && available.Count > 0)
            {
                selected = available[0];
                selectedIndex = 0;
            }
        }

        public static void Release()
        {
            if(Irradiance != null)
            {
                Irradiance.Release();
                Irradiance = null;
            }

            if(Prefiltered != null)
            {
                Prefiltered.Release();
                Prefiltered = null;
            }
        }

        public static async Task Load()
        {
            int index = selectedIndex;
            if (index < 0) index = 0;
            if (index > available.Count - 1) index = available.Count - 1;

            if(index >= 0 && index < available.Count)
            {
                string f = available[index];

                string iradpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hdri", f, "irradiance.dds");
                string prefpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hdri", f, "prefiltered.dds");

                try
                {
                    DDSImage rad = await DDSReader.DDSReader.ReadImageAsync(iradpath);
                    DDSImage pre = await DDSReader.DDSReader.ReadImageAsync(prefpath);

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Collection<DDSMipMap> mips = (Collection<DDSMipMap>)rad.Frames;

                        if (mips.Count > 0)
                        {

                            var mip = mips[0];
                            byte[] data = mip.MipmapData[0];
                            if (Irradiance != null)
                            {
                                Irradiance.Release();
                            }

                            Irradiance = new GLTextuer2D(PixelInternalFormat.Rgb8);
                            Irradiance.Bind();
                            Irradiance.SetData(data, PixelFormat.Rgb, (int)mip.Width, (int)mip.Height);
                            Irradiance.SetFilter((int)TextureMinFilter.Linear, (int)TextureMagFilter.Linear);
                            Irradiance.SetWrap((int)TextureWrapMode.ClampToEdge);
                            GLTextuer2D.Unbind();
                        }

                        mips = (Collection<DDSMipMap>)pre.Frames;

                        if (mips.Count > 0)
                        {
                            if (Prefiltered != null)
                            {
                                Prefiltered.Release();
                            }

                            Prefiltered = new GLTextuer2D(PixelInternalFormat.Rgb8);
                            Prefiltered.Bind();
                            Prefiltered.SetMaxMipLevel(4);

                            for (int i = 0; i < mips.Count; i++)
                            {
                                var mip = mips[i];
                                byte[] data = mip.MipmapData[0];

                                Prefiltered.SetData(data, PixelFormat.Rgb, (int)mip.Width, (int)mip.Height, i);
                            }

                            Prefiltered.SetFilter((int)TextureMinFilter.LinearMipmapLinear, (int)TextureMagFilter.Linear);
                            Prefiltered.SetWrap((int)TextureWrapMode.ClampToEdge);

                            GLTextuer2D.Unbind();
                        }

                        if(OnHdriLoaded != null)
                        {
                            OnHdriLoaded.Invoke(Irradiance, Prefiltered);
                        }
                    });
                }
                catch (Exception e)
                {
                    Log.Error(e);   
                }
            }
        }
    }
}
