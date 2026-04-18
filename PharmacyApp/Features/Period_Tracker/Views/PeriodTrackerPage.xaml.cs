using Windows.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PharmacyApp.Features.Period_Tracker.ViewModels;
using Syncfusion.UI.Xaml.Core;

namespace PharmacyApp.Features.Period_Tracker.Views
{
    public sealed partial class PeriodTrackerPage : Page
    {
        public PeriodTrackerViewModel ViewModel { get; } = new PeriodTrackerViewModel();

        public PeriodTrackerPage()
        {
            InitializeComponent();
        }

        private void OnCalculateCycleClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.CalculatePeriodTracker(
                StartPeriodDatePicker.Date,
                CycleDaysNumberBox.Value,
                PeriodLastsNumberBox.Value,
                PMSRadioButtons.SelectedIndex);

            ViewModel.CalendarsVisibility = "Visible";
            ViewModel.ShopVisibility = "Visible";
        }

        private void OnNextCycleMonthClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.UpdatePeriodTracker(true);
        }

        private void OnPreviousCycleMonthClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.UpdatePeriodTracker(false);
        }

        private void OnRemoveNoteClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoveNote((NoteViewModel)((Button)sender).DataContext);
        }

        private void OnAddNoteClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.AddNewNote();
        }

        private void OnNoteIsDoneChecked(object sender, RoutedEventArgs e)
        {
            StackPanel parent = (StackPanel)((CheckBox)sender).Parent;
            ((TextBox)parent.FindChildByName("NoteBodyTextBox")).FontStyle = FontStyle.Italic;
        }

        private void OnNoteIsDoneUnchecked(object sender, RoutedEventArgs e)
        {
            StackPanel parent = (StackPanel)((CheckBox)sender).Parent;
            ((TextBox)parent.FindChildByName("NoteBodyTextBox")).FontStyle = FontStyle.Normal;
        }
    }
}