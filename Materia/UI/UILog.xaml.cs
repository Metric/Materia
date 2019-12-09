using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Materia.Logging;
using Materia.Nodes;

namespace Materia.UI
{
    /// <summary>
    /// Interaction logic for UILog.xaml
    /// </summary>
    public partial class UILog : UserControl
    {
        public static int MaxBlockCount = 1024;
        MateriaLogLevel target;

        public UILog()
        {
            InitializeComponent();
            target = MateriaLogLevel.All;
            
            MateriaLogTarget.OnLogWrite += MateriaLogTarget_OnLogWrite;
        }

        private void MateriaLogTarget_OnLogWrite(MateriaLogLevel level, string msg)
        {
            if((target & level) != 0 || target == MateriaLogLevel.All)
            {
                var p = new Paragraph(new Run(msg));

                switch(level)
                {
                    case MateriaLogLevel.Error:
                        p.Foreground = new SolidColorBrush(Colors.Red);
                        break;
                    case MateriaLogLevel.Info:
                        p.Foreground = new SolidColorBrush(Colors.LightBlue);
                        break;
                    case MateriaLogLevel.Warn:
                        p.Foreground = new SolidColorBrush(Colors.Yellow);
                        break;
                    default:
                        break;
                }

                Log.Document.Blocks.Add(p);

                if(Log.Document.Blocks.Count > MaxBlockCount)
                {
                    var blocks = Log.Document.Blocks.ToList();
                    blocks.RemoveRange(0, 256);
                    Log.Document.Blocks.Clear();
                    Log.Document.Blocks.AddRange(blocks);
                }

                Log.ScrollToEnd();
            }
        }

        private void UpdateTargetLevel()
        {
            target = MateriaLogLevel.All;

            if(chkInfo.IsChecked == true)
            {
                target |= MateriaLogLevel.Info;
            }
            if(chkDebug.IsChecked == true)
            {
                target |= MateriaLogLevel.Debug;
            }
            if(chkError.IsChecked == true)
            {
                target |= MateriaLogLevel.Error;
            }

            target |= MateriaLogLevel.Warn;
        }

        private void ChkInfo_Click(object sender, RoutedEventArgs e)
        {
            UpdateTargetLevel();

            //handle separate case for
            //shader specific debug logging
            if (chkShader.IsChecked == true)
            {
                Graph.ShaderLogging = true;
            }
            else
            {
                Graph.ShaderLogging = false;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateTargetLevel();
        }
    }
}
