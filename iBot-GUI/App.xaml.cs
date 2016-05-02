using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using IBot;

namespace iBot_GUI
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var t = new Thread(Program.Main);
            t.Start();

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("de-DE");
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            Program.Shutdown();
            Environment.Exit(0);
        }
    }
}