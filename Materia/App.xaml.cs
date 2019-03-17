using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Materia
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if(AppDomain.CurrentDomain.SetupInformation.ActivationArguments != null && AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData != null
                && AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData.Length > 0)
            {
                string fname = null;

                try
                {
                    fname = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData[0];

                    Uri uri = new Uri(fname);
                    fname = uri.LocalPath;

                    this.Properties["OpenFile"] = fname;
                }
                catch
                {
                    this.Properties["OpenFile"] = null;
                }
            }

            base.OnStartup(e);
        }
    }
}
