using Manage_Coffee.Models;
using Microsoft.AspNetCore.Mvc;

namespace Manage_Coffee.ViewComponents
{
    public class DuongViewComponent : ViewComponent
    {
        private readonly Cf2Context _context;
        public DuongViewComponent(Cf2Context context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var duong = await Task.Run(() => _context.Duongs.ToList());
            return View(duong);
        }
    }
}
