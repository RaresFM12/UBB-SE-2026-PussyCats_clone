using System.Collections.Generic;
using PharmacyApp.Models;

namespace PharmacyApp.Features.Products_Catalogue
{
    public interface IProductCatalogueService
    {
        List<Item> GetItems(
            string search,
            List<string> categories = null,
            List<(float min, float max)> priceRanges = null,
            string stockFilter = null,
            bool? discounted = null,
            List<string> substances = null,
            bool ascending = true,
            int page = 0,
            int pageSize = ProductCatalogueService.DefaultPageSize,
            string sortBy = null);
    }
}
