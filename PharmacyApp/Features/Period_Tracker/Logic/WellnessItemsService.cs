using PharmacyApp.Common.Repositories;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PharmacyApp.Features.Period_Tracker.Logic
{
    public class WellnessItemsService : IWellnessItemsService
    {
        private const string WellnessCategoryName = "wellness";

        private readonly IItemsRepository itemRepository;

        public WellnessItemsService(IItemsRepository itemRepository)
        {
            this.itemRepository = itemRepository;
        }

        public List<Item> GetWellnessItems()
        {
            return itemRepository
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