using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WarehouseMS.Data;
using WarehouseMS.Models;

public class StockController : Controller
{
    private readonly AppDbContext _context;

    public StockController(AppDbContext context)
    {
        _context = context;
    }

    // MÖVCUD STOK SƏHİFƏSİ
    public async Task<IActionResult> Index()
    {
        // Bütün stok qeydlərini, məhsulu, anbarı və rəfi tam şəkildə yükləyirik
        var stocks = await _context.Stocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Include(s => s.Shelf)
            .ToListAsync();

        return View(stocks);
    }

    // YENİ STOK DAXİL ET (GET: /Stock/Create)
    public IActionResult Create()
    {
        ViewBag.Products = new SelectList(_context.Products, "Id", "Name");
        ViewBag.Warehouses = new SelectList(_context.Warehouses, "Id", "Name");
        return View();
    }

    // YENİ STOK DAXİL ET (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Stock stock)
    {
        if (ModelState.IsValid)
        {
            stock.Type = "In"; // Stok daxil etmək mədaxildir

            // Eyni məhsul, anbar və rəfdə köhnə stok varmı?
            var existingStock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.ProductId == stock.ProductId &&
                                         s.WarehouseId == stock.WarehouseId &&
                                         s.ShelfId == stock.ShelfId);

            if (existingStock != null)
                existingStock.Quantity += stock.Quantity;
            else
                _context.Add(stock);

            // Tarixçəyə (Movements) yazırıq
            var movement = new StockMovement
            {
                ProductId = stock.ProductId,
                Type = "In",
                Quantity = stock.Quantity,
                Date = DateTime.Now
            };
            _context.StockMovements.Add(movement);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); // Birbaşa Mövcud Stok cədvəlinə qayıdır
        }

        ViewBag.Products = new SelectList(_context.Products, "Id", "Name", stock.ProductId);
        ViewBag.Warehouses = new SelectList(_context.Warehouses, "Id", "Name", stock.WarehouseId);
        return View(stock);
    }


    // ANBAR SEÇİLƏNDƏ RƏFLƏRİ GƏTİRMƏK (AJAX)
    [HttpGet]
    public async Task<JsonResult> GetShelves(int warehouseId)
    {
        var shelves = await _context.Shelves
            .Where(s => s.WarehouseId == warehouseId)
            .Select(s => new { id = s.Id, code = s.ShelfCode })
            .ToListAsync();
        return Json(shelves);
    }

    

    // STOK REDAKTƏSİ (GET)
    public async Task<IActionResult> Edit(int id)
    {
        var stock = await _context.Stocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Include(s => s.Shelf)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (stock == null) return NotFound();

        return View(stock);
    }

    // STOK REDAKTƏSİ (POST) - Düzəliş/Çıxış Logu Əlavə Edildi
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Stock stock)
    {
        var originalStock = await _context.Stocks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == stock.Id);
        if (originalStock == null) return NotFound();

        var s = await _context.Stocks.FindAsync(stock.Id);
        if (s == null) return NotFound();

        // Fərqi tapırıq ki, artım yoxsa azalma olduğunu loga yazaq
        int difference = stock.Quantity - originalStock.Quantity;

        s.Quantity = stock.Quantity;
        _context.Update(s);

        if (difference != 0)
        {
            // 🌟 HƏRƏKƏT TARİXÇƏSİNƏ LOG YAZILIR
            var movement = new StockMovement
            {
                ProductId = s.ProductId,
                Type = difference > 0 ? "In" : "Out", // Müsbətdirsə giriş, mənfidirsə çıxış
                Quantity = Math.Abs(difference),      // Həmişə müsbət rəqəm yazılır
                Date = DateTime.Now
            };
            _context.StockMovements.Add(movement);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Details", "Product", new { id = s.ProductId });
    }

    // STOK HƏRƏKƏTLƏRİ SƏHİFƏSİ
    public async Task<IActionResult> Movements()
    {
        var movements = await _context.StockMovements
            .Include(m => m.Product)
            .OrderByDescending(m => m.Date)
            .ToListAsync();

        return View(movements);
    }
}