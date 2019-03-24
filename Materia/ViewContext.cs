using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using OpenTK;

namespace Materia
{
    public static class ViewContext
    {
        public static GraphicsContext Context { get; set; }

        public static void VerifyContext(GLControl control)
        {
            if(Context == null)
            {
                Context = new GraphicsContext(GraphicsMode.Default, control.WindowInfo);
            }
        }

        public static void Dispose()
        {
            if(Context != null)
            {
                Context.Dispose();
                Context = null;
            }
        }
    }
}
