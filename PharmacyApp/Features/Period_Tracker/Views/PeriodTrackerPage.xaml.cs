using Microsoft.UI.Xaml.Controls;
using PharmacyApp.Features.Period_Tracker.Logic;
using PharmacyApp.Features.Period_Tracker.ViewModels;

namespace PharmacyApp.Features.Period_Tracker.Views
{
    public sealed partial class PeriodTrackerPage : Page
    {
        public PeriodTrackerViewModel ViewModel { get; }

        public PeriodTrackerPage()
        {
            var serviceFactory = new PeriodTrackerServiceFactory();
            ViewModel = new PeriodTrackerViewModel(serviceFactory);
            InitializeComponent();
        }
    }
}
