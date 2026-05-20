using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WarehouseMS.Data;
using WarehouseMS.Models;

namespace WarehouseMS.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // 1. MƏHSUL SİYAHISI (Index)
        public async Task<IActionResult> Index()
        {
            // Məhsulu çəkirik, kateqoriyasını və daxilindəki qalıq siyahısını mütləq LEFT JOIN ilə yükləyirik
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stocks) // Bura mütləq gəlməlidir
                .ToListAsync();
            return View(products);
        }

        // 2. MƏHSUL DETALLARI (Genişləndirilmiş)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stocks)
                    .ThenInclude(s => s.Warehouse)
                .Include(p => p.Stocks)
                    .ThenInclude(s => s.Shelf)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // 3. YENİ MƏHSUL YARATMA (GET)
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // 4. YENİ MƏHSUL YARATMA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            // Validasiyada Stocks kolleksiyasının boş olmasına görə xəta verməməsi üçün:
            ModelState.Remove("Category");
            ModelState.Remove("Stocks");

            if (string.IsNullOrEmpty(product.SKU))
            {
                product.SKU = "PROD-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            }

            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // 5. MƏHSUL REDAKTƏ ETMƏ (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // 6. MƏHSUL REDAKTƏ ETMƏ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CategoryId,Price,Barcode")] Product product)
        {
            if (id != product.Id) return NotFound();

            ModelState.Remove("Category");
            ModelState.Remove("Stocks");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // 7. MƏHSUL SİLMƏ (AJAX)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return Json(new { success = false, message = "Məhsul tapılmadı!" });

            // Təhlükəsizlik: Əgər məhsulun stokda hərəkəti və ya qalığı varsa silməyə qoyma
            bool hasStock = await _context.Stocks.AnyAsync(s => s.ProductId == id);

            if (hasStock)
            {
                return Json(new
                {
                    success = false,
                    message = "Bu məhsulu silmək olmaz! Çünki bazada stok qalığı mövcuddur. Əvvəlcə stoku sıfırlayın."
                });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<JsonResult> GetProductByBarcode(string barcode)
        {
            if (string.IsNullOrEmpty(barcode)) return Json(null);

            var product = await _context.Products
                .Where(p => p.Barcode == barcode)
                .Select(p => new { name = p.Name, price = p.SalePrice })
                .FirstOrDefaultAsync();

            return Json(product);
        }
    }
}