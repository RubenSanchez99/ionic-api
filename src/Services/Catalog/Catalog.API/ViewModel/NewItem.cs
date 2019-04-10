using Microsoft.AspNetCore.Http;

namespace Catalog.API.ViewModel
{
    public class NewItem
    {
        public IFormFile file { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public string PictureFileName { get; set; }

        public int CatalogTypeId { get; set; }

        public int CatalogBrandId { get; set; }
    }
}