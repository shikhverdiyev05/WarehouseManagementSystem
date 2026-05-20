using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WarehouseMS.Data;
using WarehouseMS.Models;
using Microsoft.AspNetCore.Http;

namespace WarehouseMS.Controllers
{
    public class SaleController : Controller
    {
        private readonly AppDbContext _context;

        public SaleController(AppDbContext context)
        {
            _context = context;
        }

        // SATIŞ TARİXÇƏSİ
        public async Task<IActionResult> Index()
        {
            var sales = await _context.Sales
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .OrderByDescending(s => s.Date)
                .ToListAsync();
            return View(sales);
        }

        // YENİ SATIŞ (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // BARKOD İLƏ SATIŞ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string barcode, int quantity, decimal price, string loggedUser)
        {
            if (string.IsNullOrEmpty(barcode) || quantity <= 0 || price <= 0)
            {
                ModelState.AddModelError("", "Zəhmət olmasa bütün sahələri düzgün doldurun.");
                return View();
            }

            // 1. Məhsulu tapırıq
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Barcode == barcode);
            if (product == null)
            {
                ModelState.AddModelError("", "Bu barkoda uyğun məhsul tapılmadı!");
                return View();
            }

            // 2. Stoku tapırıq
            var currentStock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.ProductId == product.Id && s.Quantity >= quantity);

            if (currentStock == null)
            {
                var totalAvailable = await _context.Stocks
                    .Where(s => s.ProductId == product.Id)
                    .SumAsync(s => s.Quantity);

                ModelState.AddModelError("", $"Kifayət qədər məhsul yoxdur! (Ümumi qalıq: {totalAvailable})");
                return View();
            }

            // 3. Stokdan miqdarı çıxırıq
            currentStock.Quantity -= quantity;
            _context.Update(currentStock);

            // 4. Hərəkət Logu
            var movement = new StockMovement
            {
                ProductId = product.Id,
                Type = "Out",
                Quantity = quantity,
                Date = DateTime.Now
            };
            _context.StockMovements.Add(movement);

            // 5. Satış Faktını Qeydə Alırıq (Müştərisiz və rəfsiz - tam təmiz)
            var sale = new Sale
            {
                ProductId = product.Id,
                WarehouseId = currentStock.WarehouseId,
                Quantity = quantity,
                Price = price,
                Date = DateTime.Now
            };
            _context.Add(sale);

            // 6. AUDIT LOG (Səssiz və təhlükəsiz yoxlama)
            string userFullName = loggedUser ?? HttpContext.Session.GetString("UserFullName") ?? "Anbar Meneceri";
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            var auditLog = new AuditLog
            {
                UserId = userFullName,
                Action = "Satış",
                TableName = "Sales",
                OldValue = "Yeni Satış Əməliyyatı",
                NewValue = $"Barkod: {barcode} | Məhsul: {product.Name} | Miqdar: {quantity} | Qiymət: {price} AZN",
                IpAddress = ipAddress
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}