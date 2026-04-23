using System;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel.Activation;
using PharmacyApp.Features.Accounts.Logic;

namespace PharmacyApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? window;

        /// <summary>
        /// Initializes the singleton application object.
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                ServiceWrapper.Initialize();

                window = new MainWindow();
                window.Activate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("App startup crash:");
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}