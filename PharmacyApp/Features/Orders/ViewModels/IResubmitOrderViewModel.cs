using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Features.Orders.ViewModels
{
    public interface IResubmitOrderViewModel
    {
        int ShownOrderID { get; set; }
        List<ItemDetail> OrderItems { get; }
        string TotalPriceString { get; }
    }
}