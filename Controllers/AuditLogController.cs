using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WarehouseMS.Data;

public class AuditLogController : Controller
{
    private readonly AppDbContext _context;

    public AuditLogController(AppDbContext context)
    {
        _context = context;
    }

    // AUDIT LOG SƏHİFƏSİ (Sadece view elemek üçün)
    public async Task<IActionResult> Index()
    {
        var logs = await _context.AuditLogs
            .OrderByDescending(l => l.Id) // Ən son edilən əməliyyat ən yuxarıda
            .ToListAsync();
        return View(logs);
    }
}