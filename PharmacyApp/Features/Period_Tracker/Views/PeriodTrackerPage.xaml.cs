using Microsoft.UI.Xaml.Controls;
using PharmacyApp.Features.Period_Tracker.ViewModels;

namespace PharmacyApp.Features.Period_Tracker.Views
{
    public sealed partial class PeriodTrackerPage : Page
    {
        public PeriodTrackerViewModel ViewModel { get; }

        public PeriodTrackerPage()
        {
            ViewModel = new PeriodTrackerViewModel();
            InitializeComponent();
        }
    }
}