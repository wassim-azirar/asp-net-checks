using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HPlusSport.API.Classes;
using HPlusSport.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HPlusSport.API.Controllers
{
    [Route("products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ShopContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ShopContext context, ILogger<ProductsController> logger) {
            try
            {
                _logger = logger;
                _context = context;

                _context.Database.EnsureCreated();
            } catch (Exception ex)
            {
                _logger.LogCritical(ex, "Fatal error in constructor");
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] ProductQueryParameters queryParameters) {
            using (_logger.BeginScope("Request ID: {requestId}", HttpContext.TraceIdentifier))
            {
                _logger.Log(LogLevel.Information,
                    "Get all products ...");

                IQueryable<Product> products = _context.Products;

                if (queryParameters.MinPrice != null &&
                    queryParameters.MaxPrice != null)
                {
                    products = products.Where(
                        p => p.Price >= queryParameters.MinPrice.Value &&
                             p.Price <= queryParameters.MaxPrice.Value);
                }

                if (!string.IsNullOrEmpty(queryParameters.SearchTerm))
                {
                    products = products.Where(p => p.Sku.ToLower().Contains(queryParameters.SearchTerm.ToLower()) ||
                                                   p.Name.ToLower().Contains(queryParameters.SearchTerm.ToLower()));
                }

                if (!string.IsNullOrEmpty(queryParameters.Sku))
                {
                    products = products.Where(p => p.Sku == queryParameters.Sku);
                }

                if (!string.IsNullOrEmpty(queryParameters.Name))
                {
                    products = products.Where(
                        p => p.Name.ToLower().Contains(queryParameters.Name.ToLower()));
                }

                if (!string.IsNullOrEmpty(queryParameters.SortBy))
                {
                    if (typeof(Product).GetProperty(queryParameters.SortBy) != null)
                    {
                        products = products.OrderByCustom(queryParameters.SortBy, queryParameters.SortOrder);
                    }
                }

                if (queryParameters.Page == 0)
                {
                    _logger.Log(LogLevel.Warning,
                        "Page is set to 0");
                }

                products = products
                    .Skip(queryParameters.Size * (Math.Max(queryParameters.Page, 1) - 1))
                    .Take(queryParameters.Size);

                _logger.Log(LogLevel.Information,
                    "Done with get all products ...");

                return Ok(await products.ToArrayAsync());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id){
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct([FromBody]Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                "GetProduct",
                new { id = product.Id },
                product
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct([FromRoute] int id, [FromBody] Product product)
        {
            if (id != product.Id) {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try 
            {
                await _context.SaveChangesAsync();
            } 
            catch (DbUpdateConcurrencyException) 
            {
                if (_context.Products.Find(id) == null) {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return product;
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> DeleteMultiple([FromQuery] int[] ids)
        {
            var products = new List<Product>();
            foreach (var id in ids)
            {
                var product = await _context.Products.FindAsync(id);

                if (product == null)
                {
                    return NotFound();
                }

                products.Add(product);
            }

            _context.Products.RemoveRange(products);
            await _context.SaveChangesAsync();

            return Ok(products);
        }
    }
}