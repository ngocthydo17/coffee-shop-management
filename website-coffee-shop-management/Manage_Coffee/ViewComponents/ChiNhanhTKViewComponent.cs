using Manage_Coffee.Models;
using Microsoft.AspNetCore.Mvc;

namespace Manage_Coffee.ViewComponents
{
    public class ChiNhanhTKViewComponent : ViewComponent
    {
        private readonly Cf2Context _context;

        public ChiNhanhTKViewComponent(Cf2Context context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync(string selectedSizeId = null)
        {
            var cn = await Task.Run(() => _context.ChiNhanhs.ToList());
            ViewBag.DefaultSizeId = selectedSizeId ?? "CN001";
            return View(cn);
        }
    }
}
