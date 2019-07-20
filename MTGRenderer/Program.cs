using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia;

namespace MTGRenderer
{
    class Program
    {
        static string graphPath;
        static string exportPath;
        static HiddenForm form;
        static int type;
        static TKGL tk;

        static void Main(string[] args)
        {
            ///arg[0] should be the type
            ///arg[1] should be the path to the mtg file
            ///arg[2] should be the export directory path
            ///

            //setup the proper opentk abstraction layer
            tk = new TKGL();

            if(args.Length >= 3)
            {
                type = 0;
                if (!int.TryParse(args[0], out type)) type = 0;
                graphPath = args[1];
                exportPath = args[2];

                bool process = true;

                if(!System.IO.File.Exists(graphPath))
                {
                    process = false;
                    Console.WriteLine("Graph file does not exist");
                }
                else if(!System.IO.Directory.Exists(exportPath))
                {
                    System.IO.Directory.CreateDirectory(exportPath);
                    Console.WriteLine("Export directory created");
                }

                if(process)
                {
                    form = new HiddenForm();
                    form.View.Load += View_Load;
                    form.Show();
                }
            }
            else
            {
                Console.WriteLine("Invalid parameters. Expecting: type(0-2) graphpath exportpath. Type 0: Separate, Type 1: Unity5, Type 2: Unreal4");
            }
        }

        private static void View_Load(object sender, EventArgs e)
        {
            form.Hide();
            form.Render(graphPath, exportPath, type);
            form.Close();
        }
    }
}
