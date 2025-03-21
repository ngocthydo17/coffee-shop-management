using Manage_Coffee.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Manage_Coffee.ViewComponents
{
    public class SizeCBViewComponent : ViewComponent
    {
        private readonly Cf2Context _context;

        public SizeCBViewComponent(Cf2Context context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string selectedSizeId = null)
        {
            var sizess = await _context.Sizes
          .Where(size => size.MaSize != "SZ001")
          .ToListAsync();
            ViewBag.DefaultSizeId = selectedSizeId ?? "SZ001";



            return View(sizess); // Trả về danh sách kích thước
        }
    }
}
