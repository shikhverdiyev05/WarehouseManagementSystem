using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseMS.Data;
using WarehouseMS.Models;

[Authorize]
public class WarehouseController : Controller
{
    private readonly AppDbContext _context;

    public WarehouseController(AppDbContext context)
    {
        _context = context;
    }

    // 1. ANBAR SİYAHISI
    public async Task<IActionResult> Index()
    {
        var warehouses = await _context.Warehouses
            .Include(w => w.Shelves)
            .ToListAsync();
        return View(warehouses);
    }

    // 2. YENİ ANBAR YARATMA (GET)
    public IActionResult Create() => View();

    // 3. YENİ ANBAR YARATMA (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Warehouse warehouse)
    {
        // Model validasiyasında Shelves kolleksiyasının boş olmasına görə xəta verməməsi üçün:
        ModelState.Remove("Shelves");

        if (ModelState.IsValid)
        {
            _context.Add(warehouse);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(warehouse);
    }

    // 4. ANBAR REDAKTƏ ETMƏ (GET)
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var warehouse = await _context.Warehouses.FindAsync(id);
        if (warehouse == null) return NotFound();
        return View(warehouse);
    }

    // 5. ANBAR REDAKTƏ ETMƏ (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Warehouse warehouse)
    {
        if (id != warehouse.Id) return NotFound();
        ModelState.Remove("Shelves");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(warehouse);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Warehouses.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(warehouse);
    }

    // 6. ANBAR DETALLARI (Rəflər və İçindəki Məhsullar)
    public async Task<IActionResult> Details(int id)
    {
        var warehouse = await _context.Warehouses
            .Include(w => w.Shelves)
                .ThenInclude(s => s.Stocks!) // ! işarəsi nullability xəbərdarlığını silir
                    .ThenInclude(st => st.Product)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (warehouse == null) return NotFound();

        return View(warehouse);
    }

    // 7. ANBARI SİLMƏK (AJAX)
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var warehouse = await _context.Warehouses
            .Include(w => w.Shelves)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (warehouse == null) return Json(new { success = false, message = "Anbar tapılmadı." });

        // Təhlükəsizlik: Anbarda rəf və ya stok varsa silmə
        var hasStock = await _context.Stocks.AnyAsync(s => s.WarehouseId == id);
        if (warehouse.Shelves.Any() || hasStock)
        {
            return Json(new { success = false, message = "Anbar boş deyil! Silmək üçün əvvəlcə rəfləri və stoku təmizləyin." });
        }

        _context.Warehouses.Remove(warehouse);
        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }

    // 8. YENİ RƏF ƏLAVƏ ETMƏK (AJAX - Details səhifəsindən)
    [HttpPost]
    public async Task<IActionResult> CreateShelf(int warehouseId, string shelfCode)
    {
        if (string.IsNullOrEmpty(shelfCode))
            return Json(new { success = false, message = "Rəf kodu boş ola bilməz!" });

        var shelf = new Shelf { WarehouseId = warehouseId, ShelfCode = shelfCode };
        _context.Shelves.Add(shelf);
        await _context.SaveChangesAsync();

        return Json(new { success = true, code = shelf.ShelfCode });
    }

    // POST: Warehouse/EditShelf (AJAX vasitəsilə rəf kodunu dəyişmək)
    [HttpPost]
    public async Task<IActionResult> EditShelf(int id, string newCode)
    {
        var shelf = await _context.Shelves.FindAsync(id);
        if (shelf == null || string.IsNullOrEmpty(newCode))
            return Json(new { success = false, message = "Rəf tapılmadı və ya kod boşdur." });

        shelf.ShelfCode = newCode;
        _context.Update(shelf);
        await _context.SaveChangesAsync();

        return Json(new { success = true, code = newCode });
    }

    // WarehouseController.cs içindəki DeleteShelf hissəsi
    [HttpPost("/Warehouse/DeleteShelf/{id}")] // Route-u dəqiqləşdiririk
    public async Task<IActionResult> DeleteShelf(int id)
    {
        var shelf = await _context.Shelves.FindAsync(id);
        if (shelf == null) return Json(new { success = false, message = "Rəf tapılmadı." });

        var hasStock = await _context.Stocks.AnyAsync(s => s.ShelfId == id);
        if (hasStock)
        {
            return Json(new { success = false, message = "Bu rəf doludur! İçində məhsul olan rəfi silmək olmaz." });
        }

        _context.Shelves.Remove(shelf);
        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }
}