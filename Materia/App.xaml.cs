using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;
using Materia.Material;
using Materia.UI;
using Materia.Imaging.GLProcessing;

namespace Materia
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected static ILogger Log = LogManager.GetCurrentClassLogger();

        protected override void OnStartup(StartupEventArgs e)
        {
            HandleFileAssociation();
            InitNLog();
            base.OnStartup(e);
        }

        private void HandleFileAssociation()
        {
            if (AppDomain.CurrentDomain.SetupInformation.ActivationArguments != null && AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData != null
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
        }

        private void InitNLog()
        {
            ExtendNLog();
            LogManager.ConfigurationReloaded += LogManager_ConfigurationReloaded;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Application.Current.DispatcherUnhandledException += DispatcherOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }

        private void LogManager_ConfigurationReloaded(object sender, LoggingConfigurationReloadedEventArgs e)
        {
            ExtendNLog();
        }

        private void ExtendNLog()
        {
            LogManager.Configuration.AddTarget("mlog", new Logging.MateriaLogTarget());
            LogManager.Configuration.AddRuleForAllLevels("mlog");
            LogManager.ReconfigExistingLoggers();

            Log.Info("nlog reconfigured");
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
        {
            Log.Error(unobservedTaskExceptionEventArgs.Exception);
        }

        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
        {
            Log.Error(dispatcherUnhandledExceptionEventArgs.Exception);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            //only here for crash cleanup
            CleanUp();

            Log.Error((Exception)unhandledExceptionEventArgs.ExceptionObject);
        }

        private static void CleanUp()
        {
            if(MateriaMainWindow.Instance != null)
            {
                MateriaMainWindow.Instance.CleanUp(null, true);
            }

            //clear material and shader caches
            PBRMaterial.ReleaseBRDF();
            ImageProcessor.ReleaseAll();
            Material.Material.ReleaseAll();

            //release gl view
            if (UI3DPreview.Instance != null)
            {
                UI3DPreview.Instance.Release();
            }

            if (UIPreviewPane.Instance != null)
            {
                UIPreviewPane.Instance.Release();
            }

            ViewContext.Dispose();
        }
    }
}
