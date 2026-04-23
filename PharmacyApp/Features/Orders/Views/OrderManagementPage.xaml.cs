using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Orders.ViewModels;

namespace PharmacyApp.Features.Orders.Views;
public sealed partial class OrderManagementPage : Page
{
    private OrderService orderService;
    public OrderManagementViewModel ViewModel { get; set; }

    public OrderManagementPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        orderService = (OrderService)e.Parameter;
        ViewModel = new (orderService);
        DataContext = ViewModel;

        ViewModel.ClickDetailButton += RedirectToPage;
    }

    private void RedirectToPage(Tuple<OrderService, OrderDetail> args)
    {
        bool completeStatus = args.Item2.IsComplete;
        bool expiredStatus = args.Item2.IsExpired;

        if (!completeStatus && !expiredStatus)
        {
            Frame.Navigate(typeof(PharmacyApp.Features.Orders.Views.EditableOrderDetailPage),
                    new Tuple<OrderService, int>(args.Item1, args.Item2.OrderID));
        }
        else
        {
            Frame.Navigate(typeof(PharmacyApp.Features.Orders.Views.NonEditableOrderDetailPage),
                    new Tuple<OrderService, int>(args.Item1, args.Item2.OrderID));
        }
    }
}

public class OrderDetailTemplateSelector : DataTemplateSelector
{
    public DataTemplate IncompleteTemplate { get; set; }
    public DataTemplate ExpiredTemplate { get; set; }
    public DataTemplate CompleteTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        OrderDetail currentOrder = (OrderDetail)item;

        if (currentOrder.IsComplete)
        {
            return CompleteTemplate;
        }
        if (currentOrder.IsExpired)
        {
            return ExpiredTemplate;
        }

        return IncompleteTemplate;
    }
}