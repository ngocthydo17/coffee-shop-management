using Manage_Coffee.Models;
using Microsoft.AspNetCore.Mvc;

namespace Manage_Coffee.ViewComponents
{
    public class DaViewComponent : ViewComponent
    {
        private readonly Cf2Context _context;
        public DaViewComponent (Cf2Context context)
        {
            _context = context; 
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var da = await Task.Run(() => _context.Das.ToList());
            return View(da);
        }
    }
}
