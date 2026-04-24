using System;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel.Activation;
using PharmacyApp.Features.Accounts.Logic;

namespace PharmacyApp
{
    public partial class App : Application
    {
        private Window? window;

        public App()
        {
            InitializeComponent();
        }

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