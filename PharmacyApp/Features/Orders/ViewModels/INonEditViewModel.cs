using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Features.Orders.ViewModels
{
    public interface INonEditViewModel
    {
        List<ItemDetail> OrderItems { get; }
        string TotalPriceString { get; }
        string StatusString { get; }
        DateOnly PickUpDate { get; }
        string PickUpDateString { get; }
    }
}
