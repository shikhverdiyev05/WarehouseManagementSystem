using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using WarehouseMS.Data;

[Authorize]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // 1. Dashboard-un yuxarı kartları üçün statistikaları doldururuq
        ViewBag.TotalProducts = await _context.Products.CountAsync();
        ViewBag.TotalCategories = await _context.Categories.CountAsync();

        // Kritik stok miqdarı 10-dan aşağı olan fərqli məhsulların sayı
        ViewBag.LowStockCount = await _context.Stocks
            .Where(s => s.Quantity < 10)
            .Select(s => s.ProductId)
            .Distinct()
            .CountAsync();

        // 🌟 ƏSAS HİSSƏ: View-un gözlədiyi kimi Stock siyahısını göndəririk (StockMovement YOX!)
        // Son əlavə olunan və ya yenilənən 5 stoku gətiririk
        // Dashboard Controller üçün Düzəliş:
        var latestStocks = await _context.Stocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Include(s => s.Shelf)
            .OrderByDescending(s => s.Id)
            .ToListAsync();

        return View(latestStocks);
    }
}