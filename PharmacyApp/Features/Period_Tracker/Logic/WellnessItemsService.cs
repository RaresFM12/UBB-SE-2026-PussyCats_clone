using System;
using System.Collections.Generic;
using System.Linq;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Models;

namespace PharmacyApp.Features.Period_Tracker.Logic
{
    public class WellnessItemsService : IWellnessItemsService
    {
        private const string WellnessCategoryName = "wellness";

        private readonly IItemsRepository itemsRepository;

        public WellnessItemsService(IItemsRepository itemsRepository)
        {
            this.itemsRepository = itemsRepository;
        }

        public List<Item> GetWellnessItems()
        {
            return itemsRepository
                .GetAllItems()
                .Where(IsWellnessItem)
                .OrderBy(item => item.Id)
                .ToList();
        }

        private static bool IsWellnessItem(Item item)
        {
            return item.Category != null &&
                   item.Category.Equals(WellnessCategoryName, StringComparison.OrdinalIgnoreCase);
        }
    }
}