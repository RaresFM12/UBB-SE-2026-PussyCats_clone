using System.Collections.Generic;
using PharmacyApp.Models;

namespace PharmacyApp.Features.Period_Tracker.Logic
{
    public interface IWellnessItemsService
    {
        List<Item> GetWellnessItems();
    }
}