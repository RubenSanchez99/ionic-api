using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Catalog.API.Infrastructure;
using Catalog.API.Model;
using Catalog.API.ViewModel;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Para el evento
using eShopOnContainers.Services.IntegrationEvents.Events;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Catalog.API.Services.ImageWriter;
using Newtonsoft.Json;

namespace Catalog.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly CatalogContext _catalogContext;
        private readonly CatalogSettings _settings;
        private readonly IImageHandler _imageHandler;

        public CatalogController(CatalogContext context, IOptionsSnapshot<CatalogSettings> settings, IImageHandler imageHandler)
        {
            _catalogContext = context ?? throw new ArgumentNullException(nameof(context));
            _settings = settings.Value;
            ((DbContext)context).ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _imageHandler = imageHandler;
        }

        // GET api/v1/[controller]/items[?pageSize=3&pageIndex=10]
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Items([FromQuery]int pageSize = 10, [FromQuery]int pageIndex = 0, [FromQuery] string ids = null)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                return GetItemsByIds(ids);
            }

            var totalItems = await _catalogContext.CatalogItems
                .LongCountAsync();

            var itemsOnPage = await _catalogContext.CatalogItems
                .OrderBy(c => c.Name)
                //.Skip(pageSize * pageIndex)
                //.Take(pageSize)
                .ToListAsync();

            itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

            var model = new PaginatedItemsViewModel<CatalogItem>(
                pageIndex, pageSize, totalItems, itemsOnPage);

            return Ok(model);
        }

        private IActionResult GetItemsByIds(string ids)
        {
            var numIds = ids.Split(',')
                .Select(id => (Ok: int.TryParse(id, out int x), Value: x));

            if (!numIds.All(nid => nid.Ok))
            {
                return BadRequest("ids value invalid. Must be comma-separated list of numbers");
            }

            var idsToSelect = numIds
                .Select(id => id.Value);

            var items = _catalogContext.CatalogItems.Where(ci => idsToSelect.Contains(ci.Id)).ToList();

            items = ChangeUriPlaceholder(items);

            return Ok(items);
        }

        [HttpGet]
        [Route("items/{id:int}")]
        public async Task<IActionResult> GetItemById(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var item = await _catalogContext.CatalogItems.SingleOrDefaultAsync(ci => ci.Id == id);
            if (item != null)
            {
                return Ok(item);
            }

            return NotFound();
        }

        // GET api/v1/[controller]/items/withname/samplename[?pageSize=3&pageIndex=10]
        [HttpGet]
        [Route("[action]/withname/{name:minlength(1)}")]
        public async Task<IActionResult> Items(string name, [FromQuery]int pageSize = 10, [FromQuery]int pageIndex = 0)
        {

            var totalItems = await _catalogContext.CatalogItems
                .Where(c => c.Name.StartsWith(name))
                .LongCountAsync();

            var itemsOnPage = await _catalogContext.CatalogItems
                .Where(c => c.Name.StartsWith(name))
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();

            itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

            var model = new PaginatedItemsViewModel<CatalogItem>(
                pageIndex, pageSize, totalItems, itemsOnPage);

            return Ok(model);
        }

        // GET api/v1/[controller]/items/type/1/brand/null[?pageSize=3&pageIndex=10]
        [HttpGet]
        [Route("[action]/type/{catalogTypeId}/brand/{catalogBrandId}")]
        public async Task<IActionResult> Items(int? catalogTypeId, int? catalogBrandId, [FromQuery]int pageSize = 10, [FromQuery]int pageIndex = 0)
        {
            var root = (IQueryable<CatalogItem>)_catalogContext.CatalogItems;

            if (catalogTypeId.HasValue)
            {
                root = root.Where(ci => ci.CatalogTypeId == catalogTypeId);
            }

            if (catalogBrandId.HasValue)
            {
                root = root.Where(ci => ci.CatalogBrandId == catalogBrandId);
            }

            var totalItems = await root
                .LongCountAsync();

            var itemsOnPage = await root
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();

            itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

            var model = new PaginatedItemsViewModel<CatalogItem>(
                pageIndex, pageSize, totalItems, itemsOnPage);

            return Ok(model);
        }

        // GET api/v1/[controller]/CatalogTypes
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> CatalogTypes()
        {
            var items = await _catalogContext.CatalogTypes
                .ToListAsync();

            return Ok(items);
        }

        // GET api/v1/[controller]/CatalogBrands
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> CatalogBrands()
        {
            var items = await _catalogContext.CatalogBrands
                .ToListAsync();

            return Ok(items);
        }

        //PUT api/v1/[controller]/items
        [Route("items")]
        [HttpPut]
        public async Task<IActionResult> UpdateProduct([FromBody]CatalogItem productToUpdate)
        {
            var catalogItem = await _catalogContext.CatalogItems
                .SingleOrDefaultAsync(i => i.Id == productToUpdate.Id);

            if (catalogItem == null)
            {
                return NotFound(new { Message = $"Item with id {productToUpdate.Id} not found." });
            }

            var oldPrice = catalogItem.Price;
            var raiseProductPriceChangedEvent = oldPrice != productToUpdate.Price;


            // Update current product
            catalogItem = productToUpdate;
            _catalogContext.CatalogItems.Update(catalogItem);

            await _catalogContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItemById), new { id = productToUpdate.Id }, null);
        }

        //POST api/v1/[controller]/items
        [Route("items")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct(NewItem data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var item = new CatalogItem
            {
                CatalogBrandId = data.CatalogBrandId,
                CatalogTypeId = data.CatalogTypeId,
                Description = data.Description,
                Name = data.Name,
                PictureFileName = data.PictureFileName,
                Price = data.Price
            };

            _catalogContext.CatalogItems.Add(item);

            await _catalogContext.SaveChangesAsync();

            if (data.file != null) 
            {
                await _imageHandler.UploadImage(data.file, item.PictureFileName);
            }

            return CreatedAtAction(nameof(GetItemById), new { id = item.Id }, null);
        }

        //DELETE api/v1/[controller]/id
        [Route("{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = _catalogContext.CatalogItems.SingleOrDefault(x => x.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            _catalogContext.CatalogItems.Remove(product);

            await _catalogContext.SaveChangesAsync();

            return NoContent();
        }

        private List<CatalogItem> ChangeUriPlaceholder(List<CatalogItem> items)
        {
            var baseUri = _settings.PicBaseUrl;
            Console.WriteLine("BaseUri: " + baseUri);

            foreach (var item in items)
            {
                item.FillProductUrl(baseUri);
            }

            return items;
        }
    }
}