using DnDesktop.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DnDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            try
            {
                // SQLitePCL.Batteries_V2.Init();
                DatabaseInitializer.Initialize();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    "The application database could not be initialized.\n\n"
                    + ex.Message,
                    "Database Initialization Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);

                Shutdown(1);
                return;
            }

            base.OnStartup(e);
        }
    }
}
