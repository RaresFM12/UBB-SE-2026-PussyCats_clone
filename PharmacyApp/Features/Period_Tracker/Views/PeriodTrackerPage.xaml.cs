using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PharmacyApp.Features.Period_Tracker.ViewModels;

namespace PharmacyApp.Features.Period_Tracker.Views
{
    public sealed partial class PeriodTrackerPage : Page
    {
        public PeriodTrackerViewModel ViewModel { get; private set; }

        public PeriodTrackerPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArguments)
        {
            base.OnNavigatedTo(navigationEventArguments);

            if (navigationEventArguments.Parameter is PeriodTrackerViewModel periodTrackerViewModel)
            {
                ViewModel = periodTrackerViewModel;
                DataContext = ViewModel;
                Bindings.Update();
            }
        }
    }
}