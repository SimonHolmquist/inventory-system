using Microsoft.AspNetCore.Mvc;
using Inventory.API.Data;
using Inventory.API.Models;
using Microsoft.EntityFrameworkCore;
using Inventory.API.Messaging;
using Inventory.API.DTOs;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(InventoryDbContext context, InventoryEventPublisher publisher) : ControllerBase
{
    private readonly InventoryDbContext _context = context;
    private readonly InventoryEventPublisher _publisher = publisher;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        return await _context.Products.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _context.Products.FindAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            Category = dto.Category
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        await _publisher.Publish(product, "product.created");

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateProductDto dto)
    {
        var existing = await _context.Products.FindAsync(id);
        if (existing is null)
            return NotFound();

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.Price = dto.Price;
        existing.Stock = dto.Stock;
        existing.Category = dto.Category;

        await _context.SaveChangesAsync();

        await _publisher.Publish(existing, "product.updated");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null) return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        await _publisher.Publish(product, "product.deleted");

        return NoContent();
    }
}
