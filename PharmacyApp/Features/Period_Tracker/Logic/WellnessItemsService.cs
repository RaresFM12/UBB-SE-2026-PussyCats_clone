using PharmacyApp.Common.Repositories;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PharmacyApp.Features.Period_Tracker.Logic
{
    public class WellnessItemsService : IWellnessItemsService
    {
        private readonly IItemsRepository itemsRepository;

        public WellnessItemsService()
            : this(new SQLItemsRepository())
        {
        }

        public WellnessItemsService(IItemsRepository itemsRepository)
        {
            this.itemsRepository = itemsRepository;
        }

        public List<Item> GetWellnessItems()
        {
            return itemsRepository.GetAllItems()
                .Where(item => item.Category != null &&
                               item.Category.Equals("wellness", StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.Id)
                .ToList();
        }
    }
}