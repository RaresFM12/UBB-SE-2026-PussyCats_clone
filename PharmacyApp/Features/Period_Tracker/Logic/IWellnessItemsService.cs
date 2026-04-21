using PharmacyApp.Models;
using System.Collections.Generic;

namespace PharmacyApp.Features.Period_Tracker.Logic
{
    public interface IWellnessItemsService
    {
        List<Item> GetWellnessItems();
    }
}