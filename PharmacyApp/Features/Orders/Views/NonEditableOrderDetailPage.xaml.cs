using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Orders.ViewModels;

namespace PharmacyApp.Features.Orders.Views
{
    public sealed partial class NonEditableOrderDetailPage : Page
    {
        private OrderService orderService;
        private NonEditDetailViewModel ViewModel { get; set; }

        public NonEditableOrderDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var extractedArgs = (Tuple<OrderService, int>)e.Parameter;

            orderService = extractedArgs.Item1;
            int orderID = extractedArgs.Item2;
            ViewModel = new (orderService, orderID);
            DataContext = ViewModel;

            base.OnNavigatedTo(e);
        }
    }
}
