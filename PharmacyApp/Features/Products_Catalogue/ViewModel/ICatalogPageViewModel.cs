using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Products_Catalogue.Service;
using PharmacyApp.Models;

namespace PharmacyApp.Features.Products_Catalogue.ViewModels
{
    public interface ICatalogPageViewModel : INotifyPropertyChanged
    {
        // Initialization
        void Initialize(IProductCatalogueService service, User user, IOrderService orderService);

        // Events
        event EventHandler<Type> NavigateRequested;

        // Commands
        ICommand SearchCommand { get; }
        ICommand ApplyFiltersCommand { get; }
        ICommand NextPageCommand { get; }
        ICommand PreviousPageCommand { get; }
        ICommand AddToCartCommand { get; }

        // Data
        ObservableCollection<UIItem> Products { get; }
        string PageText { get; }
        string EmptyMessage { get; }
        bool IsEmptyMessageVisible { get; }
        User CurrentUser { get; }
        IOrderService OrderService { get; }
    }
}