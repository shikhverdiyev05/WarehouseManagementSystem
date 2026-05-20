using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseMS.Data;
using WarehouseMS.Models;

[Authorize]
public class CategoryController : Controller
{
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context)
    {
        _context = context;
    }

    // LIST: Bütün kateqoriyaları göstərir
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories.ToListAsync();
        return View(categories);
    }

    // CREATE: Formu açır
    public IActionResult Create() => View();

    // CREATE: Formu bazaya yazır
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    // EDIT: Redaktə səhifəsini açır
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    // EDIT: Dəyişiklikləri yadda saxlayır
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    // DELETE: AJAX vasitəsilə silmə (Sürətli və müasir)
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return Json(new { success = false, message = "Tapılmadı!" });

        // Kateqoriya doludursa silməyə qoyma
        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
            return Json(new { success = false, message = "Bu kateqoriyada məhsullar var, silmək olmaz!" });

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }
}