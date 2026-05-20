using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WarehouseMS.Data;
using WarehouseMS.Models;

[Authorize]
public class StockMovementController : Controller
{
    private readonly AppDbContext _context;

    public StockMovementController(AppDbContext context)
    {
        _context = context;
    }

    // Hərəkət Tarixçəsi
    public async Task<IActionResult> Index()
    {
        var stocks = await _context.Stocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Include(s => s.Shelf) // Rəfləri mütləq Include et!
            .ToListAsync();
        return View(stocks);
    }

    // Yeni Hərəkət (Giriş/Çıxış)
    public IActionResult Create()
    {
        ViewBag.Products = new SelectList(_context.Products, "Id", "Name");
        ViewBag.Warehouses = new SelectList(_context.Warehouses, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StockMovement movement, int WarehouseId)
    {
        if (ModelState.IsValid)
        {
            // 1. Hərəkəti qeyd et
            _context.Add(movement);

            // 2. Stok cədvəlini yenilə (Və ya yarat)
            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.ProductId == movement.ProductId && s.WarehouseId == WarehouseId);

            if (stock == null)
            {
                stock = new Stock
                {
                    ProductId = movement.ProductId,
                    WarehouseId = WarehouseId,
                    Quantity = 0
                };
                _context.Stocks.Add(stock);
            }

            if (movement.Type == "In")
            {
                stock.Quantity += movement.Quantity;
            }
            else if (movement.Type == "Out")
            {
                if (stock.Quantity < movement.Quantity)
                {
                    ModelState.AddModelError("", "Anbarda kifayət qədər məhsul yoxdur!");
                    ViewBag.Products = new SelectList(_context.Products, "Id", "Name");
                    ViewBag.Warehouses = new SelectList(_context.Warehouses, "Id", "Name");
                    return View(movement);
                }
                stock.Quantity -= movement.Quantity;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(movement);
    }
}